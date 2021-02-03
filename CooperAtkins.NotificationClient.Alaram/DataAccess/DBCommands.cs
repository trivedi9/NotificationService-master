/*
 *  File Name : DBCommands.cs
 *  Author : Vasu Ravuri 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/25/2010
 *  Description: To maintain stored procedures information.
 */
namespace CooperAtkins.NotificationClient.Alarm.DataAccess
{

    internal struct DBCommands
    {
        public const string USP_NS_ADDNOTE = "usp_ns_AddNote";
        public const string USP_NS_GET_ALARMLIST = "usp_ns_GetAlarmList";
        public const string USP_NS_REMOVE_NOTIFICATION = "usp_ns_RemoveNotification";
        public const string USP_NS_REMOVE_IVRNOTIFICATION = "usp_ns_RemoveIVRNotification";
        public const string USP_NS_UPDATENOTIFICATIONSTATUS = "usp_ns_UpdateNotificationStatus";
        public const string USP_NS_DORESUMEPROCESS = "usp_ns_DoResumeProcess";
        public const string USP_NS_GETMISSEDCOMMSETTINGS = "usp_ns_GetMissedCommSettings";
        public const string USP_NS_GETMISSEDCOMMUNICATION = "usp_ns_GetMissedCommunication";
        public const string USP_NS_GET_IVRALARMLIST = "usp_ns_GetIVRNotificationList";
        public const string USP_NS_CHECKBACKINACCEPT = "usp_ns_CheckBackInAccept";
        public const string USP_NS_ALARMCURRENTSTATUS = "usp_ns_AlarmCurrentStatus";
        public const string USP_NS_GETIVRNOTIFICATIONTHREADLIST = "usp_ns_GetIVRNotificationThreadList";
        
    }
}
