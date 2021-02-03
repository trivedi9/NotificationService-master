/*
 *  File Name : IModuleAsync.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/19/2010
 *  
 */
namespace CooperAtkins.Interface.Alarm
{
    /// <summary>
    /// IModuleAsync Interface to expose Invoke method
    /// </summary>
    public interface IModuleAsync
    {
        //Invoke module, It's entry point of module
        bool Invoke(AlarmObject alarmObject, INotificationClient client);
    }
}
