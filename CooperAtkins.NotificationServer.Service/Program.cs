using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace CooperAtkins.NotificationServer.Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {

         //   #if DEBUG
         //   NotificationServerService service = new NotificationServerService();
         //   service.InitializeLifetimeService();
            
          //  Thread.Sleep(Timeout.Infinite);
          //  #else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new NotificationServerService() 
			};
            ServiceBase.Run(ServicesToRun);
           // #endif
        }
    }
}


