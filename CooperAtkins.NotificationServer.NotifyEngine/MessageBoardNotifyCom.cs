/*
 *  File Name : MessageBaordNotifyCom.cs
 *  Author : Pradeep
 *  @ PCC Technology Group LLC
 *  Created Date : 11/27/2010
 */

namespace CooperAtkins.NotificationServer.NotifyEngine
{
    using System;
    using CooperAtkins.Generic;
    using System.ComponentModel.Composition;
    using CooperAtkins.Interface.NotifyCom;

    [Export(typeof(INotifyCom))]
    public class MessageBoardNotifyCom : INotifyCom
    {

        #region INotifyCom Members

        public MessageBoardNotifyCom()
        {
            if (LockObj == null)
            {
                LockObj = new object();
            }
        }

        private static object LockObj = null;
        private static bool IsProcessCompleted = true;

        public NotifyComResponse Invoke(INotifyObject notifyObject)
        {
            while (!IsProcessCompleted)
            {
                System.Threading.Thread.Sleep(200);
            }
            lock (LockObj)
            {
                IsProcessCompleted = false;

                NotifyComResponse response = null;
                response = new NotifyComResponse();

                try
                {
                    LogBook.Write("Executing Invoke Method for Sensor: " + notifyObject.NotifierSettings["SensorAlarmID"].ToStr());
                    //get the message board instance
                    IMessageBoard messageBoard = GetMessageBoard(notifyObject);
                    //invoke display message method
                    messageBoard.DisplayMessage(notifyObject.NotificationData.ToStr());
                    response.IsSucceeded = true;
                    response.IsError = false;

                    if (notifyObject.NotifierSettings["ClearMessage"].ToBoolean() || notifyObject.NotifierSettings["IsDynamicNotificationCleared"].ToBoolean())
                    {
                        response.ResponseContent = "Clearing message from message board: [" + notifyObject.NotifierSettings["Name"].ToStr() + "] " + (notifyObject.NotifierSettings["IsNetworkConnected"].ToBoolean() == true ? "IP:" : "COM") + notifyObject.NotifierSettings["IpAddress"].ToStr() + ":" + notifyObject.NotifierSettings["Port"] + ".";
                    }
                    else
                    {
                        response.ResponseContent = "Message to: [" + notifyObject.NotifierSettings["Name"].ToStr() + "] " + (notifyObject.NotifierSettings["IsNetworkConnected"].ToBoolean() == true ? "IP:" : "COM") + notifyObject.NotifierSettings["IpAddress"].ToStr() + ":" + notifyObject.NotifierSettings["Port"] + " sent successfully.";
                    }

                    IsProcessCompleted = true;
                }
                catch (Exception ex)
                {
                    IsProcessCompleted = true;
                    response.IsSucceeded = false;
                    response.IsError = true;
                    if (notifyObject.NotifierSettings["ClearMessage"].ToBoolean() || notifyObject.NotifierSettings["IsDynamicNotificationCleared"].ToBoolean())
                    {
                        response.ResponseContent = "Clearing message from message board: [" + notifyObject.NotifierSettings["Name"].ToStr() + "] " + (notifyObject.NotifierSettings["IsNetworkConnected"].ToBoolean() == true ? "IP:" : "COM") + notifyObject.NotifierSettings["IpAddress"].ToStr() + ":" + notifyObject.NotifierSettings["Port"] + " failed.";
                    }
                    else
                    {
                        response.ResponseContent = "Message to: [" + notifyObject.NotifierSettings["Name"].ToStr() + "] " + (notifyObject.NotifierSettings["IsNetworkConnected"].ToBoolean() == true ? "IP:" : "COM") + notifyObject.NotifierSettings["IpAddress"].ToStr() + ":" + notifyObject.NotifierSettings["Port"] + " Failed.";
                    }
                    LogBook.Write(ex, "CooperAtkins.NotificationServer.NotifyEngine.MessageBoardNotifyCom");

                }

                return response;
            }

        }

        public void UnLoad()
        {

        }


        #endregion

        #region HelperMethods
        /// <summary>
        /// Get the message board instance based on the boardtype
        /// </summary>
        /// <param name="boardType"></param>
        /// <returns></returns>
        private IMessageBoard GetMessageBoard(INotifyObject notifyObject)
        {
            IMessageBoard msgBoard = null;
            int boardType = notifyObject.NotifierSettings["BoardType"].ToInt();

            switch (((MessageBoardType)boardType).ToStr())
            {
                case "MB_Unknown":
                    break;
                case "MB_215C":
                    msgBoard = new MsgBrd215C();
                    msgBoard.BoardType = MessageBoardType.MB_215C;
                    break;
                case "MB_BetaBrite":
                    msgBoard = new MsgBrdBetaBrite();
                    msgBoard.BoardType = MessageBoardType.MB_BetaBrite;
                    break;
            }
            msgBoard.COMMSettings = notifyObject.NotifierSettings["COMSettings"].ToStr().Replace('8', '7');
            msgBoard.Name = notifyObject.NotifierSettings["Name"].ToStr();
            msgBoard.IPAddress = notifyObject.NotifierSettings["IpAddress"].ToStr();
            msgBoard.Port = notifyObject.NotifierSettings["Port"].ToInt();
            msgBoard.IsNetworkAttached = notifyObject.NotifierSettings["IsNetworkConnected"].ToBoolean();
            msgBoard.IsGroup = notifyObject.NotifierSettings["IsGroup"].ToBoolean();
            msgBoard.IsEnabled = notifyObject.NotifierSettings["IsEnabled"].ToBoolean();
            msgBoard.SensorAlarmID = notifyObject.NotifierSettings["SensorAlarmID"].ToStr();
            msgBoard.SensorFactoryID = (notifyObject.NotifierSettings["SensorFactoryID"] != null) ? notifyObject.NotifierSettings["SensorFactoryID"].ToStr() : "";

            /* added on 02/28/2011 to server time for the first time. */
            if (notifyObject.NotifierSettings.ContainsKey("SetServerTime"))
            {
                msgBoard.SetServerTime = notifyObject.NotifierSettings["SetServerTime"].ToBoolean();
            }

            //add new for multiple message board issue on 02/16/2011
            msgBoard.ID = notifyObject.NotifierSettings["NotifyID"].ToInt();

            if (notifyObject.NotifierSettings["IsDynamicNotificationCleared"] != null)
                msgBoard.IsDynamicNotificationCleared = notifyObject.NotifierSettings["IsDynamicNotificationCleared"].ToBoolean();
            else
                msgBoard.IsDynamicNotificationCleared = null;

            return msgBoard;
        }
        #endregion
    }
}
