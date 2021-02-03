/*
 *  File Name : NotificationClient.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/23/2010
 *  
 */
namespace CooperAtkins.NotificationClient.NotificationComposer
{
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.IO;
    using System.Reflection;
    using CooperAtkins.Generic;
    using CooperAtkins.Interface.NotifyCom;
    using CooperAtkins.Interface.Alarm;
    using CooperAtkins.Interface.NotificationComposer;
    using CooperAtkins.NotificationClient.Generic;
    using System;
    using System.Net.Sockets;
    using System.Text;

    /// <summary>
    /// NotificationClient (Singleton)
    /// </summary>
    public class NotificationClient : INotificationClient
    {
        private INotifyReceiver _notifyReceiver = null;
        private NotifyClientEnd _notifyClientEnd = null;
        private const string SERVERPOPUP = "ServerPopup";


        [ImportMany(typeof(INotificationComposer))]
        private IEnumerable<INotificationComposer> _notificationComposers = null;

        private string[] _alwaysInvokeComposers = null;


        private static NotificationClient _selfInstance = null;
        public static NotificationClient GetInstance()
        {

            if (_selfInstance == null)
            {
                _selfInstance = new NotificationClient();
                _selfInstance.Import();
            }

            return _selfInstance;
        }

        public void Close()
        {
            if (_notifyReceiver != null)
            {
                _notifyReceiver.StopReceiving();
            }
        }

        private NotificationClient()
        {
            _notifyClientEnd = new NotifyClientEnd();
            _notifyClientEnd.Import();
        }

        public void Import()
        {
            if (_notificationComposers != null) return;
            /* An aggregate catalog that combines multiple catalogs */
            var catalog = new AggregateCatalog();

            /* Adds all the parts found in all assemblies in current directory */
            catalog.Catalogs.Add(
             new SafeDirectoryCatalog(
              Path.GetDirectoryName(
               Assembly.GetExecutingAssembly().Location)));

            /*Create the CompositionContainer with the parts in the catalog*/
            CompositionContainer container = new CompositionContainer(catalog);

            /*Fill the imports of this object*/
            container.ComposeParts(this);

            _alwaysInvokeComposers = GetAlwaysInvokeComposers();
        }

        public void Send(CooperAtkins.Interface.Alarm.AlarmObject alarmObject, string[] notificationTypes)
        {
            INotificationComposer composer = null;
            string[] notificationTypes4Ref = notificationTypes;
            bool ImDone = false;

            /*Added on 02/28/2011 to update genStore values for every new alarm object.*/
            new CooperAtkins.NotificationClient.Generic.DataAccess.GenStoreInfo().UpdateGenStore();

        Label4Revisit:

            if (notificationTypes != null)
            {
                foreach (string notifyType in notificationTypes)
                {
                    /* As we are using same composer for Server Popup and Remote Popup we are using this 
                       flag in composer to differentiate the notification object*/
                    if (notifyType == SERVERPOPUP)
                    {
                        alarmObject.IsServerPopup = true;
                    }

                    /* if already sent the notification, do not send it again for always invokes */
                    if (ImDone && notificationTypes4Ref != null)
                    {
                        bool flag = false;
                        foreach (string nextType in notificationTypes4Ref)
                        {
                            if (nextType.ToLower().Trim() == notifyType.ToLower().Trim())
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (flag)
                            continue;
                    }

                    NotificationEndPointElement endpoint;
                    composer = GetComposer(WhoAmI(notifyType, out endpoint));
                    if (composer != null)
                        SendToNotifyEngine(alarmObject, composer, notifyType, endpoint);

                    /*After sending the notify type(Server pop) to Notification Server,
                     * set the flag to false so that we can use remote popup as notification */
                    if (notifyType == SERVERPOPUP)
                    {
                        alarmObject.IsServerPopup = false;
                    }
                }
            }

            if (!ImDone)
            {
                ImDone = true;
                notificationTypes = _alwaysInvokeComposers;
                goto Label4Revisit;
            }
        }
        public void Send(CooperAtkins.Interface.Alarm.AlarmObject alarmObject)
        {

            try
            {

                /* get type of notifications based on notify profile id */

                NotificationEligibility notificationEligibility = new NotificationEligibility(alarmObject.NotifyProfileID, alarmObject.IsDynamicNotificationCleared, alarmObject.IsDynamicNotificationRemoved, alarmObject.IsProcessCompleted, alarmObject.IsResumedNitification);
                notificationEligibility.NotificationSentCount = alarmObject.NotificationSentCount;


                string[] notificationTypes = notificationEligibility.GetNotifyTypes();
                alarmObject.IVRUserID = notificationEligibility.IVRUserID;


                string nTypes = string.Empty;

              //  if (notificationTypes != null)
              //  {
                    foreach (string item in notificationTypes)
                    {
                        nTypes += ", " + item;
                    }

                    LogBook.Write(AlarmHelper.BasicAlarmInformation(alarmObject) + ", active notification types:" + nTypes);
               // }
                /*  if user configured dynamic objects for one escalation and if the user not configured in next escalation then do not update the "HasDynamicTypes",
                        to clear the previous dynamic notification, "HasDynamicTypes" should not become false.
                    */
#if DEBUG
                LogBook.Write(String.Format("HasDynamicTypes = {0} HasPreviousMessageBoard = {1} HasPreviousSwitch = {2}",
                    alarmObject.HasDynamicTypes, alarmObject.HasPreviousMessageBoard, alarmObject.HasPreviousSwitch));

                LogBook.Write(String.Format("IsDynamicNotificationCleared = {0} IsDynamicNotificationRemoved = {1} AlarmStateExitTime = {2}",
                    alarmObject.IsDynamicNotificationCleared, alarmObject.IsDynamicNotificationRemoved, alarmObject.AlarmStateExitTime));
#endif
                if (!alarmObject.HasDynamicTypes)
                {
                    alarmObject.HasDynamicTypes = notificationEligibility.HasDynamicTypes;
                    if (!alarmObject.HasPreviousMessageBoard)
                        alarmObject.HasPreviousMessageBoard = notificationEligibility.HasPreviousMessageBoard;
                    if (!alarmObject.HasPreviousSwitch)
                        alarmObject.HasPreviousSwitch = notificationEligibility.HasPreviousSwitch;
                }

                if ((alarmObject.HasPreviousSwitch || alarmObject.HasPreviousMessageBoard) && alarmObject.IsDynamicNotificationCleared && !alarmObject.IsDynamicNotificationRemoved)
                {
                    // this is where we would need to add logic 
                    // but the settings likely carry through only for the previous notification which in this case was blank.
                    if (!nTypes.ToLower().Contains("switch") && alarmObject.HasPreviousSwitch)
                    {
                        LogBook.Write("Adding switch to list");
                        nTypes += ", " + "Switch";

                        string[] tempArray = new string[notificationTypes.Length + 1];
                        for (int arrIndex = 0; arrIndex < notificationTypes.Length; arrIndex++)
                        {
                            tempArray[arrIndex] = notificationTypes[arrIndex];
                        }
                        tempArray[notificationTypes.Length] = "Switch";
                        notificationTypes = tempArray;
                    }

                    if (!nTypes.ToLower().Contains("messageboard") && alarmObject.HasPreviousMessageBoard)
                    {
                        LogBook.Write("Adding messageboard to list");
                        nTypes += ", " + "MessageBoard";

                        string[] tempArray = new string[notificationTypes.Length + 1];
                        for (int arrIndex = 0; arrIndex < notificationTypes.Length; arrIndex++)
                        {
                            tempArray[arrIndex] = notificationTypes[arrIndex];
                        }
                        tempArray[notificationTypes.Length] = "MessageBoard";
                        notificationTypes = tempArray;
                    }
                }

                // We should only allow specific types to be "sent" if dynamic notifications have not been removed
                // ONLY SEND IF IT HAS NOT EXITED ALARM STATE OR IF IT NEEDS TO REMOVE DYNAMIC NOTIFICATIONS

                // correction - if this is an escalation and the option to continue escalation when it returns to normal is selected, it also needs to send the notifications on
                // the previous change stopped this from happening, also if it is a first alarm that has since returned to normal, we need to send it on.

                //if ((alarmObject.AlarmStateExitTime == DateTime.MinValue) ||
                //    ((alarmObject.HasPreviousSwitch || alarmObject.HasPreviousMessageBoard) &&
                //    alarmObject.IsDynamicNotificationCleared && !alarmObject.IsDynamicNotificationRemoved) ||
                //    (alarmObject.AlarmStateExitTime != DateTime.MinValue && alarmObject.IsEscalationNotification && alarmObject.StopEscalationOnExitAlarm == false) )
           
                if ((alarmObject.AlarmStateExitTime == DateTime.MinValue) ||
                    ((alarmObject.HasPreviousSwitch || alarmObject.HasPreviousMessageBoard) &&
                    alarmObject.IsDynamicNotificationCleared && !alarmObject.IsDynamicNotificationRemoved) ||
                    (alarmObject.AlarmStateExitTime != DateTime.MinValue && alarmObject.IsEscalationNotification && alarmObject.StopEscalationOnExitAlarm == false) ||
                    // next line would address a first time alarm that had already "returned to normal" and would allow it to send. Case in point is when a Wi-Fi transmitter buffers readings, 
                    //goes out of range and back in, then re-establishes communication with Wi-Fi and server
                    (alarmObject.AlarmStateExitTime != DateTime.MinValue && alarmObject.IsEscalationNotification == false && alarmObject.IsFailsafeEscalationNotification == false))
                {
                    if (alarmObject.IsDynamicNotificationCleared && !alarmObject.IsDynamicNotificationRemoved
                        && alarmObject.AlarmStateExitTime != DateTime.MinValue)
                        //&& (alarmObject.IsEscalationNotification && alarmObject.StopEscalationOnExitAlarm == false))
                    // We are removing dynamic notifications, so remove all other types and only add the dynamic ones that should be included
                    {
                        string[] tempArray = null;
                        if (alarmObject.HasPreviousSwitch && alarmObject.HasPreviousMessageBoard)
                        {
                            tempArray = new string[2];
                            tempArray[0] = "MessageBoard";
                            tempArray[1] = "Switch";
                        }
                        else if (alarmObject.HasPreviousSwitch)
                        {
                            tempArray = new string[1];
                            tempArray[0] = "Switch";
                        }
                        else if (alarmObject.HasPreviousMessageBoard)
                        {
                            tempArray = new string[1];
                            tempArray[0] = "MessageBoard";
                        }

                        notificationTypes = tempArray;
                    }
                    Send(alarmObject, notificationTypes);
                    if (notificationTypes != null)
                    {
                        alarmObject.NotificationSentCount++;
                    }
                }
                //}
            }
            catch (Exception ex)
            {
                LogBook.Write("NotificationClient.Send line 263 Exception: "+ex.Message);
            }
        }

        private void SendToNotifyEngine(CooperAtkins.Interface.Alarm.AlarmObject alarmObject, INotificationComposer composer, string composeType, NotificationEndPointElement endpoint)
        {
            INotifyObject[] notifyObjects = null;
            if (composer != null)
                notifyObjects = composer.Compose(alarmObject);

            if (notifyObjects != null)
            {
                foreach (INotifyObject notifyObject in notifyObjects)
                {
                    notifyObject.NotificationType = composeType;
                    Interface.NotifyCom.NotifyComResponse response = null;

                    //Invoke Engine and send
                    //Engine returns NotifyComResponse
                    response = InvokeNotifyEngine(notifyObject, endpoint);
                    composer.Receive(response, notifyObject);
                }
            }
        }

        internal NotifyComResponse InvokeNotifyEngine(INotifyObject notifyObject, NotificationEndPointElement endpoint)
        {
            //Implement client INotifyReceiver for client and notification server communication.
            int notifyClienttimeOutValue = 0;
            string senderType = "", endpointAddress;
            if (endpoint != null && !string.IsNullOrEmpty(endpoint.WType) && !string.IsNullOrEmpty(endpoint.EndpointAddress))
            {
                senderType = endpoint.WType;
                endpointAddress = endpoint.EndpointAddress;
                notifyClienttimeOutValue = endpoint.Timeout;
                LogBook.Write(notifyObject.NotificationType + " end point type is " + endpointAddress);

            }
            else
            {
                senderType = Interface.Alarm.AlarmModuleConfiguration.Instance.Configuration.EndPoint.WType;
                endpointAddress = Interface.Alarm.AlarmModuleConfiguration.Instance.Configuration.EndPoint.EndpointAddress;
                notifyClienttimeOutValue = Interface.Alarm.AlarmModuleConfiguration.Instance.Configuration.EndPoint.Timeout;
                LogBook.Write(notifyObject.NotificationType + " end point type is {embed}");
            }

            NotifyComResponse response = null;

            if (senderType == "{embed}")
            {
                _notifyReceiver = CommunicationAdapter.GetInstance();
                response = _notifyReceiver.Execute(notifyObject);
            }
            else
            {
                //Invoke remote object
                INotificationChannelClient client = _notifyClientEnd.GetClient(senderType, endpointAddress);
                //client.EndPointAddress = endpoint.EndpointAddress;
                client.OnReceive((data, remEndpoint) =>
                {
                    try
                    {
                        response = NotifyComResponse.Create(data);
                    }
                    catch (Exception ex)
                    {
                        response = new NotifyComResponse();
                        response.IsSucceeded = true;
                        response.IsError = true;
                        response.ResponseContent = "Notification Failed, while receiving data from notification engine.";
                        LogBook.Write("Error has occurred while receiving data from notification engine (Exception Message:" + ex.Message + ") \n Received Data: " + data);

                    }
                });

                bool timeout = false;
                long startTick = DateTime.Now.Ticks;
            Label4Retry:

                try
                {
                    client.Send(notifyObject.GetXML());
                }
                catch (Exception ex)
                {

                    if (notifyClienttimeOutValue == 0 || notifyClienttimeOutValue >= ((DateTime.Now.Ticks - startTick) / TimeSpan.TicksPerMillisecond))
                        timeout = true;

                    if (!timeout)
                        goto Label4Retry;
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("Notification Service - Error has occurred while connecting to the remote notification engine.");
                        sb.AppendLine("Remote endpoint:" + endpointAddress);
                        sb.AppendLine("NotificationType:" + notifyObject.NotificationType);
                        sb.AppendLine("Data:" + notifyObject.NotificationData);
                        sb.AppendLine("Technical Information:" + ex.Message + ". " + ex.StackTrace);
                        LogBook.Write(sb.ToString(), "Notification Client", ErrorSeverity.Critical);

                        response = new NotifyComResponse();
                        response.IsSucceeded = false;
                        response.IsError = true;
                        response.ResponseContent = "Failed, while connecting to notification engine.";
                        LogBook.Write("Error has occurred while connecting to the remote notification engine (Exception Message:" + ex.Message + ")");


                        //record notification

                        NotifyTypes notifyType;
                        Enum.TryParse<NotifyTypes>(notifyObject.NotificationType.Trim(), true, out notifyType);

                        NotificationStyle notificationStyle = new NotificationStyle();
                        notificationStyle.RecordNotification(sb.ToString(), 0,0, response.IsSucceeded ? NotifyStatus.PASS : NotifyStatus.FAIL, notifyType);
                    }
                }
            }
            return response;
        }

        private INotificationComposer GetComposer(string comFullName)
        {
            INotificationComposer composer = null;
            foreach (INotificationComposer nCom in _notificationComposers)
            {
                if (nCom.GetType().FullName.ToLower() == comFullName.ToLower()) //to avoid and case discrepencies
                {
                    composer = nCom;
                    break;
                }
            }
            return composer;
        }

        private string[] GetAlwaysInvokeComposers()
        {
            List<string> composers = new List<string>();
            foreach (ComposerElement element in AlarmModuleConfiguration.Instance.Configuration.Composers)
            {
                if (element.InvokeAlways != null && (bool)element.InvokeAlways)
                {
                    composers.Add(element.Name);
                }
            }
            return composers.ToArray();
        }

        internal string WhoAmI(string name, out NotificationEndPointElement endPoint)
        {
            string comFullName = "";
            endPoint = null;
            foreach (ComposerElement element in AlarmModuleConfiguration.Instance.Configuration.Composers)
            {
                if (element.Name.ToLower() == name.ToLower())
                {
                    comFullName = element.ModuleType;
                    if (element.EndPoint != null && element.EndPoint.WType != "" && element.EndPoint.EndpointAddress != "")
                    {
                        endPoint = element.EndPoint;
                    }
                    break;
                }
            }
            return comFullName;
        }

        internal string GetCustomVoiceSettings(string name, out string customElements)
        {
            string customName = "";
            customElements = null;
            foreach (VoiceCustomElement element in AlarmModuleConfiguration.Instance.Configuration.VoiceCustoms)
            {
                if (element.Name.ToLower() == name.ToLower())
                {
                    customName = element.Name;
                    if (element.Name != null)
                    {
                        customElements = element.Value;
                    }
                    break;
                }
            }
            return customElements;
        }

    }
}
