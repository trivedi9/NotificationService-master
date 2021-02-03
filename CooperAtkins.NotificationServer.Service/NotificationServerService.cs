/*
 *  File Name : AlarmProcess.cs
 *  Author : Rajesh 
 *  @ PCC Technology Group LLC
 *  Created Date : 11/22/2010
 */
namespace CooperAtkins.NotificationServer.Service
{
    using System;
    using System.Diagnostics;
    using System.ServiceProcess;
    using CooperAtkins.NotificationServer.NotifyEngine;
    using CooperAtkins.Generic;


    partial class NotificationServerService : ServiceBase
    {
        NotifyServerGateway _nsGateway = null;
        public NotificationServerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                /*starting the server to listen the notifications from client.*/
                _nsGateway = new NotifyServerGateway();
                _nsGateway.Open();
                _nsGateway.Start();
            }
            catch (Exception ex){
                if (!EventLog.SourceExists("Log"))
                {
                    EventLog.CreateEventSource("Log", "NotificationServer");
                }
                EventLog myLog = new EventLog();
                myLog.Source = "Log";

                // Write an informational entry to the event log.    
                myLog.WriteEntry(ex.Message + "\\nStackTrace: " + ex.StackTrace);

            }
        }

        protected override void OnStop()
        {
            try
            {
                /*stop the server.*/
                _nsGateway.Stop();
            }
            catch (Exception ex)
            {
                LogBook.Write(ex, "Notification Server Service");
            }
        }
    }
}
