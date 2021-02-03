/*
 *  File Name : PuckAlarm.cs
 *  Author : Pradeep.I
 *  @ PCC Technology Group LLC
 *  Created Date : 11/29/2010
 *  Description: Entity calss for PuckAlarmContext.
 */

namespace CooperAtkins.NotificationClient.EscalationModule.DataAccess
{
    using System;
    using EnterpriseModel.Net;

    public class PuckAlarm : DomainEntity
    {
        public string UTID { get; set; }
        public int Probe { get; set; }
        public string ProbeName { get; set; }
        public DateTime AlarmTime { get; set; }
        public string AlarmType { get; set; }
        public decimal? AlarmData { get; set; }
        public Int16 AlarmActive { get; set; }
        public bool UserActive { get; set; }
        public decimal MinTemp { get; set; }
        public decimal MaxTemp { get; set; }
        public int AlarmProfileRecID { get; set; }
        public int ThresholdMinutes { get; set; }
        public int AlarmID { get; set; }
        public int NotificationID { get; set; }
    }
}
