/*
 *  File Name : AlarmQueue.cs
 *  Author : Vasu Ravuri 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/25/2010
 *  Description: To maintain the queued/in process objects information.
 */
namespace CooperAtkins.NotificationClient.Alarm
{
    using System.Collections.Generic;
    using CooperAtkins.Interface.Alarm;

    public static class AlarmQueue
    {
        public static Queue<AlarmObject> AlarmObjectQueue { get; set; }
        public static List<string> CurrentProcessSensorAlarmID { get; set; }
        public static List<AlarmObject> CurrentProcessObjects { get; set; }
        public static List<AlarmObject> DynamicAlarmObjects { get; set; }
        public static List<AlarmObject> AwaitingAcknowledment { get; set; }
        public static List<AlarmObject> AwaitingBackInAccept { get; set; }
        public static List<AlarmObject> WaitingToProcess { get; set; }
        public static List<AlarmObject> CurrentStateObjects { get; set; }//added by Pradeep I, on 08/26/2013
    }
}
