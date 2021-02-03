/*
 *  File Name : EscalationFrontController.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/19/2010
 *  
 */
namespace CooperAtkins.NotificationClient.EscalationModule
{
    using System.Threading;
    using System.ComponentModel.Composition;
    using EnterpriseModel.Net;
    using CooperAtkins.Interface.Alarm;
    using CooperAtkins.NotificationClient.Generic;
    using CooperAtkins.NotificationClient.EscalationModule.DataAccess;


    [Export(typeof(IModule))]
    public class EscalationFrontController : IModule
    {
        public bool Invoke(AlarmObject alarmObject, INotificationClient client)
        {
            /* if the value is null, this notification is to clear the dynamic notifications(message boards and relay switches). */
            if (alarmObject.IsDynamicNotificationCleared)
            {
                return true;
            }

            bool hasEscalations = false;
            bool isContactSensor = false;

            /* Make isContactSensor as true if the sensor type is contact. */
            if (AlarmHelper.IsContactSensor(alarmObject.SensorType) && alarmObject.IsInAlarmState)
            {
                isContactSensor = true;
            }


            EscalationList escalationList = new EscalationList();

            /* Loading escalation information for current object */
            escalationList.Load(new Criteria() { ID = alarmObject.EscalationProfileID });
            if (escalationList.Count > 0)
            {
                hasEscalations = true;
                alarmObject.HasEscalations = true;
            }

            /*update the escalation information to alarm object*/
            if (hasEscalations)
            {
                alarmObject.StopEscalationOnExitAlarm = escalationList[0].StopEscOnSesnorNormalState;
                alarmObject.StopEscalationOnUserAck = escalationList[0].StopEscOnUserAck;
            }
            else
            {
                alarmObject.StopEscalationOnExitAlarm = false;
                alarmObject.StopEscalationOnExitAlarm = false;
            }

            escalationList.Dispose();

            /* if the sensor has escalations or it is a contact sensor then then send to the escalation process */
            if ((hasEscalations || isContactSensor) && alarmObject.AlarmType != AlarmType.COMMUNICATIONS)
            {
                ThreadPool.QueueUserWorkItem(new EscalationProcess().DoEscalationProcess, new EscalationState()
                {
                    AlarmObject = alarmObject,
                    NotificationClient = client,
                    EscalationList = escalationList
                });
            }
            else
            {
                /* if the object does not has any escalation then mark it as completed.*/                
                AlarmHelper.MarkAsCompleted(alarmObject," DOES NOT have any escalations ");
            }

            /* if the sensor type is contact then return false to stop sending notification immediately.*/
            if (isContactSensor && alarmObject.AlarmType != AlarmType.COMMUNICATIONS)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
