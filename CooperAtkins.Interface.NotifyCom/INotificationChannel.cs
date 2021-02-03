/*
 *  File Name : INotificationChannel.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/26/2010
 *  
 */

namespace CooperAtkins.Interface.NotifyCom
{
    /// <summary>
    /// INotificationChannel Interface
    /// </summary>
    public interface INotificationChannel
    {
        /// <summary>
        /// EndPointAddress
        /// </summary>
        string EndPointAddress { get; set; }
    }
}
