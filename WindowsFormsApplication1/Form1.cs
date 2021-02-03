/*
 *  File Name : Form1.cs
 *  Author : Vasu
 *  @ PCC Technology Group LLC
 *  Created Date : 11/30/2010
 *  Description: To test the service.
 */

namespace WindowsFormsApplication1
{
    using System;
    using System.Windows.Forms;
    using CooperAtkins.NotificationClient.Alarm;
    using System.Configuration;
    using System.Reflection;
    using System.Net.Sockets;
    using System.Net;
    using System.Threading;
    using System.Collections;
    using CooperAtkins.Generic;
    using CooperAtkins.NotificationClient.Generic.DataAccess;

    public partial class Form1 : Form
    {
        ArrayList alSockets = new ArrayList();
        AlarmInitializer target = null;
        System.Timers.Timer timerConstructSensorObject = null;
        System.Timers.Timer timerProcessAlarmObject = null;
        System.Timers.Timer timerClearCompletedObject = null;
        System.Timers.Timer timerMissedCommunication = null;

        System.Timers.Timer timerIvrNotification = null;

        public Form1()
        {
            InitializeComponent();
        }
        static System.Timers.Timer timerStartProcess = null;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main1()
        {

            timerStartProcess = new System.Timers.Timer(1000 * 1);
            timerStartProcess.Elapsed += new System.Timers.ElapsedEventHandler(timerStartProcess_Elapsed);
            timerStartProcess.Enabled = true;
            timerStartProcess.Start();
        }

        static void timerStartProcess_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timerStartProcess.Stop();
            timerStartProcess.Enabled = false;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Main1();
            /* process initiation. */
            var processInitializer = new Thread(InitProcess);
            processInitializer.IsBackground = true;
            processInitializer.Start();


        }

        private void InitProcess()
        {
            //Configure Enterprise Model Library
            EnterpriseModel.Net.LibConfig.Instance.Config = new CooperAtkins.NotificationClient.Generic.DataAccess
               .EnterpriseModelConfig(ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString);

            EnterpriseModelConfig config = new EnterpriseModelConfig(ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString);

            bool connectionOpened = false;
            int lastAttemptMins = 5 * 1000;
            while (!connectionOpened)
            {
                EnterpriseModel.Net.DataAccess dataAccess = null;
                try
                {
                    dataAccess = config.GetDbGateway();
                    dataAccess.Initialize();
                    dataAccess.Connection.Open();
                    connectionOpened = true;
                    dataAccess.Connection.Close();
                }
                catch
                {
                    LogBook.Write("Unable to open database connection, SqlConnectionString:" + "");
                    LogBook.Write("Attempting to connect after:" + (lastAttemptMins / 1000).ToString() + " Seconds");
                    Thread.Sleep(lastAttemptMins);
                    lastAttemptMins += lastAttemptMins;
                }
                finally
                {
                    if (dataAccess != null)
                        dataAccess.Dispose();
                }
            }

            target = new AlarmInitializer();

            try
            {
                //target.SetServerTime();

            }
            catch
            {

            }

            /* if user restarts the service, this operation will resumes previously lost notification*/
            target.ResumeNotificationProcess();

            button1_Click(null, null);

            UDPListener();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            //label3.Text = "Process Started";
            /* Calling ConstructSensorObjects method to add new notifications to alarm queue.*/
            timerConstructSensorObject = new System.Timers.Timer(1000 * 5);
            timerConstructSensorObject.Elapsed += new System.Timers.ElapsedEventHandler(timerConstructSensorObjects_Elapsed);
            timerConstructSensorObject.Start();

            /* Calling ProcessAlarmObjects method to process the queued objects. */
            timerProcessAlarmObject = new System.Timers.Timer(1000 * 5);
            timerProcessAlarmObject.Elapsed += new System.Timers.ElapsedEventHandler(timerProcessAlarmObjects_Elapsed);
            timerProcessAlarmObject.Start();

            timerClearCompletedObject = new System.Timers.Timer(1000 * 5);
            timerClearCompletedObject.Elapsed += new System.Timers.ElapsedEventHandler(timerClearCompletedObject_Elapsed);
            timerClearCompletedObject.Start();


            /*IVR Notification process.*/
            var t = new Thread(IvrNotification);
            t.IsBackground = true;
            t.SetApartmentState(ApartmentState.STA);
            t.Start();

            /*digital page Notification process.*/
            var threadDigitalPage = new Thread(ProcessDigitalPage);
            threadDigitalPage.IsBackground = true;
            threadDigitalPage.Start();

            var threadSms = new Thread(ProcessSms);
            threadSms.IsBackground = true;
            threadSms.Start();

        }

        private void ProcessSms()
        {
            Thread.Sleep(5000);
            while (true)
            {
                target.ProcessSms();
                Thread.Sleep(5000);
            }
        }
        private void ProcessDigitalPage()
        {
            Thread.Sleep(5000);
            while (true)
            {
                target.ProcessDigitalPager();
            }
        }


        private void IvrNotification()
        {
            Thread.Sleep(5000);

            //timerIvrNotification.Stop();
            while (true)
            {
                bool CDYNE_ACCOUNT = GenStoreInfo.GetInstance().CDYNE_ACCOUNT == "" ? false : true;
                target.ProcessIVRNotification();
                //target.DialNumber(); 

            }
        }

        void timerIvrNotification_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timerIvrNotification.Stop();
            target.ProcessIVRNotification();            
            timerIvrNotification.Start();
        }


        void timerClearCompletedObject_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            /* Pause the timer till the process is completed. */
            timerClearCompletedObject.Stop();

            /*Processing new notifications and adding them to queue.*/
            target.AlarmGarbageCollector();

            /* Start the timer once ConstructSensorObjects process completed. */
            timerClearCompletedObject.Start();
        }

        void timerConstructSensorObjects_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            /* Pause the timer till the process is completed. */
            timerConstructSensorObject.Stop();

            /*Processing new notifications and adding them to queue.*/
            target.ConstructSensorObjects();

            /* Start the timer once ConstructSensorObjects process completed. */
            timerConstructSensorObject.Start();

            SetControlPropertyValue(label1, "Text", "Alarm Queued Objects: " + AlarmQueue.AlarmObjectQueue.Count.ToString());
            SetControlPropertyValue(label2, "Text", "Alarm Current Processed Objects: " + AlarmQueue.CurrentProcessObjects.Count.ToString());
            SetControlPropertyValue(label4, "Text", "Latest Time: " + DateTime.Now.ToString());


            Application.DoEvents();

        }

        void timerProcessAlarmObjects_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            /* Pause the timer till the process is completed. */
            timerProcessAlarmObject.Stop();

            /*Processing the queued objects.*/
            target.ProcessAlarmObjects();

            /* Start the timer once ConstructSensorObjects process completed. */
            timerProcessAlarmObject.Start();
        }

        Socket _udpSocket = null;
        Thread _thdUdpHandler = null;
        /// <summary>
        /// To listen form external sources 
        /// </summary>

        private void UDPListener()
        {

            /* listing to 11353 port.*/
            int port = 11353;
            _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            _udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _udpSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            ThreadStart thdstHandler = new ThreadStart(HandleThread);

            _thdUdpHandler = new Thread(thdstHandler);
            _thdUdpHandler.IsBackground = true;
            _thdUdpHandler.Start();

        }

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
                    LogBook.Write("Received data from external source, doing update check, data: " + s.Replace("\0", ""));

                    //doing update check.
                    target.ConstructSensorObjects();
                    target.ProcessAlarmObjects();
                    Thread.Sleep(1000);
                }
                catch { }
            }

        }

        private void handlerThread()
        {
            while (true)
            {
                Socket handlerSocket = (Socket)alSockets[alSockets.Count - 1];
                byte[] bytes = new byte[1024];
                handlerSocket.Receive(bytes);
                string s = System.Text.ASCIIEncoding.ASCII.GetString(bytes);

                LogBook.Write("Received data from external source, doing update check, data: " + s.Replace("\0", ""));

                // doing update check.
                target.ConstructSensorObjects();
                target.ProcessAlarmObjects();


                handlerSocket = null;
                Thread.Sleep(1000);
            }
        }

        #region Helper Methods


        delegate void SetControlValueCallback(Control oControl, string propName, object propValue);
        private void SetControlPropertyValue(Control oControl, string propName, object propValue)
        {
            if (oControl.InvokeRequired)
            {
                SetControlValueCallback d = new SetControlValueCallback(SetControlPropertyValue);
                oControl.Invoke(d, new object[] { oControl, propName, propValue });
            }
            else
            {
                Type t = oControl.GetType();
                PropertyInfo[] props = t.GetProperties();
                foreach (PropertyInfo p in props)
                {
                    if (p.Name.ToUpper() == propName.ToUpper())
                    {
                        p.SetValue(oControl, propValue, null);
                    }
                }
            }
        }

        #endregion

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            target.Close();
        }
    }
}
