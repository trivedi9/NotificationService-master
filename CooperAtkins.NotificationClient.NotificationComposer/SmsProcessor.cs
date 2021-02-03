using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CooperAtkins.Interface.Alarm;
using CooperAtkins.Interface.NotifyCom;
using CooperAtkins.Generic;

namespace CooperAtkins.NotificationClient.NotificationComposer
{
    public class SmsProcessor
    {
        private const Int16 MAX_ATTEMPTS = 5;
        public static bool SendSms(INotifyObject notifyObject)
        {
            if (notifyObject.NotifierSettings["AttemptCount"].ToInt16() > MAX_ATTEMPTS)
            {
                LogBook.Write("Max attempts (" + MAX_ATTEMPTS.ToString() + ") completed for current page and removing from queue.");
                new NotificationStyle().RecordNotification("Max attempts (" + MAX_ATTEMPTS.ToString() + ") completed for current sms and removing from queue.", notifyObject.NotifierSettings["NotificationID"].ToInt(),0, NotifyStatus.FAIL, NotifyTypes.SMS);
                return true;
            }


            /* get end point for voice composer*/
            NotificationEndPointElement element;

            NotificationComposer.NotificationClient _client = NotificationComposer.NotificationClient.GetInstance();
            _client.WhoAmI("Sms", out element);
            NotifyComResponse response = _client.InvokeNotifyEngine(notifyObject, element);

            /*Check the response object*/
            if (response != null)
            {
                /*Record notification information.*/
                new NotificationStyle().RecordNotification(response.ResponseContent.ToStr(), notifyObject.NotifierSettings["NotificationID"].ToInt(),0, (response.IsSucceeded ? NotifyStatus.PASS : NotifyStatus.FAIL), NotifyTypes.SMS);

                /*Log response content*/
                LogBook.Write(" Sms Response: " + response.ResponseContent.ToStr());
            }

            return response.IsSucceeded;
        }
    }
}
