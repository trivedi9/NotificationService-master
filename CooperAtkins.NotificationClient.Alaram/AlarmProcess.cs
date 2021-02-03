/*
 *  File Name : AlarmProcess.cs
 *  Author : Rajesh 
 *  @ PCC Technology Group LLC
 *  Created Date : 11/22/2010
 */

namespace CooperAtkins.NotificationClient.Alarm
{
    using CooperAtkins.Interface.Alarm;
    using CooperAtkins.Generic;

    public class AlarmProcess
    {
        ImportModules _importModules;
        NotificationComposer.NotificationClient _client;

        public AlarmProcess()
        {
            //Import Modules       
            _importModules = new ImportModules();
            _importModules.Import();

            //Initialize Notification Client            
            _client = NotificationComposer.NotificationClient.GetInstance();            
        }

        public void StartProcess(AlarmObject alarmObject)
        {
            /*Added new by Pradeep I on 09/04/2013 - Start. If service has been restarted and few notifications are already processed we need to start processing the*/
            /*remianing escalations. Below is te code*/
            if (alarmObject.NotificationSentCount > 0 && alarmObject.NotificationType != null)
            {
                bool flag = _importModules.Run(alarmObject, (INotificationClient)_client);
            }/*Added new by Pradeep I on 09/04/2013 - End*/
            else if (_importModules.Run(alarmObject, (INotificationClient)_client))
            {
                LogBook.Write(CooperAtkins.NotificationClient.Generic.AlarmHelper.BasicAlarmInformation(alarmObject) + ", Notification process started.");
                //Send Notification
                _client.Send(alarmObject);

                LogBook.Write(CooperAtkins.NotificationClient.Generic.AlarmHelper.BasicAlarmInformation(alarmObject) + ", Notifications sent to the clients.");
            }

        }
        /// <summary>
        /// Stop process and Close client end connections
        /// </summary>
        public void StopProcess()
        {
            _client.Close();
        }
    }
}
