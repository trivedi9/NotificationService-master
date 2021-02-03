/*
 *  File Name : MessageBoardHelper.cs
 *  Author : Pradeep.I
 *  @ PCC Technology Group LLC
 *  Created Date : 02/28/2011
 */
namespace CooperAtkins.NotificationClient.NotificationComposer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using CooperAtkins.Interface.Alarm;
    using System.Collections;
    public class MessageBoardHelper
    {
        public void SetServerTime()
        {
            NotificationEndPointElement element = null;
            NotificationClient notificationClient = NotificationClient.GetInstance();

            //notificationClient.WhoAmI("MessageBoard", out element);

            AlarmObject alarmObject = new AlarmObject();
            alarmObject.SetServerTime = true;

            notificationClient.Send(alarmObject, new string[] { "MessageBoard" });
        }
    }
}
