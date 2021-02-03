/*
 *  File Name : IVRNotificationCom.cs
 *  Author : Vasu Ravuri
 *  @ PCC Technology Group LLC
 *  Created Date : 01/03/2011
 */

namespace CooperAtkins.NotificationServer.NotifyEngine
{
    using System;
    using System.ComponentModel.Composition;
    using System.Collections;
    using System.Threading;
    using System.Windows.Forms;
    using Way2call.Driver;
    using SpeechLib;
    using CooperAtkins.Interface.NotifyCom;
    using CooperAtkins.Generic;


    [Export(typeof(INotifyCom))]
    public class IvrNotificationCom : INotifyCom, System.ComponentModel.ISynchronizeInvoke
    {
        /* Constants */
        const int WAIT_SECONDS = 60;
        const int USER_RESPONSE_WAIT_SECONDS = 30;

        /*static variables that are used in the way2call events.*/
        private static TTSVoice ttSVoice = null;
        private static NotifyComResponse notifyComResponse = null;
        private static Way2call.Driver.CWay2callDriver w2cDrv = null;
        private static Hashtable Action = new Hashtable();
        private static bool stopVoicePrompt = false;
        private static bool digitReceived = false;
        private static string sensorInfo = string.Empty;
        private static string phoneNumber = string.Empty;

        private INotifyObject _notifyObject = null;
        private bool isCallConnected = false;
        private System.Timers.Timer timerNoConnect = null;
        private System.Timers.Timer timerNoSilenceDetected = null;
        private DateTime ConnectedTime;


        /// <summary>
        /// Returns true if the process completed.
        /// </summary>
        public static bool IsDone;

        private bool _isInDebugMode = false;

        #region INotifyCom Members

        public NotifyComResponse Invoke(INotifyObject notifyObject)
        {
            NotifyComResponse response = new NotifyComResponse();
            object obj = new object();

            try
            {

                /* lock the current process to stop accessing from new process until the current process completed*/
                lock (obj)
                {
                    /* initialize the way2call device. */
                    if (!InitComponent())
                    {
                        response.IsError = true;
                        response.IsSucceeded = false;
                        response.ResponseContent = "Device Initialization failed";
                        return response;
                    }

                    _notifyObject = notifyObject;
                    LogBook.Write("IvrNotificationCom Invoke() Method.");

                    Thread.Sleep(5000);

                    /* Dialing to a phone number.*/
                    DialNumber();

                    int waitSecs = 0;

                    /*Wait and Check till process completed*/
                    while (true)
                    {
                        System.Windows.Forms.Application.DoEvents();
                        waitSecs++;

                        Thread.Sleep(100);

                        /*if call completed then come out of current process.*/
                        if (IsDone)
                        {
                            response = GetComResponse();
                            break;
                        }
                        /* wait for 1.5 minutes, still if there is no response then disconnect the call.*/
                        else if (waitSecs >= 900)
                        {
                            Hangup();
                            response.IsError = true;
                            response.IsSucceeded = false;
                            response.ResponseContent = "No response from the modem, forcefully hanged up.";

                            LogBook.Write("No response from the modem, forcefully hanging up.");
                        }

                        /*if there is no ResponseContent then update it.*/
                        if (response.ResponseContent == null)
                        {
                            response.ResponseContent = "No response.";
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                response.ResponseContent = "Error dialing the number, message: " + ex.Message;
                response.IsError = true;
                response.IsSucceeded = false;
                LogBook.Write("Error dialing the number, message", ex, "CooperAtkins.NotificationServer.NotifyEngine");
            }
            return response;
        }

        public void UnLoad()
        { }

        #endregion

        /// <summary>
        /// Initializing the component.
        /// </summary>
        public bool InitComponent()
        {
            LogBook.Write("Initializing the device.");
            try
            {
                /* If the component was configured, no need configure it again (it is a singleton object)  */
                if (w2cDrv != null)
                {
                    return true;
                }
                else
                {

                    w2cDrv = new Way2call.Driver.CWay2callDriver();
                    /* adding required keys to handle the events which are fired continuously */
                    Action.Add("CallConnected", 0);
                    Action.Add("SilenceDetected", 0);
                }
            }
            catch (Exception ex1)
            {
                LogBook.Write("ex1 error: " + ex1.Message);
            }

            //w2cDrv = new Way2call.Driver.CWay2callDriver();

            /* if device configuration failed then return from the process. */
            if (!ConfigureDevice())
            {
                LogBook.Write("Unable to configure device.");
                return false;
            }



            CWay2callDriver.OnCallConnected += new CWay2callDriver.DelegateNoParams(CWay2callDriver_OnCallConnected);
            CWay2callDriver.OnSilentDetected += new CWay2callDriver.DelegateNoParams(CWay2callDriver_OnSilentDetected);
            CWay2callDriver.OnDTMF_KeyDown += new CWay2callDriver.DelegateOnDTMF_KeyPress(CWay2callDriver_OnDTMF_KeyDown);
            CWay2callDriver.OnCallProgressTimeOut += new CWay2callDriver.DelegateNoParams(CWay2callDriver_OnCallProgressTimeOut);
            CWay2callDriver.OnCallProgressBusyTone += new CWay2callDriver.DelegateNoParams(CWay2callDriver_OnCallProgressBusyTone);
            CWay2callDriver.OnCallProgressRingBackTone += new CWay2callDriver.DelegateNoParams(CWay2callDriver_OnCallProgressRingBackTone);
            CWay2callDriver.OnNativeDeviceEvent += new CWay2callDriver.DelegateNativeDeviceEvent(CWay2callDriver_OnNativeDeviceEvent);

            /*Configure the voice*/
            ConfigureVoice();
            return true;
        }

        void InitCall()
        {
            /* to hang-up call if the user did not respond.*/
            timerNoConnect = new System.Timers.Timer();
            timerNoConnect.Elapsed += new System.Timers.ElapsedEventHandler(timerNoConnect_Elapsed);
            timerNoConnect.Interval = WAIT_SECONDS * 1000;

            /* To handle answering machine, answering machine will continue the conversation without any silence.*/
            timerNoSilenceDetected = new System.Timers.Timer();
            timerNoSilenceDetected.Interval = 1 * 1000;
            timerNoSilenceDetected.Elapsed += new System.Timers.ElapsedEventHandler(timerNoSilenceDetected_Elapsed);



            LogBook.Write("NotificationDate: " + _notifyObject.NotificationData.ToString());
            LogBook.Write("PhoneNo: " + _notifyObject.NotifierSettings["PhoneNo"].ToString());

            /* assign message and phone number from client information. */
            sensorInfo = _notifyObject.NotificationData.ToString();
            phoneNumber = _notifyObject.NotifierSettings["PhoneNo"].ToString();

            notifyComResponse = new NotifyComResponse();
        }

        /* Event will fire when the remote phone start ringing */
        void CWay2callDriver_OnCallProgressRingBackTone(ushort DeviceID)
        {
            timerNoConnect.Start();
            WriteLog("Call Progress RingBack");
        }

        void CWay2callDriver_OnCallProgressBusyTone(ushort DeviceID)
        {
            WriteLog("Call Progress BusyTone");
            Hangup();
        }

        void CWay2callDriver_OnCallProgressTimeOut(ushort DeviceID)
        {
            WriteLog("Call Progress TimeOut");
            Hangup();
        }

        static void CWay2callDriver_OnDTMF_KeyDown(ushort DeviceID, CWay2callDriver.sDTMF_Key Key, CWay2callDriver.DTMF_Origin Source)
        {
            if (Action["CallConnected"].ToInt() == 0)
                return;//only when call is connected

            /* make flags to true stop playing the sensor information message. */
            stopVoicePrompt = true;
            digitReceived = true;

            /* update the response status. */
            notifyComResponse.ResponseContent += "\r\n Acknowledgement received, " + Key.kChar + " pressed.";
            notifyComResponse.IsSucceeded = true;
            WriteLog("Digit Received");
            WriteLog("\t" + "Digit: " + Key.kChar);

            /* playing message. */
            ttSVoice.SPVoice.Speak("Thank You   Good Bye   ", SpeechVoiceSpeakFlags.SVSFNLPSpeakPunc);
            ttSVoice.SPVoice.WaitUntilDone(System.Threading.Timeout.Infinite);

            Hangup();

        }
        static void CWay2callDriver_OnNativeDeviceEvent(ushort DeviceID, uint Event, uint EventData, uint EventDataEx, byte[] pEventBuffer)
        {
            /* hang up the call if it raises call busy or disconnected event. */
            if (w2cDrv.Device[DeviceID].EventDescription(Event, EventData).ToString().ToLower().Contains("busy"))
            {
                WriteLog("Call is Busy");
                WriteLog("Hanging Up");
                Hangup();
            }
            else if (w2cDrv.Device[DeviceID].EventDescription(Event, EventData).ToString().ToLower().Contains("disconnected"))
            {
                WriteLog("disconnected");
                WriteLog("Hanging Up");
                Hangup();
            }

            LogBook.Debug("DeviceID: " + DeviceID + ", Event:" + w2cDrv.Device[DeviceID].EventDescription(Event, EventData).ToString());

        }

        void CWay2callDriver_OnSilentDetected(ushort DeviceID)
        {
            /* Don't do any action as the call was not connected.*/
            if (!isCallConnected)
                return;

            /* silence may detect multiple times, don't do below action once they executed.*/
            if (Action["SilenceDetected"].ToInt() == 1)
                return;



            /* Mark as the below actions executed. */
            Action["SilenceDetected"] = 1;

            WriteLog("Silence Detected");

            /* stop silence detection timer. */
            timerNoSilenceDetected.Stop();

            if ((ConnectedTime - DateTime.Now).Seconds < 3)
            {
                WriteLog("Silence detected in less than 3 seconds after connection - Likely a human");
                PlayInitialGreeting();
            }
            else
            {
                WriteLog("Silence detected more than 3 seconds after connection - Likely an automated device");
                HandleAnsweringMachine();
            }
        }



        void CWay2callDriver_OnCallConnected(ushort DeviceID)
        {
            //event may be fired multiple times, don't execute below actions once they executed.
            if (Action["CallConnected"].ToInt() == 1)
                return;


            isCallConnected = true;

            /* Mark as the actions executed. */
            Action["CallConnected"] = 1;

            notifyComResponse.ResponseContent += "\r\nCall Connected";

            /*As the call connected, stop the timer.*/
            timerNoConnect.Stop();

            WriteLog("w2cDrv.OnConnected");
            ConnectedTime = DateTime.Now;

            WriteLog(@"Checking for automated device");

            /* start detecting the silence.*/
            timerNoSilenceDetected.Start();
        }

        /// <summary>
        /// Configures the way2call device.
        /// </summary>
        /// <returns></returns>
        private bool ConfigureDevice()
        {
            int iErr = 0;

            CWay2callDriver.Errors w2cErr = CWay2callDriver.Errors.SUCCESS;
            WriteLog("Initializing Way2call driver ...\n");

            w2cErr = (CWay2callDriver.Errors)w2cDrv.InitializeDriver(0);//must be called

            if (CWay2callDriver.Errors.SUCCESS != w2cErr)
            {
                WriteLog("Error opening the driver ...: " + w2cErr.ToString());
                iErr = w2cDrv.ShutdownDriver(0);
                return false;
            }

            // no device(s)...
            if (0 == w2cDrv.NumDevices)
            {
                WriteLog("There are no Hi-Phone devices connected to this PC.");
                iErr = w2cDrv.ShutdownDriver(0);
                return false;
            }

            WriteLog("Driver version: " + w2cDrv.Version.ToString() + "\n");

            //assume device #0 exists
            w2cErr = (CWay2callDriver.Errors)w2cDrv.Device.Open(0);//open the device

            if (CWay2callDriver.Errors.SUCCESS != w2cErr)
            {
                WriteLog("Error opening the device ...: " + w2cErr.ToString());
                iErr = w2cDrv.ShutdownDriver(0);//must be last called
                return false;
            }


            /* Silent detection for every 3 seconds */
            CWay2callDriver.CDevice.TONE_MONITOR_IDS iToneId = CWay2callDriver.CDevice.TONE_MONITOR_IDS.TONE_ID_00;

            //w2cDrv.Device.LocalDevice = CWay2callDriver.CDevice.LOCAL_DEVICE.Phone;

            w2cDrv.Device.ToneMonitor.Tone[iToneId].Duration = 3000;
            w2cDrv.Device.ToneMonitor.Tone[iToneId].Frequency1 = 0;
            w2cDrv.Device.ToneMonitor.Tone[iToneId].Frequency2 = 0;
            w2cDrv.Device.ToneMonitor.Tone[iToneId].Frequency3 = 0;
            w2cDrv.Device.ToneMonitor.Tone[iToneId].Enabled = true;

            w2cDrv.Device.ToneMonitor.Start();

            return true;
        }

        /// <summary>
        /// Dials the number.
        /// </summary>
        public void DialNumber()
        {
            LogBook.Write("Dialing the number");
            InitCall();

            LogBook.Write("InitCall() completed");
            LogBook.Write("PhoneNo. : " + phoneNumber + ", Voice Prompt: " + sensorInfo);

            /* reset previous flags */
            isCallConnected = false;
            IsDone = false;
            stopVoicePrompt = false;
            digitReceived = false;
            Action["CallConnected"] = 0;
            Action["SilenceDetected"] = 0;

            notifyComResponse = new NotifyComResponse();



            CWay2callDriver.Errors xError = (CWay2callDriver.Errors)w2cDrv.Device.Call("w" + phoneNumber, true, true);
            Thread.Sleep(2000);

            /* handling errors  */
            if (xError != CWay2callDriver.Errors.SUCCESS)
            {
                LogBook.Write("Call Error: " + xError.ToString());

                notifyComResponse.ResponseContent += "\r\n Call Error: " + xError.ToString();
                notifyComResponse.IsError = true;

                w2cDrv.Device.HangUp(false);

                /* update call status as not completed. */
                Action["CallConnected"] = 0;
            }
            else
            {
                /* no errors, dialing the number.*/
                WriteLog("Starting to dial " + phoneNumber);
            }
        }



        public static void Hangup()
        {
            /*Hang up the call.*/
            w2cDrv.Device.HangUp(false);
            WriteLog("Hanging Up");

            /*Update IsDone to true, to process next notification.*/
            IsDone = true;
        }


        private void timerNoConnect_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            /* Call not connected, hang up the call.*/
            timerNoConnect.Stop();
            WriteLog("No connection within " + WAIT_SECONDS.ToString() + " seconds...hanging up");
            Hangup();

        }

        void timerNoSilenceDetected_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            /* If no silence was detected, play the message may be it is a answering machine.  */
            timerNoSilenceDetected.Stop();
            Action["SilenceDetected"] = 1;
            WriteLog("Silence not detected in " + timerNoSilenceDetected.Interval.ToString() + " milliseconds");
            WriteLog(@"\t*** Likely an automated device ***");
            HandleAnsweringMachine();
        }

        public void HandleAnsweringMachine()
        {
            WriteLog("Handle Answering Machine");
            PlayInitialGreeting();
        }
        private void PlayInitialGreeting()
        {
            notifyComResponse.ResponseContent += "\r\n Playing message.";
            WriteLog("PlayInitialGreeting()");

            int speakCount = 0;

            /* Playing the message until we receive any digit press or till complete max tries.*/
            while (!stopVoicePrompt)
            {
                /* unmute local device */
                w2cDrv.Device.AudioControl.LocalDeviceToLine = CWay2callDriver.CDevice.CAudioControl.SIGNAL_SWITCH.UnMute;

                sensorInfo = sensorInfo.Replace(",", " ").Replace(".", " ");
                Application.DoEvents();

                /*Playing the voice.*/
                ttSVoice.SPVoice.Speak(sensorInfo, SpeechVoiceSpeakFlags.SVSFNLPSpeakPunc);
                ttSVoice.SPVoice.WaitUntilDone(System.Threading.Timeout.Infinite);


                /*wait for key press.*/
                int waitCount = 0;

                /* Using while loop to fire pending events, generally the key press of remote phone.*/
                /* waiting for 4 seconds between the message. */
                while (true)
                {
                    if (waitCount >= 4)
                        break;

                    /* Doing other events. */
                    Application.DoEvents();
                    Thread.Sleep(1000);

                    waitCount++;
                }

                /*Adding 4 seconds*/
                speakCount++;

                /* To wait for user response, if the answering machine answered the call the it will not give the response*/
                /*waiting for 40 seconds*/
                if (speakCount >= 4)
                {
                    WriteLog("No response found.");
                    stopVoicePrompt = true;
                }
            }

            /* update key press status */
            if (!digitReceived)
            {
                notifyComResponse.ResponseContent += "\r\n No Acknowledgement received, call disconnecting.";
                notifyComResponse.IsSucceeded = false;
            }
            else
            {
                notifyComResponse.IsSucceeded = true;
            }
            Hangup();
        }

        /// <summary>
        /// Configures the voice.
        /// </summary>
        private static void ConfigureVoice()
        {
            SpWaveFormatEx fFormat = new SpWaveFormatEx();
            ttSVoice = new TTSVoice();

            //ttSVoice.SPVoice.Voice = ttSVoice.SPVoice.GetVoices("gender=female", "").Item(0);

            StartService("AudioSrv", 20 * 1000);

            /* set the audio out to the telephony device*/
            ttSVoice.MMSysAudioOut.DeviceId = (int)w2cDrv.Device.Info.WaveOutDrvID;

            WriteLog("Sound DeviceId: " + w2cDrv.Device.Info.WaveOutDrvID.ToStr());

            fFormat.FormatTag = 1;
            fFormat.Channels = 1;
            fFormat.SamplesPerSec = 8000;
            fFormat.AvgBytesPerSec = 16000;
            fFormat.BlockAlign = 2;
            fFormat.BitsPerSample = 16;

            ttSVoice.MMSysAudioOut.Format.SetWaveFormatEx(fFormat);

            /* Prevent SAPI from changing the wave format when the device changes*/
            ttSVoice.SPVoice.AllowAudioOutputFormatChangesOnNextSet = false;
            ttSVoice.SPVoice.AudioOutputStream = ttSVoice.MMSysAudioOut;
        }

        public static void StartService(string serviceName, int timeoutMilliseconds)
        {
            System.ServiceProcess.ServiceController service = new System.ServiceProcess.ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
                if (service.Status == System.ServiceProcess.ServiceControllerStatus.Stopped)
                {
                    service.Start();
                    service.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Running, timeout);
                }

            }
            catch (Exception ex)
            {
                LogBook.Write("Unable to start service Windows audio, please start the service manually. Err:" + ex.Message);
            }
        }



        #region Helper


        /*Logs the information*/
        public static void WriteLog(string logText)
        {
            try
            {
                LogBook.Write(logText);
            }
            catch { }
        }


        #endregion


        /// <summary>
        /// Gets updated response.
        /// </summary>
        /// <returns></returns>
        internal NotifyComResponse GetComResponse()
        {
            return notifyComResponse;
        }


        #region ISynchronizeInvoke interface events


        public IAsyncResult BeginInvoke(Delegate method, object[] args)
        {
            return null;
        }

        public object EndInvoke(IAsyncResult result)
        {
            return null;
        }

        public object Invoke(Delegate method, object[] args)
        {
            return method.DynamicInvoke(args);
        }

        public bool InvokeRequired
        {
            get { return true; }
        }

        #endregion
    }
}
