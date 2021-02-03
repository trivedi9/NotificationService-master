/*
 *  File Name : PopupClient.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/29/2010
 */

namespace CooperAtkins.NotificationServer.NotifyEngine.POPUP
{
    using System;
    using CooperAtkins.Interface.NotifyCom;
    using CooperAtkins.Generic;
    using CooperAtkins.SocketManager;

    public class PopupClient
    {
        private string _remoteHost;
        private int _remotePort;
        private string _alertMessage;
        private string _addressBookName;

        public PopupClient(INotifyObject notifyObject)
        {
            try
            {
                /* get remote popup settings. */
                _remoteHost = notifyObject.NotifierSettings["RemoteHost"].ToStr();
                _remotePort = notifyObject.NotifierSettings["RemotePort"].ToInt();
                _addressBookName = notifyObject.NotifierSettings["Name"].ToStr();
                _alertMessage = notifyObject.NotificationData.ToStr();
            }
            catch (Exception ex)
            {
                /*Debug Object values for reference*/
                LogBook.Debug(notifyObject, this);

                /*Write exception log*/
                LogBook.Write("Error has occurred while retrieving values from notification settings", ex, "CooperAtkins.NotificationServer.NotifyEngine.POPUP.PopupClient");
            }
        }
        /// <summary>
        /// Send Popup message
        /// </summary>
        /// <returns></returns>
        public NotifyComResponse Send()
        {
            NotifyComResponse notifyComResponse = new NotifyComResponse();
            try
            {

                /*Send Pop up using UDP Client*/
                NetworkClient networkClient = new NetworkClient();
                networkClient.UdpClient(_remoteHost, _remotePort, _alertMessage);

                /*Record notify response*/
                notifyComResponse.IsError = false;
                notifyComResponse.IsSucceeded = true;
                notifyComResponse.ResponseContent = "Popup message sent to [" + _addressBookName + "] " + _remoteHost;
            }
            catch (Exception ex)
            {
                /*Log remote port,host*/
                /*Record notify response*/
                notifyComResponse.IsError = true;
                notifyComResponse.IsSucceeded = false;
                notifyComResponse.ResponseContent = "Popup message to [" + _addressBookName + "] " + _remoteHost + " Failed.";

                /*Debug Object values for reference*/
                LogBook.Debug(notifyComResponse, this);

                /*Write exception log*/
                LogBook.Write("Error has occurred while sending popup to ." + _remoteHost, ex, "CooperAtkins.NotificationServer.NotifyEngine.POPUP.PopupClient");
            } 

            return notifyComResponse;
        }
    }
}
