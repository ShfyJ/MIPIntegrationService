using MIPSharedLibrary.Constants;
using MIPSharedLibrary.Models;
using MIPWorkerService.Services;

namespace MIPWorkerService.Workers;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IDataService _dataService;
    private readonly IHttpService _httpService;

    public Worker(ILogger<Worker> logger, IDataService dataService, IHttpService httpService)
    {
        _logger = logger;
        _dataService = dataService;
        _httpService = httpService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            var reqDate = DateTime.Now.AddMonths(-2).Date;
            var dataType = DataInfoType.DailyData;
            var resultData = await _dataService.FetchDataAsync(1003, reqDate, dataType);
            var dtl = DateTime.Now.AddDays(20).ToString();
            var consumers = new List<string> { };
            var result = await _httpService.PostVolumeDataAsync(resultData.Item1, dtl, consumers);

            if (result.Success)
            {
                await _dataService.SaveDataAsync(new SentData(result.Data.CorrelationId, result.Data.JsonContent, DateTime.Now, 
                    resultData.Item2, result.Data.Success, result.Data.Message));
                
            }

            await Task.Delay(1000, stoppingToken);
            break;
            
        }
    }
}
