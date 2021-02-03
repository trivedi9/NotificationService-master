/*
 *  File Name : INotifyCom.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/19/2010
 *  
 */
namespace CooperAtkins.Interface.NotifyCom
{
    using System;
    /// <summary>
    /// INotifyObject Interface
    /// </summary>
    public interface INotifyObject
    {
        bool TwoWayCommunication { get; set; }
        object NotificationData { get; set; }
        string NotificationType { get; set; }
        System.Collections.Hashtable NotifierSettings { get; set; }
        String GetXML(); 
    }
   
}
