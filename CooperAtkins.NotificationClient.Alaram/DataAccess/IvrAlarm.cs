/*
 *  File Name : IvrAlarm.cs
 *  Author : Vasu Ravuri 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/25/2010
 *  Description: IVR information.
 */


namespace CooperAtkins.NotificationClient.Alarm.DataAccess
{
    using System;
    using CooperAtkins.Interface.Alarm;

    class IvrAlarm : AlarmObject
    {
        public int IvrID { get; set; }
        public bool IsSuccess { get; set; }
        public Int16 AttemptCount { get; set; }
        public DateTime QueueTime { get; set; }
        public DateTime LastAttemptTime { get; set; }
        public int IvrAlarmID { get; set; }
        public string IVR_SensorName { get; set; }
        public bool IsCelsius { get; set; }
        public int LanguageID { get; set; }
        public string StoreName { get; set; }
        public string StoreNumber { get; set; }
        public int ThreadID { get; set; }
       
        
    }
}
