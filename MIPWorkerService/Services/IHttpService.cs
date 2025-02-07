using MIPSharedLibrary.Utils;
using MIPWorkerService.DtoModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPWorkerService.Services
{
    public interface IHttpService
    {
        Task<Result<HttpResponseData>> PostVolumeDataAsync(SentDataModel sentDataModel, string dtl, List<string> consumers);
        Task FetchSubscribersAsync(CancellationToken cancellationToken = default);
        Task FetchDeliverySummaryAsync(CancellationToken cancellationToken = default);
        Task FetchReportAsync(Guid correlationId, CancellationToken cancellationToken = default);
    }
}
