/*
 *  File Name : ScriptNotifyCom.cs
 *  Author : Pradeep
 *  @ PCC Technology Group LLC
 *  Created Date : 11/30/2010
 */

namespace CooperAtkins.NotificationServer.NotifyEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using CooperAtkins.Interface.NotifyCom;
    using CooperAtkins.Generic;
    using System.ComponentModel.Composition;

    [Export(typeof(INotifyCom))]
    public class ScriptNotifyCom : INotifyCom
    {
        public NotifyComResponse Invoke(INotifyObject notifyObject)
        {
            NotifyComResponse response;
            ScriptHelper client = new ScriptHelper(notifyObject);
            /*Log
                 * Notification data received from email notification composer 
                 * Executing Script...*/
            LogBook.Write("Notification data received from Script Notification Composer");
            LogBook.Write("Executing Script");

            try
            {
                /* sending notification.*/
                response = client.Send();

                /*Log : Sending response to Script Notification Composer */
                LogBook.Write("Sending response to Script Notification Composer");
            }
            catch (Exception ex)
            {
                response = new NotifyComResponse();
                response.IsSucceeded = false;
                response.IsError = true;
                //response.ResponseContent = ex.Message + "\n" + ex.StackTrace;

                /*Debug Object values for reference*/
                LogBook.Debug(response, this);

                /*Write exception log*/
                LogBook.Write(ex, "CooperAtkins.NotificationServer.NotifyEngine.ScriptNotifyCom");
            }
            return response;
        }


        public void UnLoad()
        {

        }
    }
}
