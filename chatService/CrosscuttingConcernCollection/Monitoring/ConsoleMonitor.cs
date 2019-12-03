using CrosscuttingConcernCollection.Base;
using CrosscuttingConcernCollection.Helper;
using System;

namespace CrosscuttingConcernCollection.Monitoring
{
    public class ConsoleMonitor : IMonitorable
    {
        public void Display(string data)
        {
            if (data.IsNotNullOrEmpty())
            {
                Console.WriteLine(data);
            }
        }
    }
}
