/*
 *  File Name : SMSNotifyCom.cs
 *  Author : Pradeep.I 
 *           @ PCC Technology Group LLC
 *  Created Date : 12/29/2010
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
    public class SmsNotifyCom : INotifyCom
    {
        SmsClient client = new SmsClient();
        #region INotifyCom Members

        public NotifyComResponse Invoke(INotifyObject notifyObject)
        {
            object obj = new object();
            NotifyComResponse response = new NotifyComResponse();
            lock (obj)
            {
                try
                {
                    /*Write Log*/
                    LogBook.Write("Sending SMS to: " + notifyObject.NotifierSettings["ToPhoneNumber"].ToStr());

                    /* call the send method of sms client */
                    // we now set and write success / error messages in the send routine instead of here
                    response = client.Send(notifyObject);
                }
                catch (Exception ex)
                {
                    /*Write exception log*/
                    response.IsError = true;
                    response.IsSucceeded = false;
                    response.ResponseContent = "SMS to [" + notifyObject.NotifierSettings["ToName"].ToStr() + "] " + notifyObject.NotifierSettings["ToPhoneNumber"].ToStr() + ", Failed";
                    LogBook.Write(ex, "CooperAtkins.NotificationServer.NotifyEngine.SMSNotifyCom");
                }
            }
            return response;
        }
        public void UnLoad()
        {
            client.Close();
        }
        #endregion
    }
}
