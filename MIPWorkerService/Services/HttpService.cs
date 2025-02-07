using Azure;
using MIPSharedLibrary.Models;
using MIPSharedLibrary.Utils;
using MIPWorkerService.Configuration;
using MIPWorkerService.DtoModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MIPWorkerService.Services
{
    public class HttpService(HttpClient httpClient, JwtTokenManager jwtTokenManager, 
        IConfiguration config, ILogger<HttpService> logger) : IHttpService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly JwtTokenManager _jwtTokenManager = jwtTokenManager;
        private readonly ILogger<HttpService> _logger = logger;
        private readonly string _mipPostDataEndpoint = config["ExternalService:PostDataUrl"]!;
        private readonly string _mipFetchSubsEndpoint = config["ExternalService:FetchSubscribersUrl"]!;
        private readonly string _mipFetchDlvSmryEndpoint = config["ExternalService:FetchDeliverySummaryUrl"]!;
        private readonly string _mipFetchReportEndpoint = config["ExternalService:FetchReportUrl"]!;

        public async Task FetchDeliverySummaryAsync(CancellationToken cancellationToken = default)
        {
            // Retrieve JWT token
            var jwtToken = _jwtTokenManager.GetToken();
            // Add Authorization header
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

            try
            {
                // Make the GET request
                _logger.LogInformation("Fetching data from {Endpoint}", _mipFetchDlvSmryEndpoint);
                var response = await _httpClient.GetAsync(_mipFetchDlvSmryEndpoint, cancellationToken);

                // Ensure successful response
                response.EnsureSuccessStatusCode();

                // Deserialize JSON response
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonConvert.DeserializeObject<object>(responseContent);

                Console.WriteLine($"Response content from Delivery Summary: {result}");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to {Endpoint} failed.", _mipFetchDlvSmryEndpoint);
                
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize response from {Endpoint}", _mipFetchDlvSmryEndpoint);
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching data from {Endpoint}", _mipFetchDlvSmryEndpoint);
            }

        }

        public async Task FetchReportAsync(Guid correlationId, CancellationToken cancellationToken = default)
        {
            // Retrieve JWT token
            var jwtToken = _jwtTokenManager.GetToken();
            // Add Authorization header
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

            try
            {
                // Make the GET request
                var endpoint = $"{_mipFetchReportEndpoint}/{correlationId}";
                _logger.LogInformation("Fetching data from {Endpoint}", endpoint);
                var response = await _httpClient.GetAsync(endpoint, cancellationToken);

                // Ensure successful response
                response.EnsureSuccessStatusCode();

                // Deserialize JSON response
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonConvert.DeserializeObject<object>(responseContent);

                if (result == null)
                    _logger.LogWarning("Deserialized response is null", endpoint);

                Console.WriteLine($"Response content from Report endpoint: {result}");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to {Endpoint} failed.", _mipFetchReportEndpoint);

            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize response from {Endpoint}", _mipFetchReportEndpoint);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching data from {Endpoint}", _mipFetchReportEndpoint);
            }

        }

        public async Task FetchSubscribersAsync(CancellationToken cancellationToken = default)
        {
            // Retrieve JWT token
            var jwtToken = _jwtTokenManager.GetToken();
            // Add Authorization header
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

            try
            {
                // Make the GET request
                _logger.LogInformation("Fetching data from {Endpoint}", _mipFetchSubsEndpoint);
                var response = await _httpClient.GetAsync(_mipFetchSubsEndpoint, cancellationToken);

                // Ensure successful response
                response.EnsureSuccessStatusCode();

                // Deserialize JSON response
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonConvert.DeserializeObject<object>(responseContent);

                Console.WriteLine($"Response content from Subscribers endpoint: {result}");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to {Endpoint} failed.", _mipFetchSubsEndpoint);

            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize response from {Endpoint}", _mipFetchSubsEndpoint);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching data from {Endpoint}", _mipFetchSubsEndpoint);
            }

        }

        public async Task<Result<HttpResponseData>> PostVolumeDataAsync(SentDataModel sentDataModel, string dtl, List<string> consumers)
        {

            // Retrieve JWT token
            var jwtToken = _jwtTokenManager.GetToken();
            // Add Authorization header
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

            // Create the request object
            var mipRequest = new MIPRequest
            {
                Data = sentDataModel,
                Dtl = DateTime.Parse(dtl).ToString("yyyy-MM-dd"), // Ensure dtl is in the correct format
                DestinationSubscribers = consumers
            };

            // Serialize the object to JSON
            var jsonContent = JsonConvert.SerializeObject(mipRequest);

            // Prepare the HTTP request
            var requestContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                // Send POST request
                var response = await _httpClient.PostAsync(_mipPostDataEndpoint, requestContent);

                if (response.IsSuccessStatusCode)
                {
                    // Log or process success
                    Console.WriteLine("Data successfully posted to MIP.");
                    var message = $"Data successfully posted to MIP. Status code: {response.StatusCode}; Message: {response.Content}";
                    return Result<HttpResponseData>.SuccessResponse(new HttpResponseData(jsonContent, mipRequest.CorrelationId, true, message));
                }
                else
                {
                    // Log or process failure
                    Console.WriteLine($"Failed message received from MIP. Status code: {response.StatusCode}");
                    var message = $"Failed message received from MIP. Status code: {response.StatusCode}; Message: {response.Content}";
                    return Result<HttpResponseData>.SuccessResponse(new HttpResponseData(jsonContent, mipRequest.CorrelationId, false, message));
                }

                
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Exception is thrown. Error: {ex.Message}");
                var message = $"Exception is thrown. Error: {ex.Message}";
                return Result<HttpResponseData>.ErrorResponse(message);
            }
        }
    }
}
