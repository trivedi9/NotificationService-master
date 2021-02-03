/*
 *  File Name : DBCommands.cs
 *  Author : Vasu Ravuri 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/29/2010
 *  Description: To maintain database stored procedures names.
 */
namespace CooperAtkins.NotificationClient.EscalationModule.DataAccess
{
    internal struct DBCommands
    {
        public const string USP_NS_GETESCALATIONPROFILEINFO = "usp_ns_GetEscalationProfileInfo";
        public const string USP_NS_PUCKALARM = "usp_ns_PuckAlarm";
        public const string USP_NS_GETFAILSAFEINFO = "usp_ns_GetFailsafeInfo";
        public const string USP_NS_ALARMCURRENTSTATUS = "usp_ns_AlarmCurrentStatus";
    }
}
