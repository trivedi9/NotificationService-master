/*
 *  File Name : EmailNotifyCom.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/23/2010
 *  
 */
namespace CooperAtkins.NotificationServer.NotifyEngine
{
    using System;
    using System.ComponentModel.Composition;
    using CooperAtkins.Interface.NotifyCom;
    using CooperAtkins.Generic;

    /// <summary>
    /// 
    /// </summary>
    [Export(typeof(INotifyCom))]
    public class EmailNotifyCom : INotifyCom
    {
        #region INotifyCom Members

        public NotifyComResponse Invoke(INotifyObject notifyObject)
        {
            NotifyComResponse response;
            EmailClient client = new EmailClient(notifyObject);

            /*Log
                 * Notification data received from email notification composer 
                 * Sending Email...*/
            LogBook.Write("Notification data received from Email Notification Composer");
            LogBook.Write("Sending Email notification to: " + notifyObject.NotifierSettings["ToAddress"].ToStr());

            try
            {
                response = client.Send();
                /*Log : Sending response to Email Notification Composer */
                LogBook.Write("Sending response to Email Notification Composer");
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
                LogBook.Write(ex, "CooperAtkins.NotificationServer.NotifyEngine.INotifyCom");
            }
            return response;
        }
        public void UnLoad()
        { }
        #endregion
    }
}
