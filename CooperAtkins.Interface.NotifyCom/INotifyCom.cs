/*
 *  File Name : INotifyCom.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/19/2010
 *  
 */
namespace CooperAtkins.Interface.NotifyCom
{
    /// <summary>
    /// INotifyCom Interface
    /// </summary>
    public interface INotifyCom
    {
        NotifyComResponse Invoke(INotifyObject notifyObject);
        void UnLoad();
    }
}
