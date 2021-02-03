using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel.Composition;
using System.Collections;
using System.Threading;

using CooperAtkins.Interface.NotifyCom;
using CooperAtkins.Generic;
using CooperAtkins.NotificationClient.NotificationComposer.com.cdyne.ws;




namespace CooperAtkins.NotificationClient.NotificationComposer
{
    [Export(typeof(INotifyCom))]
    public class IVRProcessor : INotifyCom
    {
  
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
            INotifyObject _notifyObject = null;
            try
            {

                {


                    _notifyObject = notifyObject;
        
                    /* Dialing to a phone number.*/
                    response.TransactionIDReturned = DialNumber(_notifyObject);
                    if (response.TransactionIDReturned > -1)
                    {
                        response.IsError = false;
                        response.IsSucceeded = true;
                    }
                    else
                    {
                        response.IsError = true;
                        response.IsSucceeded = false;
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
        /// Dials the number.
        /// </summary>
        public long DialNumber(INotifyObject _notifyObject)
        {
            long QueueID = -1;
            string sensorInfo = _notifyObject.NotificationData.ToString();     
            string phoneNumber = _notifyObject.NotifierSettings["PhoneNo"].ToString();
            string storePhoneNumber = _notifyObject.NotifierSettings["StorePhoneNo"].ToString();
            string voiceMode = _notifyObject.NotifierSettings["CDYNE_VOICEID"].ToString();
            string voiceVolume = _notifyObject.NotifierSettings["CDYNE_VOLUME"].ToString();
            string voiceLicense = _notifyObject.NotifierSettings["CDYNE_ID"].ToString();
            string voiceRetries = _notifyObject.NotifierSettings["CDYNE_RETRIES"].ToString();

            LogBook.Write("PhoneNo.: " + _notifyObject.NotifierSettings["PhoneNumber"] + ", Voice Prompt: " + sensorInfo);

           

  

            PhoneNotify notify = new PhoneNotify();
            AdvancedNotifyRequest anr = new AdvancedNotifyRequest();           
            PhoneNotify pn = new PhoneNotify();

            anr.CallerIDName = "TempTrak";
            anr.CallerIDNumber = storePhoneNumber ; //TT registration #, if blank shows as private
            anr.PhoneNumberToDial = phoneNumber;
            anr.TextToSay = /*"~\\ActOnFeature(false)~*/ "~\\SetVar(Attempt|1)~ ~\\ActOnDigitPress(false)~   ~\\Label(Menu)~ ~\\AssignDTMF(*|Ack)~ ~\\ActOnDigitPress(true)~ ~\\Beep()~ ~\\PlaySilence(0.1)~" + sensorInfo + " ~\\WaitForDTMF(1)~" + "Press the star key to acknowledge receipt of this alert.  Press 1 to repeat this message. ~\\PlaySilence(0.1)~   ~\\WaitForDTMF(10)~ ~\\IncreaseVariable(Attempt|1)~ ~\\GotoIf(Attempt|1|Menu)~ ~\\GotoIf(Attempt|2|Menu)~ ~\\GotoIf(Attempt|3|AttemptEnd)~ ~\\Label(AttemptEnd)~ Good Bye ~\\EndCall()~ ~\\Label(Ack)~ ~\\PlaySilence(0.1)~ Thank you for acknowledging receipt of this alert. ~\\PlaySilence(0.1)~ Log into TempTrak to take corrective action and officially clear alert. ~\\PlaySilence(0.1)~ Good Bye. ~\\EndCall()~";
            anr.VoiceID = voiceMode.ToInt(); //store this in config if customer does not like voice
            anr.UTCScheduledDateTime = DateTime.UtcNow;
            anr.LicenseKey = voiceLicense; //"54B1B99F-7E7E-40AC-88EB-A91DBE859B82"; //stored in config file for now
            anr.TryCount = 0; //voiceRetries.ToInt();  //controlled by TT not CDyne
            anr.NextTryInSeconds = 0; //controlled by TT
            anr.TTSvolume = Convert.ToByte(voiceVolume); //also store this in config
            

            NotifyReturn nr = pn.NotifyPhoneAdvanced(anr);
            LogBook.Write("Info Passed to Notify Phone Advanced: ");
            LogBook.Write("CallerIDName: "+anr.CallerIDName);
            LogBook.Write("CallerIDNumber: " + anr.CallerIDNumber);
            LogBook.Write("PhoneNumberToDial: " + anr.PhoneNumberToDial);
            LogBook.Write("TextToSay: " + anr.TextToSay);
            LogBook.Write("VoiceID: " + anr.VoiceID);
            LogBook.Write("UTCScheduledDateTime: " + anr.UTCScheduledDateTime);
            LogBook.Write("LicenseKey: " + anr.LicenseKey);
            LogBook.Write("NextTryInSeconds: " + anr.NextTryInSeconds);
            LogBook.Write("TTSVolume: " + anr.TTSvolume);            
            
            LogBook.Write("Dialed: " + phoneNumber+" ......... ");
          
            //final released version will have script uploaded to CDyne so we can say goodbye if user presses key

 
            QueueID = nr.QueueID;
            LogBook.Write("QueueID: " + nr.QueueID + " ......... ");
            return QueueID;
        }
    }













}

