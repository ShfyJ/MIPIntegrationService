using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPWorkerService.DtoModels
{
    public class SentDataModel
    {
        public int DeviceId { get; set; }
        public DateTime RequestedDate { get; set; }
        public int ValuesCount { get; set; }
        public string PointCode { get; set; }
        public string DeviceSerialNumber { get; set; }
        public string DeviceType { get; set; }  //deviceModel property of Device table in asodu DB
        public int MeterUnitType { get; set; }
        public List<GasVolumeData> Values { get; set; }
    }
}
