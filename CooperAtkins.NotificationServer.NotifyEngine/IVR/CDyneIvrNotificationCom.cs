namespace CooperAtkins.NotificationClient.NotifyEngine
{
    using System;
    using System.ComponentModel.Composition;
    using System.Collections;
    using System.Threading;
    using System.Windows.Forms;

    using CooperAtkins.Interface.NotifyCom;
    using CooperAtkins.Generic;
    using NotificationServer.NotifyEngine.Notify;


    [Export(typeof(INotifyCom))]
    public class CDyneIvrNotificationCom : INotifyCom
    {
        /* Constants */
        const int WAIT_SECONDS = 60;
        const int USER_RESPONSE_WAIT_SECONDS = 30;

        /*static variables that are used in the way2call events.*/
     
        private static NotifyComResponse notifyComResponse = null;
  
        private static Hashtable Action = new Hashtable();
        private static bool stopVoicePrompt = false;
        private static bool digitReceived = false;
        private static string sensorInfo = string.Empty;
        private static string phoneNumber = string.Empty;
        private static string personName = string.Empty;
        private static string timeOutOfRange = string.Empty;
        private static string value = string.Empty;
        private static string deviceName = string.Empty;
        private static string isMissComm = string.Empty;
        private static string groupName = string.Empty;
        private static string alarmTime = string.Empty;
        private static string probe = string.Empty;
        private static string fromNumber = string.Empty;
        private static string IVRServiceResponseText = string.Empty;
        private static string DigitsPressed = string.Empty;
        private static DateTime StartTime;
        private static DateTime EndTime;
        private static long QueueID = -1;
        private static long AlarmID = -1;
        private static int TryCount = -1;
        private static bool CallAnswered = false;
        private static bool CallComplete = false;
        private static string MachineDetection = string.Empty;
        private static int Duration = -1;



        private INotifyObject _notifyObject = null;
        private bool isCallConnected = false;
        private long transactionIDreturned = -1;
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
               // lock (obj)
                {
            

                    _notifyObject = notifyObject;
                    LogBook.Write("CDyneIvrNotificationCom Invoke() Method.");

                    

                    /* Dialing to a phone number.*/
                   response.TransactionIDReturned = DialNumber();

           
                    
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
          //check to be sure we can access 3rd party webservice first
            //and that customer acct is active

         
         
            return true;
        }

        void InitCall()
        {
  


            LogBook.Write("NotificationDate: " + _notifyObject.NotificationData.ToString());
            LogBook.Write("PhoneNo: " + _notifyObject.NotifierSettings["PhoneNumber"].ToString());
            LogBook.Write("Person: " + _notifyObject.NotifierSettings["PersonName"].ToString());

            /* assign message and phone number from client information. */
            sensorInfo = _notifyObject.NotificationData.ToString();
            AlarmID = Convert.ToInt64(_notifyObject.NotifierSettings["AlarmID"].ToString());
            phoneNumber = _notifyObject.NotifierSettings["PhoneNumber"].ToString();
            personName = _notifyObject.NotifierSettings["PersonName"].ToString();
            timeOutOfRange = _notifyObject.NotifierSettings["TimeOutOfRange"].ToString();
            value = _notifyObject.NotifierSettings["Value"].ToString();
            deviceName = _notifyObject.NotifierSettings["DeviceName"].ToString();
            isMissComm = _notifyObject.NotifierSettings["isMissComm"].ToString();
            groupName = _notifyObject.NotifierSettings["GroupName"].ToString();
            alarmTime = _notifyObject.NotifierSettings["AlarmTime"].ToString();
            probe = _notifyObject.NotifierSettings["Probe"].ToString();
            fromNumber = _notifyObject.NotifierSettings["FromNumber"].ToString();
            
            notifyComResponse = new NotifyComResponse();
         
        }

       
        /// <summary>
        /// Dials the number.
        /// </summary>
        public long DialNumber()
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




           
           
                LogBook.Write("Starting to dial " + phoneNumber);

              PhoneNotify notify = new PhoneNotify();
       
                 

                            AdvancedNotifyRequest anr = new AdvancedNotifyRequest();
                            PhoneNotify pn = new PhoneNotify();
                            
                            anr.CallerIDName = "TempTrak";
                            anr.CallerIDNumber = fromNumber; //TT registration #
                            anr.PhoneNumberToDial = phoneNumber;
                          //  anr.TextToSay = "This is an important message from your "+_alarmObject.StoreName TempTrak Monitoring System, to alert,,"+personName+",,at <say-as type=\"number:digits\">"+phoneNumber+"</say-as>"+",,that device name,"+deviceName+",probe number,"+probe+", is in violation,,, measuring a value of "+value+" for an elapsed time of "+timeOutOfRange+",,,Press any key to acknowlege this call,, Thank You,,,";
                          //  string promptMessage = @"This is the " + alarmObject.StoreName + " Temp Track voice notification system, , , , There is an alarm with "
                         //   + alarmObject.IVR_SensorName + ", , Last Reading was " + SensorValueToTTS(alarmObject.UTID, alarmObject.Probe, alarmObject.SensorType, (decimal)alarmObject.Value, alarmObject.IsCelsius)
                        //   + ", , , , press any key to acknowledge. ";
              anr.TextToSay = "This is the Temp Track voice notification system, , , , There is an alarm with "
                            + deviceName + ", , Last Reading was " + value+ ", , , , press any key to acknowledge. ";
                            anr.VoiceID = 6;
                            anr.UTCScheduledDateTime = DateTime.UtcNow;
                            anr.LicenseKey = "54B1B99F-7E7E-40AC-88EB-A91DBE859B82"; //stored in config file for now
                            anr.TryCount = 5;
                            anr.NextTryInSeconds = 30;
                            anr.TTSvolume = 5;

                            NotifyReturn nr = pn.NotifyPhoneAdvanced(anr);

                            //store TransactionID into table where
                            QueueID = -1;
                            QueueID = nr.QueueID;
                         
                         

                            Thread.Sleep(30000);
                            //now query the webservice and update records
                            MachineDetection = nr.MachineDetection;
                            DigitsPressed = nr.DigitsPressed;
                            CallAnswered = nr.CallAnswered;
                            CallComplete = nr.CallComplete;
                            IVRServiceResponseText = nr.ResponseText;
                            Duration = nr.Duration;
                            StartTime = nr.StartTime;
                            EndTime = nr.EndTime;

            if (DigitsPressed.Length > 0) 
            {
                notifyComResponse.IsSucceeded = true;
                notifyComResponse.IsError = false;
                 notifyComResponse.ResponseContent += "\r\n Acknowledgement received, " + DigitsPressed + " pressed.";
            notifyComResponse.IsSucceeded = true;
            LogBook.Write("Digit Received");
            LogBook.Write("\t" + "Digit: " + DigitsPressed);
            }
            else 
            {
                notifyComResponse.IsSucceeded = false;
                notifyComResponse.IsError = true;
                if (MachineDetection == "MACHINE") 
                    notifyComResponse.ResponseContent += "\r\n Answering Machine Detected";
                else if (CallAnswered == false) 
                    notifyComResponse.ResponseContent += "\r\n No Answer";
                else if ((CallAnswered == true) && (DigitsPressed.Length == 0))
                    notifyComResponse.ResponseContent += "\r\n Call Answerd, No Digits Pressed";

            }

            return QueueID;
            }
        }





   





   

    }

