/*
 *  File Name : MissedComm.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 12/24/2010
 */

namespace CooperAtkins.NotificationClient.Alarm.DataAccess
{
    using System;
    using EnterpriseModel.Net;

    public class MissedComm : DomainEntity
    {
        public string GroupName { get; set; }
        public string PuckName { get; set; }
        public string SensorType { get; set; }
        public string FactoryID { get; set; }
        public string UTID { get; set; }
        public int Probe { get; set; }
        public DateTime LastContact { get; set; }
        public int Interval { get; set; }
        public int LogIntervalMins { get; set; }
    }
}
