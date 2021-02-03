/*
 *  File Name : NotificationClient.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/23/2010
 *  
 */
namespace CooperAtkins.NotificationClient.NotificationComposer
{
    using System;
    using System.Reflection;
    using CooperAtkins.Interface.NotifyCom;


    /// <summary>
    /// CommunicationAdapter
    /// </summary>
    internal class CommunicationAdapter
    {
        private static INotifyReceiver _receiver = null;

        private static object lockInstance = new object();

        public static INotifyReceiver GetInstance()
        {
            lock (lockInstance)
            {
                if (_receiver == null)
                {

                    /* Below code for embed notification engine */
                    Assembly assembly = Assembly.Load("CooperAtkins.NotificationServer.NotifyEngine");

                    /* Get the type to use. */
                    Type notifyType = assembly.GetType("CooperAtkins.NotificationServer.NotifyEngine.NotificationReceiver");

                    /* Create an instance. */
                    _receiver = (INotifyReceiver)Activator.CreateInstance(notifyType);
                }
            }
            return _receiver;
        }
        private static Interface.Alarm.NotificationEndPointElement GetEndPoint(string composeType)
        {
            Interface.Alarm.NotificationEndPointElement endPoint = null;

            /*get endpoint based on composer name.*/
            foreach (Interface.Alarm.ComposerElement composer in Interface.Alarm.AlarmModuleConfiguration.Instance.Configuration.Composers)
            {
                if (composer.EndPoint != null && composer.Name.ToLower() == composeType.ToLower())
                {
                    endPoint = composer.EndPoint;
                    break;
                }
            }
            return endPoint;
        }



    }
}
