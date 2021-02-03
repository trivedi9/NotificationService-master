/*
 *  File Name : EscalationState.cs
 *  Author : Vasu Ravuri 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/25/2010
 *  Description: To maintain escalation state information.
 */

namespace CooperAtkins.NotificationClient.EscalationModule
{
    using System;
    using CooperAtkins.Interface.Alarm;
    using CooperAtkins.NotificationClient.EscalationModule.DataAccess;

    internal class EscalationState
    {
        public AlarmObject AlarmObject;
        public INotificationClient NotificationClient;
        public DateTime StartThreadTime;
        public EscalationList EscalationList;
    }
}
