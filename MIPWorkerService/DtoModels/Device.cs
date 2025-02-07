using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPWorkerService.DtoModels
{
    public class Device
    {
        public int DeviceId { get; set; }
        public string PointCode { get; set; }
        public string DeviceSerialNumber { get; set; }
        public string DeviceModel { get; set; } 
        public int MeterUnitType { get; set; }
    }
}
