using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPWorkerService.DtoModels
{
    public class GasVolume
    {
        public int DeviceId { get; set; }
        public DateTime Timestamp { get; set; }
        public double DifferentialPressure { get; set; }
        public double Pressure { get; set; }
        public double Temperature { get; set; }
        public double Volume { get; set; }
    }
}
