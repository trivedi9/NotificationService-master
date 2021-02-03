/*
 *  File Name : ScriptClient.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/30/2010
 */

namespace CooperAtkins.NotificationServer.NotifyEngine
{
    using System;
    using System.Diagnostics;
    using CooperAtkins.Interface.NotifyCom;
    using CooperAtkins.Generic;

    public class ScriptHelper
    {
        private string _scriptName;
        private string _scriptArguments;

        public ScriptHelper(INotifyObject notifyObject)
        {
            try
            {
                /* get settings information.*/
                _scriptName = notifyObject.NotifierSettings["ScriptName"].ToStr();
                _scriptArguments = notifyObject.NotifierSettings["ScriptArgs"].ToStr();
            }
            catch (Exception ex)
            {
                /*Debug Object values for reference*/
                LogBook.Debug(notifyObject, this);

                /*Write exception log*/
                LogBook.Write("Error has occurred while retrieving values from notification settings", ex, "CooperAtkins.NotificationServer.NotifyEngine.Script.ScriptClient");
            }
        }

        /// <summary>
        /// Send Script for execution
        /// </summary>
        /// <returns></returns>
        public NotifyComResponse Send()
        {
            NotifyComResponse notifyComResponse = new NotifyComResponse();
            try
            {
                /*log that script started*/
                LogBook.Write("Notification Script Started");
                Process process = new Process();
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.Arguments = _scriptArguments;
                process.StartInfo.FileName = _scriptName;
                try
                {
                    process.Start();
                    /*Record notify response*/
                    notifyComResponse.IsError = false;
                    notifyComResponse.IsSucceeded = true;
                    notifyComResponse.ResponseContent = "Script executed successfully.";
                }
                catch (Exception ex)
                {
                    /*Record notify response*/
                    notifyComResponse.IsError = true;
                    notifyComResponse.IsSucceeded = false;
                    notifyComResponse.ResponseContent = "Script execution failed";

                    /*Debug Object values for reference*/
                    LogBook.Debug(notifyComResponse, this);

                    /*Write exception log*/
                    LogBook.Write("Error has occurred while running the script.", ex, "CooperAtkins.NotificationServer.NotifyEngine.ScriptHelper");
                }
            }
            catch (Exception ex)
            {
                /*Record notify response*/
                notifyComResponse.IsError = true;
                notifyComResponse.IsSucceeded = false;
                notifyComResponse.ResponseContent = "Script execution failed";

                /*Debug Object values for reference*/
                LogBook.Debug(notifyComResponse, this);

                /*Write exception log*/
                LogBook.Write("Error has occurred while preparing script settings.", ex, "CooperAtkins.NotificationServer.NotifyEngine.Script.ScriptHelper");
            }
            return notifyComResponse;
        }
    }
}
