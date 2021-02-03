/*
 *  File Name : AlarmProcess.cs
 *  Author : Rajesh 
 *  @ PCC Technology Group LLC
 *  Created Date : 11/22/2010
 */
namespace CooperAtkins.NotificationClient.Service
{
    using System;
    using System.Net;
    using System.ServiceProcess;
    using System.Configuration;
    using System.Net.Sockets;
    using System.Threading;
    using System.Collections;
    using CooperAtkins.NotificationClient.Alarm;
    using CooperAtkins.NotificationClient.Generic;
    using CooperAtkins.NotificationClient.Generic.DataAccess;
    using CooperAtkins.Generic;
    using System.Diagnostics;
    using Microsoft.Win32;
    using EnterpriseModel.Net;


    public partial class NotificationClientService : ServiceBase
    {
        Socket _udpSocket = null;
        Thread _thdUdpHandler = null;
        AlarmInitializer _target = null;
        System.Timers.Timer timerConstructSensorObject = null;
        System.Timers.Timer timerProcessAlarmObject = null;
        System.Timers.Timer timerClearCompletedObject = null;
        //System.Timers.Timer timerMissedCommunication = null;
        public static string connString;
        public static string DBName;
        public static int OUID;
        public static string[] _args;
        

        public NotificationClientService()
        {
            InitializeComponent();
        }

        public NotificationClientService(string instanceDBConnString, string dbName, int oUID)
        {
            connString = instanceDBConnString;
            DBName = dbName;
            OUID = oUID;
            InitializeComponent();
        }

        public void Start(string[] args)
        {
            OnStart(args);
        }

        protected override void OnStart(string[] args)
        {
            _args = args;
            /* process initiation. */
            var serviceStarter = new Thread(StartService);
            serviceStarter.IsBackground = true;
            serviceStarter.Start();
        }

        private void StartService()
        {
            if (connString.ToStr() == string.Empty)
                connString = ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString;
                connString = connString.Remove(0, connString.IndexOf(";") + 1);
                connString = connString.Replace("MultipleActiveResultSets=true;", "");    

            EnterpriseModelConfig config = new EnterpriseModelConfig(connString);

            bool connectionOpened = false;
            int lastAttemptMins = 30 * 1000;
            while (!connectionOpened)
            {
                DataAccess dataAccess = null;
                try
                {
                    dataAccess = config.GetDbGateway();
                    dataAccess.Initialize();
                    dataAccess.Connection.Open();
                    connectionOpened = true;
                    initializeProcess.isDBConnected = true;
                    dataAccess.Connection.Close();
                }
                catch
                {
                    LogBook.Write("Unable to open database connection, SqlConnectionString:" + connString);
                    LogBook.Write("Attempting to connect after:" + (lastAttemptMins / 1000).ToString() + " Seconds");
                    Thread.Sleep(lastAttemptMins);
                    lastAttemptMins += lastAttemptMins;
                }
            }

            /* if the the process type is not a sub process */
            if (_args != null && _args.Length == 0)
                initializeProcess.InitProcess(_args);


            if (initializeProcess.DefaultConnectionString.ToStr() != string.Empty)
                connString = initializeProcess.DefaultConnectionString;

            config = new EnterpriseModelConfig(connString);

            EnterpriseModel.Net.LibConfig.Instance.Config = config;

            if (DBName.ToStr() == string.Empty)
                DBName = initializeProcess.firstDBName;

            if (DBName.ToStr() == string.Empty)
                DBName = "Intelliware";

            //Program.
            //Check if the old notification service is running
            LogBook.SetAppName(DBName);

            string logPath = GetRegValue("LogFilePath", Common.TEMP_TRAK_REG_KEY).ToStr();
            
            /*in case if it returns empty*/
            if (logPath.Trim() == string.Empty)
            {
                //logPath = @"C:\log.txt";
                logPath = @"C:\Program Files (x86)\TempTrak\LogFiles";
            }

            if (OUID == 0)
                logPath = logPath + @"\IWNotify.log";
            else
                logPath = logPath + @"\IWNotify-" + OUID + ".log";

            LogBook.SetLogFilePath(logPath);
            LogBook.Write("LogPath: " + logPath);
           /* LogBook.Write("Checking the Old version is running");
            string serviceName = "Intelli-Ware Notification Service";
            //set the time out for the service to stop
            int timeoutMilliseconds = 500;

            ServiceController service = new ServiceController(serviceName);

            try
            {

                System.TimeSpan timeout = System.TimeSpan.FromMilliseconds(timeoutMilliseconds);
                //if the service is running stop the service, else write a log to know the service status
                if (service.Status == ServiceControllerStatus.Running)
                {
                    LogBook.Write("Old version is running");
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                    LogBook.Write("Old version is stopped");
                }
                else
                { LogBook.Write("Old version is not running"); }
            }
            catch (Exception ex)
            {
                //log the error if an error occurs while stopping the old notification service
                LogBook.Write("Exception occurred while checking the old service status.");
                LogBook.Write(ex.StackTrace);
            }
            finally { service.Dispose(); }
            */
            LogBook.Write("New Service Starting....");
            InitProcess();
        }

        private void InitProcess()
        {
            LogBook.Write("SqlConnectionString:" + connString);

            _target = new AlarmInitializer();

            LogBook.Write("new AlarmInitializer called");

            try
            {
                /* Added on 02/28/2011 To set server time */
                _target.SetServerTime();
            }
            catch (Exception ex)
            {
                LogBook.Write("Unable to set server time to message boards", ex, "CooperAtkins.NotificationClient.Service.NotificationClientService");
            }

            /* if service restarts, this operation will resumes previously lost notification*/
            _target.ResumeNotificationProcess();

            LogBook.Write("ResumeNotificationProcess Completed");

            /* Stating the process. */
            StartProcess();

            LogBook.Write("StartProcess(); Completed");

            /* listening to external source. */
            UDPListener();
            LogBook.Write("UDPListener(); Completed");
        }


        private void StartProcess()
        {
            /* Changed back to 5 per revert source due to missed communication notifications not working properly
            * Commented By: Srinivas Rao E
            * Data: 2/2/02/2012
            * Description: Timers runs for every 3 seconds to get the latest infomration from the database and to process the alarms.
            */
            /* Calling ConstructSensorObjects method to add new notifications to alarm queue.*/
            timerConstructSensorObject = new System.Timers.Timer(1000 * 5);
            timerConstructSensorObject.Elapsed += new System.Timers.ElapsedEventHandler(timerConstructSensorObjects_Elapsed);
            timerConstructSensorObject.Start();

            /* Calling ProcessAlarmObjects method to process the queued objects. */
            timerProcessAlarmObject = new System.Timers.Timer(1000 * 1);
            timerProcessAlarmObject.Elapsed += new System.Timers.ElapsedEventHandler(timerProcessAlarmObjects_Elapsed);
            timerProcessAlarmObject.Start();

            timerClearCompletedObject = new System.Timers.Timer(1000 * 1);
            timerClearCompletedObject.Elapsed += new System.Timers.ElapsedEventHandler(timerClearCompletedObject_Elapsed);
            timerClearCompletedObject.Start();

            ///*Missed Communication*/
            //timerMissedCommunication = new System.Timers.Timer(1000 * 5 * 60); //Replace with 1000 * 60 * 15
            //timerMissedCommunication.Elapsed += new System.Timers.ElapsedEventHandler(timerMissedCommunication_Elapsed);
            //timerMissedCommunication.Start();

            /*IVR notification process.*/
            //if a shutdown occured while calls were in-process, clear the inprocess flag so they can resume
            IVRClearInProcessFlags clearInProcessFlags = new IVRClearInProcessFlags();
            clearInProcessFlags.Execute();

            var ivrProcess = new Thread(IvrNotification);
            ivrProcess.IsBackground = true;
            ivrProcess.SetApartmentState(ApartmentState.MTA); //was sta, i now added threading
            ivrProcess.Start();

            /*digital page Notification process.*/
            var threadDigitalPage = new Thread(ProcessDigitalPage);
            threadDigitalPage.IsBackground = true;
            threadDigitalPage.Start();

            /*sms Notification process.*/
            var threadSms = new Thread(ProcessSms);
            threadSms.IsBackground = true;
            threadSms.Start();

        }


        private void ProcessDigitalPage()
        {
            Thread.Sleep(5000);
            while (true)
            {
                _target.ProcessDigitalPager();
                //added on 02/17/2012 Pradeep I
                //to execute current conditional loop every 5 seconds instead of running continuously
                Thread.Sleep(5000); 
            }
        }
        private void ProcessSms()
        {
            Thread.Sleep(5000);
            while (true)
            {
                _target.ProcessSms();
                //added on 02/17/2012 Pradeep I
                //to execute current conditional loop every 5 seconds instead of running continuously
                Thread.Sleep(5000);
            }
        }
        private void IvrNotification()
        {
            Thread.Sleep(5000);

           //ProcessThreadCollection threadlist = ivrProcess.Threads;
            while (true)
            {
                _target.ProcessIVRNotification();
                //added on 02/17/2012 Pradeep I
                //to execute current conditional loop every 5 seconds instead of running continuously
                Thread.Sleep(5000); 
            }
        }


        /* Uncommented 5/9/2012 by Mike R. due to missed communication notifications failing
         * Commented By: Srinivas Rao E
         * Data: 2/2/02/2012
         * Description: Timers runs every 3 seconds, so there is no need to listen from external source.
        */
        /// <summary>
        /// To listen form external sources 
        /// </summary>
        private void UDPListener()
        {

            /* listing to 11353 port.*/
            int port = ConfigurationManager.AppSettings["UDPListenerPort"].ToInt();
            _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            _udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _udpSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            ThreadStart thdstHandler = new ThreadStart(HandleThread);

            _thdUdpHandler = new Thread(thdstHandler);
            _thdUdpHandler.IsBackground = true;
            _thdUdpHandler.Start();
        }

        /// <summary>
        /// check for the data received from the external source, if 
        /// received construct the alarm object and process them
        /// </summary>
        private void HandleThread()
        {


            while (true)
            {
                if (_udpSocket == null)
                    break;
                try
                {
                    byte[] bytes = new byte[1024];
                    _udpSocket.Receive(bytes);

                    string s = System.Text.ASCIIEncoding.ASCII.GetString(bytes);
                    LogBook.Write("Received data from external source, doing update check, data: " + s.Replace("\0",""));

                    //doing update check.
                    _target.ConstructSensorObjects();
                    _target.ProcessAlarmObjects();
                    Thread.Sleep(1000);
                }
                catch { }
            }

        }

        void timerClearCompletedObject_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                timerClearCompletedObject.Stop();
                _target.AlarmGarbageCollector();
                timerClearCompletedObject.Start();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                timerClearCompletedObject.Start();
            }
        }

        void timerConstructSensorObjects_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                /* Pause the timer till the process is completed. */
                timerConstructSensorObject.Stop();

                /*Processing new notifications and adding them to queue.*/
                _target.ConstructSensorObjects();

                /* Start the timer once ConstructSensorObjects process completed. */
                timerConstructSensorObject.Start();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                timerConstructSensorObject.Start();
            }
        }

        void timerProcessAlarmObjects_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                /* Pause the timer till the process is completed. */
                timerProcessAlarmObject.Stop();

                /*Processing the queued objects.*/
                _target.ProcessAlarmObjects();

                /* Start the timer once ConstructSensorObjects process completed. */
                timerProcessAlarmObject.Start();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                timerProcessAlarmObject.Start();
            }
        }

        //void timerMissedCommunication_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    try
        //    {
        //        /* Pause the timer till the process is completed. */
        //        timerMissedCommunication.Stop();
        //        _target.MissedCommunicationNotification();
        //        /* Start the timer once process completed. */
        //        timerMissedCommunication.Start();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        timerMissedCommunication.Start();
        //    }
        //    LogBook.Write("timerMissedCommunication.Start(); Called");
        //}
        /// <summary>
        /// close the alarm initializer
        /// </summary>
        /// 

        public void StopService()
        {
            OnStop();
        }

        protected override void OnStop()
        {
            int pid = Process.GetCurrentProcess().Id;
            LogBook.Write("Stopping exe's");
            LogBook.Write(Process.GetCurrentProcess().ProcessName);
            LogBook.Write("Master Connection String: " + ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString);
            foreach (Process pro in Process.GetProcessesByName("CooperAtkins.NotificationClient.Service"))
            {
                if (pro.Id != pid)
                {
                    try
                    {
                        ServiceEntity svcEntity = new ServiceEntity();
                        svcEntity.PID = pro.Id;
                        svcEntity.ExeName = ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString;
                        using (ServiceEntityContext context = new ServiceEntityContext())
                        {
                            LogBook.Write("Executing update for process id: " + pro.Id.ToString());
                            context.Save(svcEntity, EnterpriseModel.Net.ObjectAction.Edit);
                        }
                        LogBook.Write("Stopping process: " + pro.Id);
                        pro.Kill();
                    }
                    catch (Exception ex)
                    {
                        LogBook.Write(ex, "Notification Client Service");
                    }
                }
            }

            /* Uncommented by 5/9/2012 by Mike R. due to missed comm notification problems
             * Commented By: Srinivas Rao E
             * Data: 2/2/02/2012
             * Description: Timers runs every 3 seconds, so there is no need to listen from external source.
             */
            if (_udpSocket != null)
            {
                try
                {
                    _udpSocket.Close();
                    _udpSocket.Dispose();
                }
                catch { }
                finally
                {
                    _udpSocket = null;
                }
                try
                {
                    _thdUdpHandler.Abort();
                }
                catch (ThreadAbortException) { }


            }

            try
            {
                _target.Close();
            }
            catch (Exception ex)
            {
                LogBook.Write(ex, "Notification Client Service");
            }



        }

        /// <summary>
        /// Return Registry Key value based on key name
        /// </summary>
        /// <param name="regKeyName"></param>
        /// <returns></returns>
        public static object GetRegValue(string regKeyName, string registryPath)
        {
            object regKeyValue = null;
            RegistryKey regkey;
            regkey = Registry.LocalMachine.OpenSubKey(registryPath);
            try
            {
                if (regkey != null)
                    regKeyValue = regkey.GetValue(regKeyName);
            }
            catch (Exception ex) {  /*Write Log*/    }
            return regKeyValue;
        }
    }
}


