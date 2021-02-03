/*
 *  File Name : IvrNotificationComposer.cs
 *  Author : Vasu Ravuri
 *  @ PCC Technology Group LLC
 *  Created Date : 12/31/2010
 */

namespace CooperAtkins.NotificationClient.NotificationComposer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using CooperAtkins.Interface.Alarm;
    using CooperAtkins.Interface.NotificationComposer;
    using CooperAtkins.Interface.NotifyCom;
    using CooperAtkins.NotificationClient.NotificationComposer.DataAccess;
    using CooperAtkins.Generic;

    [Export(typeof(INotificationComposer))]
    public class IvrNotificationComposer : INotificationComposer
    {
        #region INotificationComposer Members

        private AlarmObject _alarmObject = null;

        public INotifyObject[] Compose(AlarmObject alarmObject)
        {
            LogBook.Write("IvrNotificationComposer  Compose() Method");
            List<INotifyObject> notifyList = new List<INotifyObject>();
            _alarmObject = alarmObject;

            /* Inserting the IVR Notification to database.*/
            //if (_alarmObject.AlarmID > 0) 
            RecordIVRNotification();

            /* not sending the direct notification to notify server, IVR notification process will be started separately (thread) from client process.*/
            return null;

        }

        /// <summary>
        /// To Record IVRNotification.
        /// </summary>
        private void RecordIVRNotification()
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
                    NumAttempts = 1,
                    PhoneNumber = _alarmObject.IVRPhoneNumber,
                    TransID = _alarmObject.TransID,
                    PersonName = _alarmObject.PersonName,
                    UserID = _alarmObject.IVRUserID,
                    QueueTime = DateTime.UtcNow
                };

                tTIVRNotifications.Execute();

                if (tTIVRNotifications.PhoneNumber.Trim() != string.Empty)
                    notificationStyle.RecordNotification("Queued for Voice: " +tTIVRNotifications.PersonName+" @ "+ tTIVRNotifications.PhoneNumber, _alarmObject.TransID, _alarmObject.NotificationID, NotifyStatus.PASS, NotifyTypes.VOICE);
                else
                    notificationStyle.RecordNotification("Phone number does not exists. Queue failed ",_alarmObject.TransID, _alarmObject.NotificationID, NotifyStatus.FAIL, NotifyTypes.VOICE);
                LogBook.Write("IVRNotification has been inserted.");
            }
            catch (Exception ex)
            {
                notificationStyle.RecordNotification("Queued for Voice: " + _alarmObject.IVRPhoneNumber.ToStr() + " Failed",_alarmObject.TransID, _alarmObject.NotificationID, NotifyStatus.FAIL, NotifyTypes.VOICE);
                LogBook.Write(ex, "CooperAtkins.IVRNotification");
            }
        }

        public void Receive(NotifyComResponse response, INotifyObject notifyObject)
        {

        }

        #endregion

    }
}
