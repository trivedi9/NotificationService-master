/*
 *  File Name : INotifyCom.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/23/2010
 */
namespace CooperAtkins.NotificationServer.NotifyEngine
{
    using System;
    using System.Text;
    using System.Net.Mail;
    using System.Net;
    using SendSMTP;
    using CooperAtkins.Interface.NotifyCom;
    using CooperAtkins.Generic;
    using System.Runtime.InteropServices;

    public class EmailClient
    {
        private string _smtpServer;
        private int _sendMethod;
        private string _fromAddress;
        private string _toAddress;
        private string _fromName;
        private int _smtpPort;
        private string _subject;
        private string _body;
        private string _smtpAuthUserName;
        private string _smtpAuthPassword;
        private string _smtpAuthDomain;
        private string _smtpAuthMethod;
        private bool _isSSL;
        private bool _isTLS;
        private string _emailToName;
        private int _readTimeOut;
        private bool _isBodyHTML;
        private bool _isAlphaPager;

        public EmailClient(INotifyObject notifyObject)
        {

            try
            {
                /*Set Email parameters*/
                _smtpServer = notifyObject.NotifierSettings["SMTPServer"].ToStr();
                _sendMethod = notifyObject.NotifierSettings["SMTPSendMethod"].ToInt();
                _fromAddress = notifyObject.NotifierSettings["FromAddress"].ToStr();
                _toAddress = notifyObject.NotifierSettings["ToAddress"].ToStr();
                _fromName = notifyObject.NotifierSettings["FromName"].ToStr();
                _smtpPort = notifyObject.NotifierSettings["SMTPPort"].ToInt();
                _subject = notifyObject.NotifierSettings["Subject"].ToStr();
                _smtpAuthUserName = notifyObject.NotifierSettings["SMTPAuthUserName"].ToStr();
                _smtpAuthPassword = notifyObject.NotifierSettings["SMTPAuthPassword"].ToStr();
                _smtpAuthDomain = notifyObject.NotifierSettings["SMPTAuthDomain"].ToStr();
                _smtpAuthMethod = notifyObject.NotifierSettings["SMTPAuthMethod"].ToStr();
                _isSSL = notifyObject.NotifierSettings["IsSSL"].ToBoolean();
                _isTLS = notifyObject.NotifierSettings["IsTLS"].ToBoolean();
                _emailToName = notifyObject.NotifierSettings["EmailToName"].ToStr();
                _readTimeOut = notifyObject.NotifierSettings["ReadTimeOut"].ToInt();
                _isBodyHTML = notifyObject.NotifierSettings["IsBodyHTML"].ToBoolean();
                _isAlphaPager = notifyObject.NotifierSettings["IsAlphaPager"].ToBoolean();
                _body = notifyObject.NotificationData.ToStr();
            }
            catch (Exception ex)
            {
                /*Debug Object values for reference*/
                LogBook.Debug(notifyObject, this);

                /*Write exception log*/
                LogBook.Write("Error has occurred while retrieving values from notification settings", ex, "CooperAtkins.NotificationServer.NotifyEngine.Email.EmailClient");
            }
        }


        public EmailClient(String SMTPServer, Int32 SMTPSendMethod, String FromAddress, String ToAddress, String FromName, Int32 SMTPPort, String Subject, String SMTPAuthUserName, String SMTPAuthPassword, String SMPTAuthDomain, String SMTPAuthMethod, Boolean IsSSL, Boolean IsTLS, Boolean isBodyHTML, Boolean isAlphaPager, String EmailToName, String Message)
        {


            /*Set Email parameters*/
            _smtpServer = SMTPServer;
            _sendMethod = SMTPSendMethod;
            _fromAddress = FromAddress;
            _toAddress = ToAddress;
            _fromName = FromName;
            _smtpPort = SMTPPort;
            _subject = Subject;
            _smtpAuthUserName = SMTPAuthUserName;
            _smtpAuthPassword = SMTPAuthPassword;
            _smtpAuthDomain = SMPTAuthDomain;
            _smtpAuthMethod = SMTPAuthMethod;
            _isSSL = IsSSL;
            _isTLS = IsTLS;
            _emailToName = EmailToName;
            _readTimeOut = 99;
            _isBodyHTML = isBodyHTML;
            _isAlphaPager = isAlphaPager;
            _body = Message;


        }

        /// <summary>
        /// Send Mail
        /// </summary>
        /// <returns></returns>
        public NotifyComResponse Send()
        {
            NotifyComResponse notifyComResponse = new NotifyComResponse();
            try
            {

                try
                {
                    MailAddress ma = new MailAddress(_fromAddress);
                }
                catch (FormatException ex)
                {
                    // invalid from mail address, set it to TempTrak@cooper-atkins.com
                    LogBook.Write("The format for the from email address (" + _fromAddress + ") is incorrect. TempTrak@cooper-atkins.com will be used instead.");
                    _fromAddress = "TempTrak@cooper-atkins.com";
                }

                SmtpClient ss = new SmtpClient(_smtpServer, _smtpPort);


                MailMessage mm = new MailMessage(_fromAddress, _toAddress, _subject, _body);

                CDO.Message message = new CDO.Message();
                /*Create Mail Message Object*/
                MailMessage mailObj = new MailMessage();
                /*Email from address*/
                mailObj.From = new MailAddress(_fromAddress, _fromName);
                /*Email to address*/
                mailObj.To.Add(new MailAddress(_toAddress));
                /*Email subject*/
                mailObj.Subject = _subject;
                /*Email Body Encoding*/
                mailObj.BodyEncoding = Encoding.Default;
                /*Email Body*/
                mailObj.Body = _body;
                /*Body format (HTML/Text)*/
                mailObj.IsBodyHtml = _isBodyHTML;

                /*Via SMTP Gateway (i.e. your local Exchange Server)-> SmtpSendMethod = 0*/
                /*Via Direct Domain SMTP Connection w/DNS MX Lookup-> SmtpSendMethod = 1*/
                /*When SmtpSendMethod = 1 we are sending via local host instead of using SMTP settings*/
                SmtpClient smtpClientObj = null;
                if (_sendMethod == 1)
                {
                    //Send message
                    string domain = mailObj.To[0].Address.Substring(mailObj.To[0].Address.IndexOf('@') + 1);
                    //To Do :need to check for MX record existence before you send. Left intentionally for you.
                    string mxRecord = SendSMTP.DnsLookUp.GetMXRecords(domain)[0];
                    smtpClientObj = new SmtpClient(mxRecord);
                }
                else
                {


                    if (_isTLS == true && _isSSL == false)
                    {

                        ss.EnableSsl = true;
                        ss.Timeout = 20000;
                        ss.DeliveryMethod = SmtpDeliveryMethod.Network;
                        ss.UseDefaultCredentials = false;
                        ss.Credentials = new NetworkCredential(_smtpAuthUserName, _smtpAuthPassword);

                        mm.BodyEncoding = UTF8Encoding.UTF8;
                        mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                        mm.IsBodyHtml = _isBodyHTML;

                    }
                    else
                    {

                        CDO.IConfiguration configuration = message.Configuration;
                        ADODB.Fields fields = configuration.Fields;


                        ADODB.Field field = fields["http://schemas.microsoft.com/cdo/configuration/smtpserver"];
                        field.Value = _smtpServer;

                        field = fields["http://schemas.microsoft.com/cdo/configuration/smtpserverport"];
                        field.Value = _smtpPort;

                        field = fields["http://schemas.microsoft.com/cdo/configuration/sendusing"];
                        field.Value = CDO.CdoSendUsing.cdoSendUsingPort;

                        field = fields["http://schemas.microsoft.com/cdo/configuration/smtpauthenticate"];

                        if (_smtpAuthMethod == "" || _smtpAuthMethod.ToUpper() == "NONE")
                            field.Value = CDO.CdoProtocolsAuthentication.cdoAnonymous;
                        else if (_smtpAuthMethod.ToUpper() == "NTLM")
                            field.Value = CDO.CdoProtocolsAuthentication.cdoNTLM;
                        else
                            field.Value = CDO.CdoProtocolsAuthentication.cdoBasic;

                        field = fields["http://schemas.microsoft.com/cdo/configuration/sendusername"];
                        field.Value = _smtpAuthUserName;

                        field = fields["http://schemas.microsoft.com/cdo/configuration/sendpassword"];
                        field.Value = _smtpAuthPassword;

                        field = fields["http://schemas.microsoft.com/cdo/configuration/smtpusessl"];
                        field.Value = _isSSL;

                        field = fields["http://schemas.microsoft.com/cdo/configuration/smtpconnectiontimeout"];
                        field.Value = 10;

                        fields.Update();

                        message.From = @"""" + _fromName + @""" <" + _fromAddress + ">"; ;
                        message.To = _toAddress;
                        message.Subject = _subject;
                        if (_isBodyHTML)
                            message.HTMLBody = _body;
                        else
                            message.TextBody = _body;


                    }


                }

                try
                {
                    if (_sendMethod == 1)
                    {
                        smtpClientObj.Send(mailObj);
                    }
                    else
                    {
                        /*Send Mail*/
                        if (_isTLS == true && _isSSL == false)
                        {
                            ss.Send(mm);
                        }
                        else
                        {
                            message.Send();
                        }

                    }

                    /*Record notify response*/
                    notifyComResponse.IsError = false;
                    notifyComResponse.IsSucceeded = true;
                    notifyComResponse.ResponseContent = "Email sent to: " + "[" + _emailToName + "]" + _toAddress + ((_isAlphaPager) ? " (ALPHA PAGER) " : "");
                }
                catch (Exception ex)
                {
                    /*Record notify response*/
                    notifyComResponse.IsError = true;
                    notifyComResponse.IsSucceeded = false;
                    notifyComResponse.ResponseContent = "Email to: " + "[" + _emailToName + "]" + _toAddress + ((_isAlphaPager) ? " (ALPHA PAGER) " : "") + " Failed " + ex.Message;

                    /*Debug Object values for reference*/
                    LogBook.Debug(notifyComResponse, this);

                    /*Write exception log*/
                    LogBook.Write("Error has occurred while sending email to ." + _emailToName, ex, "CooperAtkins.NotificationServer.NotifyEngine.Email.EmailClient");
                }
                finally
                {
                    // Added on 2/19/2012
                    // Srinivas Rao Eranti
                    // Added try catch to release the message object
                    try
                    {
                        Marshal.FinalReleaseComObject(message);
                        // GC.SuppressFinalize(message);
                    }
                    catch
                    {
                    }
                }

            }
            catch (Exception ex)
            {
                /*Record notify response*/
                notifyComResponse.IsError = true;
                notifyComResponse.IsSucceeded = false;
                notifyComResponse.ResponseContent = "Email to: " + "[" + _emailToName + "]" + _toAddress + ((_isAlphaPager) ? "(ALPHA PAGER)" : "") + " Failed " + ex.Message;


                /*Debug Object values for reference*/
                LogBook.Debug(notifyComResponse, this);

                /*Write exception log*/
                LogBook.Write("Error has occurred while preparing SMTP setting.", ex, "CooperAtkins.NotificationServer.NotifyEngine.Email.EmailClient");
            }
            return notifyComResponse;
        }


    }
}
