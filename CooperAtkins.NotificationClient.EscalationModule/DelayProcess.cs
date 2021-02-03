/*
 *  File Name : DelayProcess.cs
 *  Author : Vasu
 *  @ PCC Technology Group LLC
 *  Created Date : 12/24/2010
 */

namespace CooperAtkins.NotificationClient.EscalationModule
{
    using System;
    using EnterpriseModel.Net;
    using CooperAtkins.Interface.Alarm;
    using CooperAtkins.NotificationClient.Generic;
    using CooperAtkins.NotificationClient.EscalationModule.DataAccess;

    public class DelayProcess
    {
        /// <summary>
        /// Insert or update alarm information.
        /// </summary>
        /// <param name="alarmObject"></param>
        public void RecordAlarm(AlarmObject alarmObject)
        {
            if (alarmObject.AlarmID != 0)
            {
                PuckAlarm puckAlarm = new PuckAlarm();
                puckAlarm.AlarmID = alarmObject.AlarmID;

                /* Update existing alarm to active if it is inactive */
                using (PuckAlarmContext context = new PuckAlarmContext())
                {
                    context.Save(puckAlarm, EnterpriseModel.Net.ObjectAction.Edit);
                }
                return;
            }

            /* if it reset alarm type then stop executing function. */
            if (alarmObject.AlarmID != 0 && alarmObject.AlarmType > 100)
                return;

            string alarmType = string.Empty;
            if (alarmObject.Value < alarmObject.AlarmMinValue)
                alarmType = "0";
            else
                alarmType = "1";

            alarmObject.AlarmTime = DateTime.UtcNow;
            alarmObject.TimeOutOfRange = Common.DateDiff("n", alarmObject.AlarmStartTime, alarmObject.AlarmTime);

            PuckAlarm puckAlarmObj = new PuckAlarm();
            puckAlarmObj.UTID = alarmObject.UTID;
            puckAlarmObj.Probe = alarmObject.Probe;
            puckAlarmObj.ProbeName = alarmObject.ProbeName;
            puckAlarmObj.AlarmTime = alarmObject.AlarmTime;
            puckAlarmObj.AlarmType = alarmType;
            puckAlarmObj.AlarmData = alarmObject.Value;
            puckAlarmObj.AlarmActive = 1;
            puckAlarmObj.UserActive = false;
            puckAlarmObj.MinTemp = alarmObject.AlarmMinValue;
            puckAlarmObj.MaxTemp = alarmObject.AlarmMaxValue;
            puckAlarmObj.AlarmProfileRecID = alarmObject.AlarmProfileID;
            puckAlarmObj.ThresholdMinutes = alarmObject.CondThresholdMins;
            puckAlarmObj.NotificationID = alarmObject.NotificationID;

            /* Insert new alarm if there is no existing alarm. */
            using (PuckAlarmContext context = new PuckAlarmContext())
            {
                puckAlarmObj = context.Save(puckAlarmObj, EnterpriseModel.Net.ObjectAction.New);
                alarmObject.AlarmID = puckAlarmObj.AlarmID;
            }
        }
    }
}
