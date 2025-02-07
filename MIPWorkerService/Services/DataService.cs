using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using MIPSharedLibrary.Constants;
using MIPSharedLibrary.Models;
using MIPWorkerService.Configuration;
using MIPWorkerService.DtoModels;
using Polly;

namespace MIPWorkerService.Services
{
    public class DataService(DatabaseConfig databaseConfig, ILogger<DataService> logger) : IDataService
    {
        private readonly DatabaseConfig _databaseConfig = databaseConfig;
        private readonly ILogger<DataService> _logger = logger;

        public async Task<(SentDataModel?, DataInfoType)> FetchDataAsync(int deviceId, DateTime requestedDate, DataInfoType type)
        {
            // Validate inputs
            if (deviceId <= 0 || requestedDate == default)
            {
                _logger.LogWarning("Invalid input parameters: DeviceId: {DeviceId}, RequestedDate: {RequestedDate}", deviceId, requestedDate);
                return (null, type);
            }

            const string gasVolumeQuery = @"
                SELECT TimeStamp, DifferentialPressure, Pressure, Temperature, Volume 
                FROM GasVolume
                WHERE DeviceId = @DeviceId AND Timestamp = @RequestedDate";

            const string deviceQuery = @"
                SELECT PointCode, DeviceSerialNumber,deviceModel, MeterUnitType 
                FROM Device
                WHERE DeviceId = @DeviceId";

            await using var connection = new SqlConnection(_databaseConfig.SourceConnectionString);

            try
            {
                // Execute both queries in one database session using QueryMultipleAsync
                await connection.OpenAsync();
                using var multiQuery = await connection.QueryMultipleAsync($"{gasVolumeQuery};{deviceQuery}", new { DeviceId = deviceId, RequestedDate = requestedDate });

                // Fetch GasVolume data
                var gasVolumeData = (await multiQuery.ReadAsync<GasVolume>()).ToList();

                // Fetch Device data
                var deviceData = await multiQuery.ReadFirstOrDefaultAsync<Device>();

                // Check for missing data
                if (!gasVolumeData.Any())
                {
                    _logger.LogWarning("No GasVolume data found for DeviceId: {DeviceId} and RequestedDate: {RequestedDate}", deviceId, requestedDate);
                    return (null, type);
                }

                if (deviceData == null)
                {
                    _logger.LogWarning("No Device data found for DeviceId: {DeviceId}", deviceId);
                    return (null, type);
                }

                // Create the SentDataModel object
                var sentData = new SentDataModel
                {
                    DeviceId = deviceId,
                    RequestedDate = requestedDate,
                    ValuesCount = gasVolumeData.Count,
                    PointCode = deviceData.PointCode,
                    DeviceSerialNumber = deviceData.DeviceSerialNumber,
                    DeviceType = deviceData.DeviceModel, 
                    MeterUnitType = deviceData.MeterUnitType,
                    Values = gasVolumeData.Select(g => new GasVolumeData
                    {
                        TimeStamp = g.Timestamp,
                        DifferentialPressure = g.DifferentialPressure,
                        Pressure = g.Pressure,
                        Temperature = g.Temperature,
                        Volume = g.Volume
                    }).ToList()
                };

                return (sentData, type);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error occurred while fetching data for DeviceId: {DeviceId} and RequestedDate: {RequestedDate}", deviceId, requestedDate);
                return (null, type); // Gracefully handle database errors
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching data for DeviceId: {DeviceId} and RequestedDate: {RequestedDate}", deviceId, requestedDate);
                return (null, type); // Gracefully handle unexpected errors
            }
        }

        private async Task<(SentDataModel?, DataInfoType)> FetchDataWithRetryAsync(int deviceId, DateTime requestedDate, DataInfoType type)
        {
            var retryPolicy = Policy
                .Handle<SqlException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt * 2),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception, "Retry {RetryCount} for DeviceId: {DeviceId}", retryCount, deviceId);
                    });

            return await retryPolicy.ExecuteAsync(() => FetchDataAsync(deviceId, requestedDate, type));
        }

        public async Task SaveDataAsync(SentData sentData)
        {
            if (sentData == null)
            {
                _logger.LogWarning("SentData is null. Skipping the save operation.");   
                return;
            }

            await using var connection = new SqlConnection(_databaseConfig.TargetConnectionString);

            var query = @"INSERT INTO SentData (CorrelationId, Data, SentDate, DataInfo, IsSuccessful, Response) 
                      VALUES (@CorrelationId, @Data, @SentDate, @DataInfo, @IsSuccessful, @Response)";

            try
            {
                await connection.ExecuteAsync(query, sentData);
                _logger.LogInformation("Successfully inserted SentData with CorrelationId: {CorrelationId}", sentData.CorrelationId);
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Exception is thrown while inserting into Target Database. Error: {ex.Message}");
                _logger.LogError(ex, "Transaction failed while inserting data into the Target Database.");
            }
            
        }
    }
}
