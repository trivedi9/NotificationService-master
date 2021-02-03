/*
 *  File Name : LogBook.cs
 *  Author : Rajes Jinaga
 *  @ PCC Technology Group LLC
 *  Created Date : 11/25/2010
 */

namespace CooperAtkins.Generic
{
    using System;
    using System.Text;
    using System.IO;
    using System.Net.Mail;
    using System.Configuration;
    using System.Threading;


    public enum ErrorSeverity
    {
        /* Normal */
        Normal = 1,
        /* Medium */
        Medium = 2,
        /* High */
        High = 3,
        /* E-mail will be sent, if the error severity is defined as critical */
        Critical = 4
    }

    /// <summary>
    /// LogBook
    /// </summary>
    public static class LogBook
    {
        static bool isDebug = false;
        static KTGUTIL2Lib.AppLog log;
        static string _logFilepath = string.Empty;
        static string _appName = string.Empty;
        static bool isFirstTime = false;
        public static object lockObj = new object();

        public static void SetLogFilePath(string logFilepath)
        {
            _logFilepath = logFilepath;
        }
        public static void SetAppName(string appName)
        {
            isFirstTime = true;
            _appName = appName == null ? "" : appName;
        }
        
        static LogBook()
        {
            log = new KTGUTIL2Lib.AppLog();
            //log.Init("NotificationService", ConfigurationManager.AppSettings["LogPath"], 1000000);
            isDebug = ConfigurationManager.AppSettings["IsDebug"].ToBoolean();
        }

        public static void Write(string logText)
        {
            try
            {
                //return;
                int i;
                if (isFirstTime)
                    i = log.Init("NotificationService" + _appName, _logFilepath, 10000000);
                isFirstTime = false;

                if (_appName == string.Empty)
                {
                    _appName = "NotificationService";
                    log.Init("NotificationService", ConfigurationManager.AppSettings["LogPath"], 10000000);
                }

                #region CommentedOut - debugShowServiceIsAlive
                //writes a dot to log file when a timer fires to show service is alive and operating when there is nothing else to log
                /*
                if (logText == "...")
                {
                    log.WriteLog(".");
                }
                else
                {
                    log.WriteLog(DateTime.Now.ToString() + ": DBName: " + _appName + " Message: " + logText + Environment.NewLine);
                } 
                 */
                #endregion

                log.WriteLog(DateTime.Now.ToString() + ": DBName: " + _appName + " Message: " + logText + Environment.NewLine);

                //using (StreamWriter sw = new StreamWriter("c:\\log.txt", true))
                //{
                //    sw.WriteLine(DateTime.Now.ToString() + ": " + logText);
                //    sw.Flush();
                //}
                
            }
            catch { }

        }
        public static void Write(string formatString, params object[] parameters)
        {

        }
        public static void Write(string formatString, string module, params object[] parameters)
        {

        }
        public static void Write(string logText, string module)
        {
            //log.WriteLog(DateTime.Now.ToString() + "Module :" + module +" Message:" + logText + "\r\n");
            //using (StreamWriter sw = new StreamWriter("c:\\log1.txt", true))
            //{
            //    sw.WriteLine(logText);
            //    sw.Flush();
            //}
        }
        public static void Write(string logText, string module, ErrorSeverity serverity)
        {
            
            try
            {
                lock (lockObj)
                {
                    log.WriteLog(DateTime.Now.ToString() + " :DBName: " + _appName + " : Message: " + logText + Environment.NewLine);
                    //using (StreamWriter sw = new StreamWriter("c:\\log.txt", true))
                    //{
                    //    sw.WriteLine(DateTime.Now.ToString() + ": Message:" + logText);
                    //    sw.Flush();
                    //}
                    if (serverity == ErrorSeverity.Critical)
                    {
                        SendMail(logText);
                    }

                }
            }
            catch { }
        }
        public static void Write(Exception exception, string module)
        {
            object obj = new object();
            try
            {
                lock (obj)
                {


                    log.WriteLog(DateTime.Now.ToString() +" "+ module+" : DBName: " + _appName +": Message: " + exception.Message
                                    + Environment.NewLine + ", Inner Exception: " + (exception.InnerException == null ? "" : exception.InnerException.Message)
                                    + ": Stack Trace:" + exception.StackTrace + Environment.NewLine);


                    //using (StreamWriter sw = new StreamWriter("c:\\log.txt", true))
                    //{
                    //    sw.WriteLine(DateTime.Now.ToString() + ": Message:" + exception.Message
                    //                    + "\r\n" + ", Inner Exception: " + (exception.InnerException == null ? "" : exception.InnerException.Message)
                    //                    + ": Stack Trace:" + exception.StackTrace);
                    //    sw.Flush();
                    //}
                }
            }
            catch { }
        }
        public static void Write(Exception exception, string module, ErrorSeverity severity)
        {
            object obj = new object();
            try
            {
                lock (obj)
                {
                    log.WriteLog(DateTime.Now.ToString() + ": DBName: " + _appName + ": Message: " + exception.Message
                                        + Environment.NewLine + ", Inner Exception: " + (exception.InnerException == null ? "" : exception.InnerException.Message)
                                        + ": Stack Trace:" + exception.StackTrace + Environment.NewLine);
                    //using (StreamWriter sw = new StreamWriter("c:\\log.txt", true))
                    //{
                    //    sw.WriteLine(DateTime.Now.ToString() + ": Message:" + exception.Message
                    //                    + "\r\n" + ", Inner Exception: " + (exception.InnerException == null ? "" : exception.InnerException.Message)
                    //                    + ": Stack Trace:" + exception.StackTrace);
                    //    sw.Flush();
                    //}
                }
            }
            catch { }
        }
        public static void Write(string logText, Exception exception, string module)
        {
            object obj = new object();
            try
            {
                lock (obj)
                {
                    log.WriteLog(DateTime.Now.ToString() + ": DBName: " + _appName + " " + logText +  Environment.NewLine + "Message: " + exception.Message
                                        + Environment.NewLine + ", Inner Exception: " + (exception.InnerException == null ? "" : exception.InnerException.Message)
                                        + ": Stack Trace:" + exception.StackTrace + Environment.NewLine);
                    //using (StreamWriter sw = new StreamWriter("c:\\log.txt", true))
                    //{
                    //    sw.WriteLine(DateTime.Now.ToString() + ": Message:" + exception.Message
                    //                    + "\r\n" + ", Inner Exception: " + (exception.InnerException == null ? "" : exception.InnerException.Message)
                    //                    + ": Stack Trace:" + exception.StackTrace);
                    //    sw.Flush();
                    //}
                }
            }
            catch { }
        }
        public static void Debug(object sender, object currentObject)
        {

        }
        public static void Debug(string logText)
        {
            try
            {
                if (isDebug)
                    log.WriteLog(DateTime.Now.ToString() + ": DBName: " + _appName + ": Message: " + logText + Environment.NewLine);
            }
            catch { }
        }
        private static void SendMail(string logMessage)
        {
            MailMessage mailObj = new MailMessage();
            string fromAddress = ConfigurationManager.AppSettings["FromEmailAddress"].ToStr();
            string body = logMessage.Substring(logMessage.IndexOf("\r\n"), logMessage.Length - logMessage.IndexOf("\r\n") - 1);
            string subject = logMessage.Substring(0, logMessage.Length - body.Length - 1);

            foreach (string s in ConfigurationManager.AppSettings["ToEmailAddress"].ToStr().Split(','))
            {
                /*Email to address*/
                mailObj.To.Add(new MailAddress(s));
            }
            /*Email subject*/
            mailObj.Subject = subject;
            /*Email Body Encoding*/
            mailObj.BodyEncoding = Encoding.Default;
            /*Email Body*/
            mailObj.Body = body;
            /*Body format (HTML/Text)*/
            mailObj.IsBodyHtml = false;

            SmtpClient smtpClientObj = new SmtpClient();
            /*Send Mail*/
            smtpClientObj.Send(mailObj);

        }
    }
}
