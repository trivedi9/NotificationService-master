/*
 *  File Name : MessageBoardNotificationComposer.cs
 *  Author : Pradeep.I
 *  @ PCC Technology Group LLC
 *  Created Date : 11/26/2010
 */
namespace CooperAtkins.NotificationClient.NotificationComposer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using EnterpriseModel.Net;
    using CooperAtkins.Interface.NotifyCom;
    using CooperAtkins.Generic;
    using System.ComponentModel.Composition;
    using CooperAtkins.Interface.Alarm;
    using CooperAtkins.Interface.NotificationComposer;
    using CooperAtkins.NotificationClient.Generic.DataAccess;



    [Export(typeof(INotificationComposer))]
    public class MsgBrdNotificationComposer : INotificationComposer
    {
        private class PreviousNotifications
        {
            public int NotifyProfileID;
            public int NotificationID;
            public string SensorAlarmID;
        }

        private static List<PreviousNotifications> PreviousNotificationsList;
        private string CurrentClearedBoards = string.Empty;

        public MsgBrdNotificationComposer()
        {
            if (PreviousNotificationsList == null)
                PreviousNotificationsList = new List<PreviousNotifications>();
        }

        public INotifyObject[] Compose(AlarmObject alarmObject)
        {
            List<INotifyObject> notifyList = null;
            try
            {
                notifyList = new List<INotifyObject>();
                LogBook.Write("Executing compose method");

                //if the dynamic notification is already cleared , no need to send to Notify objects list to notify engine
                if (alarmObject.IsDynamicNotificationClearProcessStarted == true)
                {
                    return new List<INotifyObject>().ToArray();
                }
                //if IsDynamicNotificationCleared is true we need to set the below flag in order skip notifications
                //received from any escalations while the clear process is going on
                if (alarmObject.IsDynamicNotificationCleared)
                {
                    alarmObject.IsDynamicNotificationClearProcessStarted = true;
                }

                //to store the message
                string message = string.Empty;





                LogBook.Write("Formatting the message");
                //create instance of notification style to generate format strings, parse it
                NotificationStyle notificationStyle = new NotificationStyle();

                //get the message from the alarm object
                message = alarmObject.Value == null ? string.Empty : alarmObject.Value.ToString();

                //if message length is greater than zero, get the format string for the message
                if (message.Length > 0)
                    message = notificationStyle.GetFormatString(alarmObject, 1, "MessageBoard");

                //replace line breaks with spaces
                message = message.Replace("\\n", " ");

                //substitute actual values for the format strings using the alarm object
                message = notificationStyle.SubstituteFormatString(message, alarmObject);

                LogBook.Write("Completed message formatting");

                //to format the message for missed communications
                if (alarmObject.IsMissCommNotification == true)
                {
                    LogBook.Write("Constructing missed comm message");
                    message = "Missed Communication" + " [" + alarmObject.MissedCommSensorCount + "] sensors";

                }

                if (alarmObject.IsDynamicNotificationCleared)
                {
                    var prvNotificationList = from info in PreviousNotificationsList
                                              where info.SensorAlarmID == alarmObject.SensorAlarmID
                                              select info;
                    CurrentClearedBoards = string.Empty;
                    for (int index = prvNotificationList.Count() - 1; index >= 0; index--)
                    {
                        PreviousNotifications info = prvNotificationList.ToList<PreviousNotifications>()[index];
                        alarmObject.NotificationID = info.NotificationID;
                        alarmObject.NotifyProfileID = info.NotifyProfileID;

                        PreapreMessage(alarmObject, message, ref notifyList);
                        PreviousNotificationsList.Remove(info);
                    }
                }
                else
                {
                    if (!alarmObject.SetServerTime)
                        AddNotificationToList(alarmObject);
                    PreapreMessage(alarmObject, message, ref notifyList);
                }
            }
            catch (Exception ex)
            {
                LogBook.Write(ex, " CooperAtkins.NotificationClient.NotificationComposer.MsgBrdNotificationComposer");
            }

            return notifyList.ToArray();
        }

        private List<INotifyObject> PreapreMessage(AlarmObject alarmObject, string message, ref List<INotifyObject> notifyList)
        {
            //to store the message board id,if single message board
            int messageBoardID = 0;

            //instantiate notify list object


            //Get the Message board ID using NotifyProfileID 
            messageBoardID = GetMessageBoardID(alarmObject.NotifyProfileID, alarmObject.NotificationID);

            //check whether notification has to be sent to single message board, or multiple message boards
            //messageBoardID will be less than 0 if we have multiple message boards
            if (messageBoardID >= 0 && alarmObject.SetServerTime == false)
            {
                //get message board configurations from GenNotifyMsgBoard table using the message board id
                MessageBoardConfigList messageBoardList = new MessageBoardConfigList();
                messageBoardList.Load(new Criteria()
                {
                    ID = messageBoardID
                });

                if (messageBoardList.Count == 0)
                {
                    //if no message board found for the above message board id
                    LogBook.Write("No message board exists with the message board id: " + messageBoardID.ToString());
                }
                else if (messageBoardList.Count == 1)//single message board
                {
                    LogBook.Write("Single message board");
                    //build the notification object with message board settings
                    Hashtable notificationSettings = GetNotificationSettings(messageBoardList[0]);
                    notificationSettings.Add("SensorAlarmID", alarmObject.SensorAlarmID);
                    notificationSettings.Add("IsDynamicNotificationCleared", alarmObject.IsDynamicNotificationCleared);
                    notificationSettings.Add("SensorFactoryID", alarmObject.FactoryID);
                    notificationSettings.Add("NotificationID", alarmObject.NotificationID);
                    notificationSettings.Add("ClearMessage", alarmObject.IsDynamicNotificationCleared);

                    if (alarmObject.IsDynamicNotificationCleared)
                    {
                        if (CurrentClearedBoards.Contains(messageBoardList[0].IpAddress + alarmObject.SensorAlarmID + ","))
                        {
                            return notifyList;
                        }
                        CurrentClearedBoards += messageBoardList[0].IpAddress + alarmObject.SensorAlarmID + ",";
                    }


                    //create the notification object to be sent to Notification Engine
                    notifyList.Add(new NotifyObject()
                    {
                        NotificationData = message,
                        NotificationType = "MESSAGEBOARD",
                        NotifierSettings = notificationSettings
                    });

                }
            }
            else
            {
                LogBook.Write("Message board group");
                //get message board configurations from GenNotifyMsgBoard table using the message board id
                MessageBoardConfigList messageBoardList = new MessageBoardConfigList();


                /*To reset all message boards at service start*/
                if (alarmObject.SetServerTime)
                {
                    messageBoardList.Load(new Criteria()
                    {
                        ID = null
                    });
                }
                else
                {
                    messageBoardList.Load(new Criteria()
                    {
                        ID = messageBoardID
                    });
                }
                LogBook.Write("Message board count: " + messageBoardList.Count.ToString());
                //create the notification object list to be sent to Notification Engine
                foreach (MessageBoardConfig msgBrdConfig in messageBoardList)
                {
                    NotifyObject notifyObject = new NotifyObject();
                    notifyObject.NotificationData = message;
                    notifyObject.NotificationType = "MESSAGEBOARD";
                    notifyObject.NotifierSettings = GetNotificationSettings(msgBrdConfig);
                    notifyObject.NotifierSettings.Add("SensorAlarmID", alarmObject.SensorAlarmID);
                    notifyObject.NotifierSettings.Add("IsDynamicNotificationCleared", alarmObject.IsDynamicNotificationCleared);
                    notifyObject.NotifierSettings.Add("SensorFactoryID", alarmObject.FactoryID);
                    notifyObject.NotifierSettings.Add("NotificationID", alarmObject.NotificationID);
                    notifyObject.NotifierSettings.Add("SetServerTime", alarmObject.SetServerTime);

                    if (alarmObject.IsDynamicNotificationCleared)
                    {
                        if (CurrentClearedBoards.Contains(msgBrdConfig.IpAddress + alarmObject.SensorAlarmID + ","))
                        {
                            continue;
                        }
                        CurrentClearedBoards += msgBrdConfig.IpAddress + alarmObject.SensorAlarmID + ",";
                    }

                    notifyList.Add(notifyObject);
                }
            }

            return notifyList;
        }


        private void AddNotificationToList(AlarmObject alarmObject)
        {
            var list = from info in PreviousNotificationsList
                       where info.SensorAlarmID == alarmObject.SensorAlarmID && info.NotifyProfileID == alarmObject.NotifyProfileID
                       select info;

            /*if the same message board is exists for this sensor, do not add this board information*/
            if (list.Count() > 0)
                return;

            PreviousNotifications previousNotifications = new PreviousNotifications()
            {
                NotificationID = alarmObject.NotificationID,
                NotifyProfileID = alarmObject.NotifyProfileID,
                SensorAlarmID = alarmObject.SensorAlarmID,
            };

            PreviousNotificationsList.Add(previousNotifications);
        }


        /// <summary>
        /// Method to send the response received from the Invoking the Notification
        /// </summary>
        /// <param name="response"></param>
        /// <param name="notifyObject"></param>
        public void Receive(NotifyComResponse response, INotifyObject notifyObject)
        {
            /*Check the response object*/
            if (response != null && notifyObject.NotifierSettings["NotificationID"].ToInt() != 0)
            {
                NotificationStyle notificationStyle = new NotificationStyle();
                if (response.IsError == false)
                {
                    /*Record notification information.*/
                    notificationStyle.RecordNotification(response.ResponseContent.ToStr(), notifyObject.NotifierSettings["NotificationID"].ToInt(), 0, response.IsSucceeded ? NotifyStatus.PASS : NotifyStatus.FAIL, NotifyTypes.MSGBOARD);
                    LogBook.Write("Notification sent to message board: " + notifyObject.NotifierSettings["IpAddress"]);
                }
                else
                {
                    notificationStyle.RecordNotification(response.ResponseContent.ToStr(), notifyObject.NotifierSettings["NotificationID"].ToInt(),0, NotifyStatus.FAIL, NotifyTypes.MSGBOARD);
                    LogBook.Write("     *** Error sending message board notification to" + notifyObject.NotifierSettings["IpAddress"]);
                    LogBook.Write("     *** Error :" + response.ResponseContent);
                }
            }
        }

        #region HelperMethods
        private Hashtable GetNotificationSettings(MessageBoardConfig messageBoardConfig)
        {
            //construct notificationSettings object
            Hashtable notificationSettings = new Hashtable();
            LogBook.Write("Executing GetNotificationSettings method:");
            try
            {
                notificationSettings.Add("NotifyID", messageBoardConfig.NotifyID);
                notificationSettings.Add("Name", messageBoardConfig.MessageBoardName);
                notificationSettings.Add("BoardType", messageBoardConfig.BoardType);
                notificationSettings.Add("COMSettings", messageBoardConfig.COMSettings);
                notificationSettings.Add("IpAddress", messageBoardConfig.IpAddress);
                notificationSettings.Add("IsEnabled", messageBoardConfig.IsEnabled);
                notificationSettings.Add("IsGroup", messageBoardConfig.IsGroup);
                notificationSettings.Add("IsNetworkConnected", messageBoardConfig.IsNetworkConnected);
                notificationSettings.Add("Port", messageBoardConfig.Port);
            }
            catch (Exception ex)
            {
                LogBook.Write("     *** Error in GetNotificationSettings :" + ex.Message);
            }
            return notificationSettings;
        }

        private int GetMessageBoardID(int notifyProfileID, int notificationID)
        {
            //get the message board id, to which the message has to be sent
            NotificationProfile notificationProfile = new NotificationProfile();
            NotificationStyle notificationStyle = new NotificationStyle();
            try
            {
                notificationProfile.NotifyProfileID = notifyProfileID;
                notificationProfile = notificationProfile.Execute();
                if (notificationProfile.MsgBoardNotifyID == 0)
                    notificationStyle.RecordNotification("Message board not found", notificationID, 0,NotifyStatus.FAIL, NotifyTypes.MSGBOARD);
            }
            catch (Exception ex)
            {
                LogBook.Write("     *** Error in GetMessageBoardID :" + ex.Message);
            }
            return notificationProfile.MsgBoardNotifyID;
        }
        #endregion
    }
}
