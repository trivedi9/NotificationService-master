/*
 *  File Name : DBCommands.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/24/2010
 *  Description: To maintain stored procedure information.
 */

namespace CooperAtkins.NotificationClient.NotificationComposer.DataAccess
{

    internal class DBCommands
    {
        public const string SP_TTNOTIFY_EMAILBODY = "sp_TTNotify_EmailBody";

        public const string USP_NS_GETNOTIFYEMAILADDRESSLIST = "usp_ns_GetNotifyEmailAddressList";

        public const string USP_NS_GETNOTIFYPAGERADDRESSLIST = "usp_ns_GetNotifyPagerAddressList";

        public const string SP_TTGETACTIVENOTIFICATIONPROFILE = "sp_TTGetActiveNotificationProfile";

        public const string USP_NS_GETNOTIFYPOPUPADDRESSLIST = "usp_ns_GetNotifyPopupAddressList";

        public const string USP_NS_GETNOTIFYMOBILELIST = "usp_ns_GetNotifyMobilesList";

        public const string USP_NS_IVR_RECORDIVRNOTIFICATION = "usp_ns_Ivr_RecordIVRNotification";

        public const string USP_NS_GETIVRNOTIFICATIONTHREADLIST = "usp_ns_GetIVRNotificationThreadList";

        
    }
}
