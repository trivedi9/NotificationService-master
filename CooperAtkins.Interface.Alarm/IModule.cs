/*
 *  File Name : IModule.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/19/2010
 *  
 */
namespace CooperAtkins.Interface.Alarm
{

    /// <summary>
    /// IModule contract
    /// </summary>
    public interface IModule
    {
        //Invoke module, It's entry point of module
        bool Invoke(AlarmObject alarmObject, INotificationClient client);
    }
}
