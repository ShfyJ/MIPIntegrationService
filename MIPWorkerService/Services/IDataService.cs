using MIPSharedLibrary.Constants;
using MIPSharedLibrary.Models;
using MIPWorkerService.DtoModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPWorkerService.Services
{
    public interface IDataService
    {
        Task<(SentDataModel?, DataInfoType)> FetchDataAsync(int deviceId, DateTime requestedDate, DataInfoType type);
        Task SaveDataAsync(SentData sentData);
    }
}
