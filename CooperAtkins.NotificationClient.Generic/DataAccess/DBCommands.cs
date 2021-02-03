/*
 *  File Name : DBCommands.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/24/2010
 */

namespace CooperAtkins.NotificationClient.Generic.DataAccess
{

    internal class DBCommands
    {
        public const string USP_NS_GETFORMATSTRING = "usp_ns_GetFormatString";
        public const string USP_NS_GETSENSORFORMATSTRINGID = "usp_ns_GetSensorFormatStringID";
        public const string USP_NS_GETACTIVENOTIFICATIONS = "usp_ns_GetActiveNotificationProfile";
        public const string USP_NS_GENSTORES = "usp_ns_GenStores";
        public const string USP_NS_GETNOTIFYMSGBOARDIDLIST = "usp_ns_GetNotifyMessageBoardList";
        public const string USP_NS_RECORDNOTIFICATION = "usp_ns_RecordNotification";
        public const string USP_NS_IS_NOTIFICATION_ACKNOWLEDGED = "usp_ns_IsNotificationAcknowledged";
        public const string USP_NS_GETNOTIFYSWITCH = "usp_ns_GetNotifySwitch";
        public const string USP_NS_GETSENSORTYPEINFOLIST = "usp_ns_GetSensorTypeInfoList";
        public const string USP_ORGANIZATIONALUNITS = "usp_OrganizationalUnits";
        public const string USP_SERVICE = "usp_Service";
        public const string USP_NS_IVR_ALARMCLEARED = "usp_ns_Ivr_AlarmCleared";
        public const string USP_NS_ISALARMCLEARDORBACKINRANGE = "usp_ns_IsAlarmCleardOrBackInRange";
        public const string USP_NS_GETCURRENTVALUE = "usp_ns_GetCurrentValue";
        public const string USP_NS_IVR_ISINPROCESS = "usp_ns_Ivr_IsInProcess";
        public const string USP_NS_GETIVRNOTIFICATIONTHREADLIST = "usp_ns_GetIVRNotificationThreadList";
        public const string USP_NS_IVR_CLEARINPROCESS = "usp_ns_IVRClearInProcess";
        
    }
}
