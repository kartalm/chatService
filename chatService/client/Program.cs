using Autofac;
using client.BusinessEntityCollection;
using CrosscuttingConcernCollection.Base;
using CrosscuttingConcernCollection.Logging;
using CrosscuttingConcernCollection.Monitoring;
using DependencyResolution;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using System.Threading;

namespace client
{
    class Program
    {
        private static Thread clientThread = null;
        private static AsynchronousClient _asynchronousClient;

        public static void Main(string[] args)
        { 
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

            #region Async Client Connect Thread Setup

            Thread.Sleep(1000);//prevent to be disconnected from chat server at app startup

            if (clientThread == null || !clientThread.IsAlive)
            {
                _asynchronousClient = new AsynchronousClient();
                clientThread = new Thread(() => _asynchronousClient.Connect())
                {
                    IsBackground = true
                };
                clientThread.Start();
            }

            #endregion

            Console.ReadLine();
        }
    }
}