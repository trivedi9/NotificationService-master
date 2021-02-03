using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Configuration;
using Microsoft.Win32;
using EnterpriseModel.Net;
using CooperAtkins.NotificationClient.Generic.DataAccess;
using System.Windows.Forms;
using CooperAtkins.Generic;
using System.Threading;

namespace CooperAtkins.NotificationClient.Service
{
    static class Program
    {

        static NotificationClientService _clientService = null;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
#if DEBUG
            if (args.Length == 0)
            {
                initializeProcess.UpdateConfig();

                NotificationClientService service = new NotificationClientService();

                service.Start(null);
                Thread.Sleep(Timeout.Infinite);
            }
            else
            {
                NewInstanceMain(args[0], args[1], args[2].ToInt());
                Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
                Application.Run();
            }

#else
            if (args.Length == 0)
            {
                initializeProcess.UpdateConfig();

                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
			        { 
				        new NotificationClientService() 
			        };
                ServiceBase.Run(ServicesToRun);

            }
            else
            {
                NewInstanceMain(args[0], args[1], args[2].ToInt());
                Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
                Application.Run();
            }

#endif
        }

        /// <summary>
        /// method to explicitly invoke the service for multiple databases
        /// </summary>
        /// <param name="connString"></param>
        /// <param name="DBName"></param>
        private static void NewInstanceMain(string connString, string DBName, int oUID)
        {
            NotificationClientService service = new NotificationClientService(connString, DBName, oUID);
            service.Start(null);
        }

        static void Application_ApplicationExit(object sender, EventArgs e)
        {
            if (_clientService != null)
            {
                _clientService.StopService();
                _clientService.Dispose();
            }
        }

        //public static void LogData(string str)
        //{
        //    try
        //    {
        //        System.IO.StreamWriter sw = new System.IO.StreamWriter("C:\\templog.txt", true);
        //        sw.WriteLine(str);
        //        sw.Flush();
        //        sw.Close();
        //        sw.Dispose();
        //    }
        //    catch { }
        //}

        //static void timerStartProcess_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    timerStartProcess.Stop();
        //    timerStartProcess.Enabled = false;
        //    ConfigurationManager.RefreshSection("connectionStrings");
        //    DataAccess dataAccess = new EnterpriseModelConfig(ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString).GetDbGateway();
        //    bool connectionOpened = false;
        //    int lastAttemptMins = 30 * 1000;
        //    while (!connectionOpened)
        //    {
        //        try
        //        {
        //            dataAccess.Initialize();
        //            dataAccess.Connection.Open();
        //            connectionOpened = true;
        //            dataAccess.Connection.Close();
        //            isDBConnected = true;
        //        }
        //        catch
        //        {
        //            LogBook.Write("Unable to open database connection, SqlConnectionString:" + "");
        //            LogBook.Write("Attempting to connect after:" + (lastAttemptMins / 1000).ToString() + " Seconds");
        //            Thread.Sleep(lastAttemptMins);
        //            lastAttemptMins += lastAttemptMins;
        //        }
        //        finally
        //        {
        //            if (dataAccess != null)
        //                dataAccess.Dispose();
        //        }
        //    }

        //}




    }
}

