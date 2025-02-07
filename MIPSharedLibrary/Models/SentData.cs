using MIPSharedLibrary.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPSharedLibrary.Models
{
    public class SentData
    {
        [Key]
        public Guid CorrelationId { get; set; }
        public string Data { get; set; } = string.Empty; // Serialized JSON payload
        public DateTime SentDate { get; set; }
        public DataInfoType DataInfo { get; set; }
        public bool IsSuccessful { get; set; }
        public string Response { get; set; } = string.Empty; // Serialized JSON response from the external service

        public SentData(Guid correlationId, string data, DateTime sentDate, DataInfoType dataInfoType, 
            bool isSuccessfull, string response)
        {
            CorrelationId = correlationId;
            Data = data;
            SentDate = sentDate;
            DataInfo = dataInfoType;
            IsSuccessful = isSuccessfull;
            Response = response;
        }

        public SentData()
        {
            
        }
    }
}
