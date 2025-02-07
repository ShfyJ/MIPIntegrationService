using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPWorkerService.Configuration
{
    public class DatabaseConfig(string sourceConnectionString, string targetConnectionString)
    {
        public string SourceConnectionString { get; } = sourceConnectionString;
        public string TargetConnectionString { get; } = targetConnectionString;
    }
}
