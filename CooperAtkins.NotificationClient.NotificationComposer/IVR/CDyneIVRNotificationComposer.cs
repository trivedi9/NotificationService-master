

namespace CooperAtkins.NotificationClient.NotificationComposer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using CooperAtkins.Interface.Alarm;
    using CooperAtkins.Interface.NotificationComposer;
    using CooperAtkins.Interface.NotifyCom;
    //using CooperAtkins.NotificationClient.IVRNotificationComposer.DataAccess;
    using CooperAtkins.NotificationClient.NotificationComposer.DataAccess;
    using CooperAtkins.Generic;
    using CooperAtkins.NotificationClient.Generic.DataAccess;
    

     [Export(typeof(INotificationComposer))]

    public class CDyneIVRNotificationComposer : INotificationComposer
    {
        #region INotificationComposer Members

        private AlarmObject _alarmObject = null;
        private GenStoreInfo _genStore = null;
        private int _recID = -1;
        private string _logContent = "";

        public int RecordID
        {
            get
            {
                return _recID;
            }
        }

        public INotifyObject[] Compose(AlarmObject alarmObject)
        {
          //  LogBook.Write("CDYNEIvrNotificationComposer  Compose() Method");
            List<INotifyObject> notifyList = new List<INotifyObject>();
            _alarmObject = alarmObject;

            /*Get GenStore Information*/
            _genStore = GenStoreInfo.GetInstance();

            if (alarmObject.IsMissCommNotification)
                _logContent = "Missed Communication: "+ alarmObject.UTID.ToStr() + " SensorAlarmID: " + alarmObject.SensorAlarmID.ToStr();
            else
                _logContent = "SensorID: " + alarmObject.UTID.ToStr() + " SensorAlarmID: " + alarmObject.SensorAlarmID.ToStr();

            /*Write Log: 
             * Started Email Notification
             * Reached Email Notification Composer*/
            LogBook.Write("*** Started Composing VOICE Notification for " + _logContent + "***");

            /* Inserting the IVR Notification to database tables TTNotifications and TTIVRNotifications.*/
            //if (_alarmObject.AlarmID > 0) 
            RecordIVRNotification(_alarmObject);
            return null;

        }


        /// <summary>
        /// To Record IVRNotification.
        /// </summary>
        private void RecordIVRNotification(AlarmObject _alarmObject)
        {
            NotificationStyle notificationStyle = null;
            try
            {
                notificationStyle = new NotificationStyle();
                TTIvrNotifications tTIVRNotifications = new TTIvrNotifications()
                {
                    Action = "C",
                    AlarmID = _alarmObject.AlarmID,
                    LastAttemptTime = DateTime.UtcNow,
                    Notification_RecID = _alarmObject.NotificationID,
                    TransID = _alarmObject.TransID,
                    PersonName = _alarmObject.PersonName,
                    NumAttempts = 0,
                    PhoneNumber = _alarmObject.IVRPhoneNumber,
                    UserID = _alarmObject.IVRUserID,
                    QueueTime = DateTime.UtcNow,
                    isInProcess = 1,
                    ThreadID = _alarmObject.ThreadID

                };

                tTIVRNotifications.Execute();

                _recID = tTIVRNotifications.RecID;

                

                if (tTIVRNotifications.PhoneNumber.Trim() != string.Empty)
                {
                    //write to TTNotifcationLog table
                    notificationStyle.RecordNotification("Queued for Voice: " + tTIVRNotifications.PersonName + " @ " + tTIVRNotifications.PhoneNumber,_alarmObject.NotificationID, _alarmObject.TransID,NotifyStatus.PASS, NotifyTypes.VOICE);
                    // INotifyObject notifyObject = GetNotifyObject(_alarmObject, _genStore);
                }
                else
                    //write to TTNotifcationLog table
                    notificationStyle.RecordNotification("Phone number does not exists. Queue failed ", _alarmObject.NotificationID, _alarmObject.TransID, NotifyStatus.FAIL, NotifyTypes.VOICE);
                LogBook.Write("IVRNotification has been ADDED for AlarmID["+tTIVRNotifications.AlarmID+"] To User [+"+tTIVRNotifications.UserID+"] at ["+tTIVRNotifications.PhoneNumber+"]");
            }
            catch (Exception ex)
            {
                //write to TTNotifcationLog table
                notificationStyle.RecordNotification("Queued for Voice: " + _alarmObject.IVRPhoneNumber.ToStr() + " Failed", _alarmObject.NotificationID, _alarmObject.TransID, NotifyStatus.FAIL, NotifyTypes.VOICE);
                LogBook.Write(ex, "CooperAtkins.IVRNotification");
            }
        }

        public void Receive(NotifyComResponse response, INotifyObject notifyObject)
        {
            NotificationStyle notificationStyle = new NotificationStyle();
            /*Write Log
            Response received from Notification Engine*/
            LogBook.Write(_logContent + " Response received from IVR Notification Engine.");

            /*Check the response object*/
            if (response != null)
            {
                /*Record notification information.*/
                notificationStyle.RecordNotification(response.ResponseContent.ToStr(), notifyObject.NotifierSettings["NotificationID"].ToInt(), 0, (response.IsSucceeded ? NotifyStatus.PASS : NotifyStatus.FAIL), NotifyTypes.EMAIL);

                /*Log response content*/
                LogBook.Write(_logContent + " IVR Response: " + response.ResponseContent.ToStr());
            }
        }

        /// <summary>
        /// Create Notification Object
        /// </summary>
        /// <param name="genStoreDAO"></param>
        /// <returns></returns>
        private NotifyObject GetNotifyObject(Interface.Alarm.AlarmObject alarmObject, GenStoreInfo genStoreInfo)
        {
            string PhoneNumber = string.Empty;
            string Name = string.Empty;
            string CallerID = string.Empty;

            NotificationStyle notificationStyle = new NotificationStyle();


            //*Initialize NotifyObject*//
            NotifyObject notifyObject = new NotifyObject();

            
            PhoneNumber = alarmObject.IVRPhoneNumber;
            Name = alarmObject.PersonName;
            CallerID = genStoreInfo.ToPhoneNumber;

           


            ///*Assigning values to notification object*/
            notifyObject.NotificationType = "Voice";

            ///*Assign values to Notification settings*/
            Hashtable notificationSettings = new Hashtable();
     
            notificationSettings.Add("PhoneNumber", PhoneNumber);
           
            notificationSettings.Add("PersonName", Name);
            notificationSettings.Add("NotificationID", alarmObject.NotificationID);
            notificationSettings.Add("TransactionID", alarmObject.TransID);
            notificationSettings.Add("TimeOutOfRange", alarmObject.TimeOutOfRange);
            notificationSettings.Add("Value", alarmObject.Value);
            notificationSettings.Add("DeviceName", alarmObject.ProbeName);
            notificationSettings.Add("isMissComm", alarmObject.IsMissCommNotification);
            notificationSettings.Add("GroupName", alarmObject.GroupName);
            notificationSettings.Add("AlarmTime", alarmObject.AlarmTime);
            notificationSettings.Add("Probe", alarmObject.Probe);
            notificationSettings.Add("FromNumber", CallerID);
            notificationSettings.Add("AlarmID", alarmObject.AlarmID);

            
            notifyObject.NotifierSettings = notificationSettings;
        

            return notifyObject;
        }

        #endregion

    }
 }
