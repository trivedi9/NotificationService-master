/*
*  File Name : SmsHelper.cs
*  Author : Vasu Ravuri
*  @ PCC Technology Group LLC
*  Created Date : 04/20/2011
*/

namespace CooperAtkins.NotificationClient.Generic
{
    using System.Collections.Generic;
    using CooperAtkins.Interface.NotifyCom;

    public class SmsHelper
    {
        public static List<INotifyObject> SmsQueue { get; set; }
    }
}
