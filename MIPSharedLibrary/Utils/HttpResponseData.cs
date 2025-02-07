using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPSharedLibrary.Utils
{
    public class HttpResponseData(string jsonContent, Guid correlationId, bool success, string msg )
    {
        public string JsonContent = jsonContent;
        public Guid CorrelationId = correlationId;
        public bool Success { get; set; } = success;
        public string Message { get; set; } = msg;
    }
}
