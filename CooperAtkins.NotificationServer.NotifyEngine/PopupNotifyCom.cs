/*
 *  File Name : PopupNotifyCom.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/29/2010
 */

namespace CooperAtkins.NotificationServer.NotifyEngine
{
    using System;
    using System.ComponentModel.Composition;
    using CooperAtkins.Interface.NotifyCom;
    using CooperAtkins.Generic;

    [Export(typeof(INotifyCom))]
    public class PopupNotifyCom : INotifyCom
    {
        #region INotifyCom Members

        public NotifyComResponse Invoke(INotifyObject notifyObject)
        {
            NotifyComResponse response;
            POPUP.PopupClient client = new POPUP.PopupClient(notifyObject);

            /*Log 
                 * Notification data received from Pager notification composer 
                 * Sending Pager...*/
            LogBook.Write("Notification data received from Popup Notification Composer");
            LogBook.Write("Sending Popup notification to: " + notifyObject.NotifierSettings["RemoteHost"].ToStr());

            try
            {
                /*sending notification.*/
                response = client.Send();

                /*Log : Sending response to Email Notification Composer */
                LogBook.Write("Sending response to Popup Notification Composer");
            }
            catch (Exception ex)
            {
                response = new NotifyComResponse(); 
                response.IsSucceeded = true;
                response.IsError = false;

                /*Debug Object values for reference*/
                LogBook.Debug(response, this);

                /*Write exception log*/
                LogBook.Write(ex, "CooperAtkins.NotificationServer.NotifyEngine.PopupNotifyCom");
            }
            return response;
        }

        public void UnLoad()
        {

        }
        #endregion
    }
}
