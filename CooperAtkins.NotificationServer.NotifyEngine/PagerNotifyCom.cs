/*
 *  File Name : PagerNotifyCom.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/29/2010
 */


namespace CooperAtkins.NotificationServer.NotifyEngine
{
    using System;
    using CooperAtkins.Interface.NotifyCom;
    using System.ComponentModel.Composition;
    using CooperAtkins.Generic;
    using System.Threading;

    [Export(typeof(INotifyCom))]
    public class PagerNotifyCom : INotifyCom
    {
        #region INotifyCom Members

        public NotifyComResponse Invoke(INotifyObject notifyObject)
        {
            NotifyComResponse response = null;
            PagerHelper client = new PagerHelper(notifyObject);

            /*Log 
                 * Notification data received from Pager notification composer 
                 * Sending Pager...*/
            LogBook.Write("Notification data received from Pager Notification Composer");
            LogBook.Write("Sending Pager notification to: " + notifyObject.NotifierSettings["ToAddress"].ToStr());

            try
            {
                int waitingSecs = 0;
                response = client.Send(notifyObject);
                while (!client.ProcessCompleted)
                {
                    Thread.Sleep(1 * 1000);
                    waitingSecs++;
                    if (waitingSecs > 120)
                    {
                        client.Message += "\r\nNo response from last 120 seconds, terminating the process";
                        response.IsError = true;
                        break;
                    }
                }

                if (notifyObject.NotifierSettings["DeliveryMethod"].ToInt() != 1)
                {
                    response.ResponseContent = client.Message;
                }

                /*Log : Sending response to Email Notification Composer */
                LogBook.Write("Sending response to Pager Notification Composer");
            }
            catch (Exception ex)
            {
                /*Debug Object values for reference*/
                LogBook.Debug(response, this);

                /*Write exception log*/
                LogBook.Write(ex, "CooperAtkins.NotificationServer.NotifyEngine.INotifyCom");
            }
            return response;
        }

        public void UnLoad()
        {

        }
        #endregion
    }
}
