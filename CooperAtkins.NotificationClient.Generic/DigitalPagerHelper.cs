/*
*  File Name : DigitalPagerHelper.cs
*  Author : Vasu Ravuri
*  @ PCC Technology Group LLC
*  Created Date : 04/20/2011
*/

namespace CooperAtkins.NotificationClient.Generic
{
    using System.Collections.Generic;
    using CooperAtkins.Interface.NotifyCom;

    public class DigitalPagerHelper
    {
        public static List<INotifyObject> DigitalPagerQueue { get; set; }
    }
}
