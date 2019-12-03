using Autofac;
using CrosscuttingConcernCollection.Base;
using CrosscuttingConcernCollection.Logging;
using CrosscuttingConcernCollection.Monitoring;
using DependencyResolution;
using server.EntityCollection;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using System.Threading;

namespace server
{
    class Program
    {
        private static Thread _listener = null;

        public static void Main(string[] args)
        {
            #region Run Only One Instance 

            bool isAppInstanceShut;
             
            var mutex = new Mutex(true, "CustomerTracking", out isAppInstanceShut);

            if (!isAppInstanceShut)
            {
                return;
            }

            #endregion

            #region Run Application As Administrator

            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

            var administrativeMode = principal.IsInRole(WindowsBuiltInRole.Administrator);

            if (!administrativeMode)
            {
                var startInfo = new ProcessStartInfo();
                startInfo.Verb = "runas";
                startInfo.FileName = Assembly.GetExecutingAssembly().CodeBase;

                try
                {
                    Process.Start(startInfo);
                    return;
                }
                catch
                {
                    //User denied access
                    return;
                }
            }

            #endregion

            #region IoC registration

            Injector.Configure(() =>
            {
                var builder = new ContainerBuilder(); 
                builder.RegisterType<ApplicationLogger>().As<ILog>();
                builder.RegisterType<ConsoleMonitor>().As<IMonitorable>();
                builder.RegisterType<CrosscuttingConcernCollection.ExceptionHandling.ApplicationException>().As<IApplicationException>();

                return builder.Build(); 
            });

            #endregion

            #region Monitor Messages

            Console.WriteLine("Live Chat Service is running. Date : " + DateTime.Now);
            Console.WriteLine("*** Welcome to Live Chat Service ***");
            Console.WriteLine("Please do not press any key to stop live chat service");
            Console.WriteLine("Connected clients listed below");
            Console.WriteLine("------------------------------------");

            #endregion

            #region Async Socket Thread Setup
 
            if (_listener == null || !_listener.IsAlive)
            {
                var asynchronousSocket = new AsynchronousSocket();
                _listener = new Thread(() => asynchronousSocket.Listen())
                {
                    IsBackground = true
                };
                _listener.Start();
            }

            #endregion

            Console.ReadLine();
        } 
    }
}