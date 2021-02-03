/*
 *  File Name : EmailNotificationComposer.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/24/2010
 */
namespace CooperAtkins.NotificationClient.NotificationComposer
{
    using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.ComponentModel.Composition;
    using CooperAtkins.Generic;
    using CooperAtkins.Interface.Alarm;
    using CooperAtkins.Interface.NotificationComposer;
    using CooperAtkins.NotificationClient.Generic.DataAccess;
    using CooperAtkins.NotificationClient.Generic;
    using CooperAtkins.NotificationClient.NotificationComposer.DataAccess;
    using CooperAtkins.Interface.NotifyCom;
    using EnterpriseModel.Net;


    [Export(typeof(INotificationComposer))]
    public class EmailNotificationComposer : INotificationComposer
    {

        /*Prepare log content */
        private string _logContent = "";

        #region INotificationComposer Members

        public INotifyObject[] Compose(AlarmObject alarmObject)
        {
            GenStoreInfo genStoreInfo = null;
            NotifyEmailAddressList notifyEmailAddressList = null;
            List<INotifyObject> notifyList = new List<INotifyObject>();
            NotificationStyle notificationStyle = new NotificationStyle();


            /*Get GenStore Information*/
            genStoreInfo = GenStoreInfo.GetInstance();


            if (alarmObject.IsMissCommNotification)
                _logContent = "Missed Communication";
            else
                _logContent = "SensorID: " + alarmObject.UTID.ToStr() + " SensorAlarmID: " + alarmObject.SensorAlarmID.ToStr();

            /*Write Log: 
             * Started Email Notification
             * Reached Email Notification Composer*/
            LogBook.Write("*** Started Composing E-Mail Notification for " + _logContent + "***");

            try
            {
                //If there's no record in the database, Execute method will return null 
                if (genStoreInfo != null)
                {
                    /*Record notification if SMTP settings are not supplied properly*/
                    if ((genStoreInfo.SmtpServer.ToStr() == string.Empty && genStoreInfo.SmtpAuthDomain == string.Empty) && genStoreInfo.SmtpSendMethod == 0)
                    {
                        /*SMTP server not defined*/
                        LogBook.Write(_logContent + " Error: No SMTP Server has been defined.");

                        /*Record notification if SMTP parameters are not supplied properly*/
                        notificationStyle.RecordNotification(ErrorMessages.EmailComposer_SMTPParmsNotSupplied, alarmObject.NotificationID,0, NotifyStatus.FAIL, NotifyTypes.EMAIL);
                    }
                    else
                    {
                        /*Get all the email addresses that are configured for notification*/
                        notifyEmailAddressList = GetNotifyEmailAddressList(alarmObject);

                        if (notifyEmailAddressList.Count != 0)
                        {
                            /*Construct a notification object for each email address and add to the notification list*/
                            notifyList = GetNotificationList(notifyEmailAddressList, alarmObject, genStoreInfo);
                            LogBook.Write("Email Notification count: " + notifyList.Count.ToString());
                        }
                        else
                        {
                            /*Log when we don't have Email address to notify*/
                            LogBook.Write(_logContent + " Error: Missing entry in NotifyEmails/Groups.");
                        }
                    }
                }
                else
                {
                    /*Record notification if genStore parameters are not supplied properly.*/
                    notificationStyle.RecordNotification(ErrorMessages.EmailComposer_GenStoreInvalid, alarmObject.NotificationID,0, NotifyStatus.FAIL, NotifyTypes.EMAIL);

                    /*Log if genStore information is empty*/
                    LogBook.Write(_logContent + " Error:  Missing GenStores table record.");
                }
            }
            catch (Exception ex)
            {
                /*Write exception log*/
                //LogBook.Write(_logContent + " " + ex);
                LogBook.Write(_logContent + " Error: ", ex, "CooperAtkins.NotificationClient.NotificationComposer.EmailNotificationComposer");
                //, "CooperAtkins.NotificationClient.NotificationComposer.EmailNotificationComposer");
            }

            //if (notifyList.Count > 0)
            /*Write Log
             * Sending Email notification data to Notification Engine*/
            LogBook.Write(_logContent + " Sending notification data to Email  Notification Engine.");

            /*Send notification list to notification engine*/
            return notifyList.ToArray();
        }

        /// <summary>
        /// Receive notification response.
        /// </summary>
        /// <param name="response"></param>
        public void Receive(NotifyComResponse response, INotifyObject notifyObject)
        {
            NotificationStyle notificationStyle = new NotificationStyle();

            /*Write Log
             Response received from Notification Engine*/
            LogBook.Write(_logContent + " Response received from Email Notification Engine.");

            /*Check the response object*/
            if (response != null)
            {
                /*Record notification information.*/
                notificationStyle.RecordNotification(response.ResponseContent.ToStr(), notifyObject.NotifierSettings["NotificationID"].ToInt(),0, (response.IsSucceeded ? NotifyStatus.PASS : NotifyStatus.FAIL), NotifyTypes.EMAIL);

                /*Log response content*/
                LogBook.Write(_logContent + " Email Response: " + response.ResponseContent.ToStr());
            }
        }

        #endregion

        /// <summary>
        /// Get E-mail Notification ID
        /// </summary>
        /// <param name="notifyProfileID"></param>
        /// <returns></returns>
        private int GetEmailNotificationID(int notifyProfileID)
        {
            NotificationProfile notificationProfile = null;
            int emailNotificationID = 0;
            try
            {
                /*Create NotificationProfile instance*/
                notificationProfile = new NotificationProfile();
                /*Send criteria notifyProfileID to get the Notification ID*/
                notificationProfile.NotifyProfileID = notifyProfileID;
                /*Execute*/
                notificationProfile = notificationProfile.Execute();
                /*Assign the EmailNotifyID to local variable emailNotificationID*/
                emailNotificationID = notificationProfile.EmailNotifyID;
            }
            catch (Exception ex)
            {
                /*Debug Object values for reference*/
                LogBook.Debug(notificationProfile, this);

                /*Write exception log*/
                LogBook.Write(_logContent + " Error has occurred while fetching EmailNotifyID ", ex, "CooperAtkins.NotificationClient.NotificationComposer.EmailNotificationComposer");
            }
            finally
            {
                /*Dispose the notificationProfile object*/
                notificationProfile.Dispose();
            }
            return emailNotificationID;
        }

        /// <summary>
        /// Get Notification E-Mail Address list based on Notification Profile ID
        /// </summary>
        /// <param name="alarmObject"></param>
        /// <returns></returns>
        private NotifyEmailAddressList GetNotifyEmailAddressList(Interface.Alarm.AlarmObject alarmObject)
        {
            NotifyEmailAddressList notifyEmailAddressList = null;
            try
            {
                /*Create NotifyEmailAddressList object*/
                notifyEmailAddressList = new NotifyEmailAddressList();
                /*Create Criteria object*/
                Criteria criteria = new Criteria();
                /*Get the Email notificationId and assign to criteria.ID*/
                criteria.ID = GetEmailNotificationID(alarmObject.NotifyProfileID);
                /*Gets list of notification email addresses*/
                notifyEmailAddressList.Load(criteria);
            }
            catch (Exception ex)
            {
                /*Debug Object values for reference*/
                LogBook.Debug(notifyEmailAddressList, this);

                /*Write exception log*/
                LogBook.Write(_logContent + " Error has occurred while fetching email address list from 'GenNotifyEmails' table", ex, "CooperAtkins.NotificationClient.NotificationComposer.EmailNotificationComposer");
            }
            finally
            {
                notifyEmailAddressList.Dispose();
            }
            return notifyEmailAddressList;
        }

        /// <summary>
        /// Get Email notification list
        /// </summary>
        /// <param name="notifyEmailAddressList"></param>
        /// <returns></returns>
        private List<INotifyObject> GetNotificationList(NotifyEmailAddressList notifyEmailAddressList, AlarmObject alarmObject, GenStoreInfo genStoreInfo)
        {
            List<INotifyObject> notifyList = new List<INotifyObject>();
            NotificationStyle notificationStyle = new NotificationStyle();

            foreach (NotifyEmailAddress notifyEmailAddress in notifyEmailAddressList)
            {
                /*Check for Notification Email Address and exit the sending process, if NotificationEmailAddress object is NULL*/
                if (notifyEmailAddress != null)
                {
                    /*Get Notification Object*/
                    INotifyObject notifyObject = GetNotifyObject(alarmObject, genStoreInfo, notifyEmailAddress);

                    if (notifyObject.NotifierSettings["FromAddress"].ToStr() == string.Empty || notifyObject.NotifierSettings["ToAddress"].ToStr() == string.Empty)
                    {
                        /*Record notification if Email (To/From) Address parameters are not supplied properly*/
                        notificationStyle.RecordNotification(ErrorMessages.EmailComposer_InvalidEmailAddress, alarmObject.NotificationID,0, NotifyStatus.FAIL, NotifyTypes.EMAIL);
                    }
                    else
                    {
                        //Add notification object to array list
                        notifyList.Add(notifyObject);
                    }
                }
                else
                {
                    /*Record notification if Email Address parameters are not supplied properly*/
                    notificationStyle.RecordNotification(ErrorMessages.EmailComposer_EmailParamsNotSupplied, alarmObject.NotificationID,0, NotifyStatus.FAIL, NotifyTypes.EMAIL);
                }
            }
            return notifyList;
        }

        /// <summary>
        /// Create Notification Object
        /// </summary>
        /// <param name="genStoreDAO"></param>
        /// <returns></returns>
        private NotifyObject GetNotifyObject(Interface.Alarm.AlarmObject alarmObject, GenStoreInfo genStoreInfo, NotifyEmailAddress notifyEmailAddress)
        {
            bool isAlphaPager = false, isSSL = false, isTSL = false, isBodyHtml = false;
            int bodyFormat = Common.FORMAT_TEXT, smtpPort = 0, timeOut = 15000;
            string subject = "", body = "", emailToAddress = "", emailFromAddress = "", name = "", fromName = "TempTrak Monitor";
            string defaultSubject = "", defaultBody = "", defaultPagerSubject = "", defaultPagerBody = "", notifyTree = "";
            Int16 alphaPager = 0;

            /*Initialize NotifyObject*/
            NotifyObject notifyObject = new NotifyObject();
            /*Initialize NotificationStyle*/

            NotificationStyle notificationStyle = new NotificationStyle();

            /*Set SSL and TLS flags*/
            isSSL = (genStoreInfo.SmtpFlags & 1) == 1 ? true : false;
            isTSL = (genStoreInfo.SmtpFlags & 2) == 2 ? true : false;

            /*Set default SMTP Port value*/
            if (genStoreInfo.SmtpPort.ToInt() == 0 || genStoreInfo.SmtpPort.ToInt() < 1 || genStoreInfo.SmtpPort.ToInt() > 32767)
            {
                /*Default SMTP port number*/
                smtpPort = 25;
                /*If SSL is enables set SMTP port to 465*/
                if (isSSL)
                    smtpPort = 465;
            }
            else
                smtpPort = genStoreInfo.SmtpPort.ToInt();

            /*Email Address*/
            emailToAddress = notifyEmailAddress.EmailAddress;

            /*Email notification name*/
            name = notifyEmailAddress.Name;

            /*Alpha Pager*/
            alphaPager = notifyEmailAddress.AlphaPager;

            /*Default Subject*/
            defaultSubject = notificationStyle.GetFormatString(alarmObject, 1, "EmailSubject");

            /*Default Body*/
            defaultBody = notificationStyle.GetFormatString(alarmObject, 1, "EmailBody");

            /*Default Pager Subject*/
            defaultPagerSubject = notificationStyle.GetFormatString(alarmObject, 1, "EmailSubjectPager");

            /*Default Pager Body*/
            defaultPagerBody = notificationStyle.GetFormatString(alarmObject, 1, "EmailBodyPager");

            /*Notification Tree*/
            notifyTree = notificationStyle.GetFormatString(alarmObject, 1, "NotifyTree");

            /*Default Pager Subject*/
            defaultPagerSubject = (defaultPagerSubject == string.Empty) ? defaultSubject : defaultPagerSubject;

            /*Default Pager Subject*/
            defaultPagerBody = (defaultPagerBody == string.Empty) ? defaultBody : defaultPagerBody;

            ///*Set Email Body and Subject format*/
            if (alphaPager == 0)
            {
                isAlphaPager = false;
                subject = defaultSubject;
                if (body.ToStr() == string.Empty)
                {
                    if (alarmObject.AlarmType != AlarmType.COMMUNICATIONS && alarmObject.AlarmType != AlarmType.RESETCOMMUNICATIONS)
                    {
                        body = notificationStyle.GetEmailBody(alarmObject);
                    }
                    else
                    {
                        if (alarmObject.AlarmType == AlarmType.COMMUNICATIONS)
                        {
                            bodyFormat = Common.FORMAT_HTML;
                            body = "<html><head><style>TH{font-family: Verdana,Arial;font-weight: bold;font-size: 10pt;background: #333333;color: white;}TD{font-family: Verdana,Arial;font-weight: normal;font-size: 10pt;}</style></head><body style='font-family: Verdana, Arial; font-size: 10pt;'><span style='font-weight: bold; font-size: 14pt; background: black; color: yellow;'>!!! Missed Communication !!!</span><br /><br /><br /><span style='font-weight: bold;'>Sensor Name: %%NAME%%</span><br>FactoryID: %%ID%%<br /><br /><table border='0' cellpadding='1' cellspacing='1'><tr><th align='left'>TYPE:</th><td>%%TYPE%%</td></tr><tr><th align='left'>GROUP:</th><td>%%GROUP%%</td></tr><tr><th align='left'>Time out of range:</th><td>%%TimeOutOfRange%%</td></tr></table></body></html>";
                        }
                        else
                        {
                            bodyFormat = Common.FORMAT_HTML;
                            body = "<html><head><style>TH{font-family: Verdana,Arial;font-weight: bold;font-size: 10pt;background: #333333;color: white;}TD{font-family: Verdana,Arial;font-weight: normal;font-size: 10pt;}</style></head><body style='font-family: Verdana, Arial; font-size: 10pt;'><span style='font-weight: bold; font-size: 14pt; background: black; color: yellow;'>!!! Back In Sight/Connection Resumed !!!</span><br /><br /><br /><span style='font-weight: bold;'>Sensor Name: %%NAME%%</span><br>FactoryID: %%ID%%<br /><br /><table border='0' cellpadding='1' cellspacing='1'><tr><th align='left'>TYPE:</th><td>%%TYPE%%</td></tr><tr><th align='left'>GROUP:</th><td>%%GROUP%%</td></tr><tr><th align='left'>Time out of range:</th><td>%%TimeOutOfRange%%</td></tr></table></body></html>";
                        }
                    }

                    if (body.ToStr() == string.Empty)
                    {
                        body = defaultBody;
                    }
                    else
                    {
                        bodyFormat = Common.FORMAT_HTML;
                        if (defaultBody.ToStr() != string.Empty)
                        {
                            defaultBody = defaultBody.Replace("\\n", "<br>");
                            body = body + "<BR></BR>" + defaultBody;
                        }
                    }

                    if (notifyTree.ToStr() != string.Empty)
                    {
                        notifyTree = notifyTree.Replace("\\n", "<br>");
                        body = body + "<BR><BR><TABLE WIDTH=500 style='padding:5px;'><TR><TD><HR><H4>Alarm Notification Action(s)</H4></TD></TR><TR><TD style='border:1px dotted #000000;background:#99ffff;'>" + notifyTree + "</TD></TR></TABLE>";
                    }

                }
            }
            else /*Alpha pager*/
            {
                isAlphaPager = true;
                subject = defaultPagerSubject;
                body = defaultPagerBody.Replace("\\n", Environment.NewLine);
            }
            /*Assign default subject and body values*/
            subject = (subject == string.Empty) ? defaultSubject : subject;
            body = (body.ToStr() == string.Empty) ? "" : body;


            /*commented on 02/28/2011 to display from email address properly*/
            /*Get a default "From" address*/
            //if (emailToAddress != string.Empty)
            //{
            //    foreach (string item in emailToAddress.ToStr().Split(','))
            //    {
            //        emailFromAddress = item;
            //        break;
            //    }
            //}

            emailFromAddress = genStoreInfo.FromAddress;

            /*Format Subject*/
            subject = notificationStyle.SubstituteFormatString(subject, alarmObject);

            /*Format Body*/
            body = notificationStyle.SubstituteFormatString(body, alarmObject);

            /*Body Format*/
            isBodyHtml = (bodyFormat == Common.FORMAT_HTML) ? true : false;

            /*In case of missed communication set the custom message*/
            if (alarmObject.IsMissCommNotification)
            {
                subject = "Missed Communication"; //set the appropriate message
                body = alarmObject.MissedCommSensorInfo;
                isBodyHtml = true;
            }

            /*Set Email From Name*/
            fromName = (genStoreInfo.FromName != string.Empty) ? genStoreInfo.FromName : fromName;

            /*Assigning values to notification object*/
            notifyObject.NotificationType = "EMAIL";

            /*Assign values to Notification settings*/
            Hashtable notificationSettings = new Hashtable();
            notificationSettings.Add("SMTPServer", genStoreInfo.SmtpServer);
            /*0=via SMTPGateway, 1=MX Direct*/
            notificationSettings.Add("SMTPSendMethod", genStoreInfo.SmtpSendMethod);
            notificationSettings.Add("SMTPAuthUserName", genStoreInfo.SmtpAuthUserName);
            notificationSettings.Add("SMTPAuthPassword", genStoreInfo.SmtpAuthPassword);
            notificationSettings.Add("SMPTAuthDomain", genStoreInfo.SmtpAuthDomain);
            notificationSettings.Add("SMTPAuthMethod", genStoreInfo.SMTPAuthMethod);
            notificationSettings.Add("SMTPFlags", genStoreInfo.SmtpFlags);
            /*Bitmask: Bit 1=Use SSL, 2=Use TLS*/
            notificationSettings.Add("IsSSL", isSSL);
            notificationSettings.Add("IsTLS", isTSL);
            notificationSettings.Add("SMTPPort", smtpPort);
            notificationSettings.Add("BodyFormat", bodyFormat);
            notificationSettings.Add("FromAddress", emailFromAddress);
            notificationSettings.Add("ToAddress", emailToAddress);
            notificationSettings.Add("Subject", subject);
            notificationSettings.Add("EmailToName", name);
            notificationSettings.Add("FromName", fromName);
            notificationSettings.Add("IsAlphaPager", isAlphaPager);
            notificationSettings.Add("IsBodyHTML", isBodyHtml);
            //notificationSettings.Add("ReadTimeOut", timeOut); //Default Time out value 15 Sec in old code
            notifyObject.NotifierSettings = notificationSettings;
            /* Notification Data */
            notifyObject.NotificationData = body;
            notifyObject.NotifierSettings.Add("NotificationID", alarmObject.NotificationID);

            return notifyObject;
        }

    }
}
