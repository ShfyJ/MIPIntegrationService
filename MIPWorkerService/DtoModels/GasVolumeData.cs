using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPWorkerService.DtoModels
{
    public class GasVolumeData
    {
        public DateTime TimeStamp { get; set; }
        public double DifferentialPressure { get; set; }
        public double Pressure { get; set; }
        public double Temperature { get; set; }
        public double Volume { get; set; }
    }
}
