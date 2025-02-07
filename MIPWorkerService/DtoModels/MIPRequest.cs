using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPWorkerService.DtoModels
{
    public class MIPRequest
    {
        public Guid CorrelationId { get; set; }
        public SentDataModel Data { get; set; }
        public string Dtl { get; set; } = string.Empty; // Date formatted as yyyy-MM-dd
        public List<string> DestinationSubscribers { get; set; }

        public MIPRequest(){

            CorrelationId = Guid.NewGuid();
            // Initializing the list and data to avoid null reference issues
            Data = new SentDataModel();
            DestinationSubscribers = [];
        }

    }
}
