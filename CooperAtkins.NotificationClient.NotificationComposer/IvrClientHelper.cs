/*
 *  File Name : IvrClientHelper.cs
 *  Author : Vasu Ravuri
 *  @ PCC Technology Group LLC
 *  Created Date : 01/03/2011
 */

namespace CooperAtkins.NotificationClient.NotificationComposer
{

    using System;
    using System.Collections;
    using System.IO;
   // using CooperAtkins.NotificationClient.NotificationComposer.DataAccess;
    using CooperAtkins.Generic;
    using CooperAtkins.Interface.NotifyCom;
    using CooperAtkins.Interface.Alarm;
    using CooperAtkins.NotificationClient.Generic.DataAccess;
    using CooperAtkins.NotificationClient.IVRNotificationComposer.DataAccess;
    using CooperAtkins.NotificationClient.NotificationComposer.com.cdyne.ws;
    


    public static class IvrClientHelper
    {
        /// <summary>
        /// Returns true if the current operation succeeded.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="settings"></param>
        /// <param name="notificationID"></param>
        /// <returns></returns>
        /// 
       
 

        public static bool Send(string data, Hashtable settings, int notificationID, int ivrID, int alarmID,AlarmObject alarmObject)
        {
            bool retValue = false;
            bool callConnected = false;
            string response = string.Empty;
            string succeeded = string.Empty;
            string error = string.Empty;
            Int16 dailAttempts = 1;
            string DigitsPressed = string.Empty;
            
            long QueueID = -1;
            
            int TryCount = -1;
            bool CallAnswered = false;
            bool Answered = false;
            bool CallComplete = false;
            string MachineDetection = string.Empty;
            int Duration = -1;
            string IVRServiceResponseText = string.Empty;
            int TTIVRNotifications_RecID = -1;
            

            NotifyObject notifyObject = new NotifyObject();
            NotifyComResponse notifyResponse = new NotifyComResponse();
            NotifyComResponse notifyComResponse = new NotifyComResponse();

            notifyObject.NotificationType = "Voice";
            notifyObject.NotificationData = data;  //voice message to be played
            notifyObject.NotifierSettings = settings; //hash table of keys and values
            

            NotificationEndPointElement element;
            string vcElement1;
            string vcElement2;
            string vcElement3;
            string vcElement4;           
           
            /* get end point for voice composer*/    

                     
            string method =  NotificationClient.GetInstance().WhoAmI("Voice", out element);

            string arguments = null;
            LogBook.Write("Sending voice alert....");
            switch (method.ToLower())
            {
                case "cooperatkins.notificationclient.notifyengine.ivrnotificationcom":


                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    if (element == null)
                        arguments = @"-m ""<notification ack='true'><notificationData><![CDATA[" + data + @"]]></notificationData><notificationType><![CDATA[Voice]]></notificationType><notificationSettings><PhoneNo><![CDATA[" + settings["PhoneNo"].ToStr() + @"]]></PhoneNo></notificationSettings></notification>"" ";
                    else
                        arguments = @"-c " + element.EndpointAddress + @" -m ""<notification ack='true'><notificationData><![CDATA[" + data + @"]]></notificationData><notificationType><![CDATA[Voice]]></notificationType><notificationSettings><PhoneNo><![CDATA[" + settings["PhoneNo"].ToStr() + @"]]></PhoneNo></notificationSettings></notification>"" ";

                    while (!callConnected)
                    {
                        IvrAlarmStatus ivrAlarmStatus = new IvrAlarmStatus();
                        try
                        {
                            LogBook.Write("Checking whether alarm is cleared or not");
                            ivrAlarmStatus.AlarmID = alarmID;
                            ivrAlarmStatus.NotificationID = notificationID;
                            ivrAlarmStatus.StoreID = GenStoreInfo.GetInstance().StoreID;
                            ivrAlarmStatus.Execute();
                        }
                        catch (Exception ex)
                        {
                            LogBook.Write(ex, "CooperAtkins.NotificationClient.NotificationComposer.IvrClientHelper", ErrorSeverity.High);
                        }
                        finally
                        {
                            ivrAlarmStatus.Dispose();
                        }

                        if (ivrAlarmStatus.IsAlarmClearedOrBackInRange)
                        {
                            break;
                        }

                        /*       analog modem use  */

                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardOutput = true;
                        //process.StartInfo.RedirectStandardError = true;
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.FileName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\Utility.exe";
                        //process.StartInfo.FileName = @"F:\DEV\CooperAtkins\CooperAtkins.NotificationEngine.Utility\bin\Debug\Utility.exe";
                        process.StartInfo.Arguments = arguments;
                        // process.StartInfo.WorkingDirectory = ;
                        process.Start();

                        process.WaitForExit();


                        response = process.StandardOutput.ReadToEnd();

                        callConnected = true;
                        if (response.ToLower().Contains("device initialization failed") || response.ToLower().Contains("invalid_device_id") || response.ToLower().Contains("dll_not_found") || response.ToLower().Contains("exception from hresult"))
                        {
                            callConnected = false;
                        }
                        /*     end  analog modem use  */
                    }
                        break;


                case "cooperatkins.notificationclient.notificationcomposer.cdyneivrnotificationcomposer":
                      long TransactionID = -1;
                      DateTime StartTime;
                      DateTime EndTime;
                      long AlarmID = -1;
                      
                    
                    string CDYNE_ID = GenStoreInfo.GetInstance().CDYNE_ACCOUNT;//NotificationClient.GetInstance().GetCustomVoiceSettings("CDyneID", out vcElement1);
                        string CDYNE_RETRIES = NotificationClient.GetInstance().GetCustomVoiceSettings("CDyneRetries", out vcElement2);
                        string CDYNE_VOICEID = NotificationClient.GetInstance().GetCustomVoiceSettings("CDyneVoiceID", out vcElement3);
                        string CDYNE_VOLUME = NotificationClient.GetInstance().GetCustomVoiceSettings("CDyneVolume", out vcElement4);
                        
                        notifyObject.NotifierSettings["CDYNE_ID"] = CDYNE_ID;
                        notifyObject.NotifierSettings["CDYNE_RETRIES"] = CDYNE_RETRIES;                        
                        notifyObject.NotifierSettings["CDYNE_VOICEID"] = CDYNE_VOICEID;
                        notifyObject.NotifierSettings["CDYNE_VOLUME"] = CDYNE_VOLUME;
                        NotificationStyle notificationStyle = new NotificationStyle();
                        TTIvrNotifications tTIvrNotificationsObject = new TTIvrNotifications();
                        callConnected = false;

                        while (!callConnected)
                        {
                            notifyComResponse.IsInProcess = true;
                            int QueryCall = 0;
                            int UnivError = 0;

                            //if (!alarmObject.IsFailsafeEscalationNotification)
                            //{
                                //IvrAlarmStatus ivrAlarmStatus = new IvrAlarmStatus();
                                //try
                                //{
                                //    LogBook.Write("Checking whether alarm is cleared or not");
                                //    ivrAlarmStatus.AlarmID = alarmID;
                                //    ivrAlarmStatus.NotificationID = notificationID;
                                //    ivrAlarmStatus.StoreID = GenStoreInfo.GetInstance().StoreID;
                                //    ivrAlarmStatus.Execute();
                                //}
                                //catch (Exception ex)
                                //{
                                //    LogBook.Write(ex, "CooperAtkins.NotificationClient.NotificationComposer.CDYNEIvrClientHelper", ErrorSeverity.High);
                                //}
                                //finally
                                //{
                                //    ivrAlarmStatus.Dispose();
                                //}

                            //12/9/15 comment start
                            //AlarmStatus alarmStatus = new AlarmStatus();
                            //try
                            //{
                            //    LogBook.Write("IVR Checking whether alarm [" + alarmObject.NotificationID + "] is cleared or not");
                            //    alarmStatus.NotificationID = alarmObject.NotificationID;
                            //    alarmStatus.StoreID = GenStoreInfo.GetInstance().StoreID;
                            //    alarmStatus.Execute();
                            //}
                            //catch (Exception ex)
                            //{
                            //    LogBook.Write(ex, "CooperAtkins.NotificationClient.Alarm.AlarmInitializer", ErrorSeverity.High);
                            //}
                            //finally
                            //{
                            //    alarmStatus.Dispose();
                            //12/9/15 comment end } 

                            int threadID = notifyObject.NotifierSettings["ThreadID"].ToInt();

                            //12/9/15 comment start
                            //if (alarmStatus.IsAlarmClearedOrBackInRange)
                            //{
                            //    Answered = false;
                            //    callConnected = false;
                            //    notifyComResponse.IsSucceeded = false;
                            //    notifyComResponse.ResponseContent += "\r\n\r\n\r\n\r\n < Phone Call To " + notifyObject.NotifierSettings["PhoneNo"] + "[" + notifyObject.NotifierSettings["CallerName"] + "]" + " SUSPENDED (Attempt # " + dailAttempts + " of " + (CDYNE_RETRIES.ToInt() + 1) + ")";
                            //    notifyComResponse.ResponseContent += "\r\n\r\n\r\n\r\nALARM WAS CLEARED OR BACK IN RANGE";                                

                            //    LogBook.Write("CALL SUSPENDED To " + notifyObject.NotifierSettings["PhoneNo"] + IVRServiceResponseText + " for NotificationID: " + notificationID + " [CDyneTransID: " + TransactionID + "] ALARM WAS CLEARED OR SENSOR IS BACK IN RANGE");
                                
                            //    notificationStyle.RecordNotification(notifyComResponse.ResponseContent.ToString(), notificationID, TransactionID.ToInt(), NotifyStatus.FAIL, NotifyTypes.VOICE);
                            //    UpdateIVRNotification(ivrID, false, false, dailAttempts, TransactionID, threadID);
                                
                            //    break;
                              
                            //12/9/15 comment end } 

                            
                            int totalCallAttempts = notifyObject.NotifierSettings["Attempts"].ToInt();

                            if ((totalCallAttempts+dailAttempts) > (CDYNE_RETRIES.ToInt() + 1))
                            //also if a notficicationId is in an alert progress state from another thread, 
                            //fall into this code to break out OR if cleared or inprogress check to be sure 
                            //we made contatc through our maximum attempts value
                            {
                                                          
                                notifyComResponse.IsInProcess = false;
                                UpdateIVRNotification(ivrID, false, notifyComResponse.IsInProcess, dailAttempts, TransactionID, threadID);                                 
                                break;
                            }
                            //}
                                                        

                            UpdateIVRNotification(ivrID, false, true,(totalCallAttempts+dailAttempts), TransactionID,threadID);
                            
                            try{
                            
                                IVRProcessor maketheIVRCall = new IVRProcessor();
                            notifyResponse =  maketheIVRCall.Invoke(notifyObject);
                          //  notifyComResponse = new NotifyComResponse();
                            TransactionID = notifyResponse.TransactionIDReturned;
                            //query for call result

                               Answered = false;
                               PhoneNotify pn2 = new PhoneNotify();
                               while (!Answered)
                               {
                                   

                                   if (QueryCall == 0) //on first call, wait 1 minute for call to complete before getting call results
                                   {
                                       System.Threading.Thread.Sleep(1 * 60 * 1000);
                                   }
                                   else
                                   {
                                       System.Threading.Thread.Sleep(5 * 1000);
                                   }
                                   NotifyReturn nr2 = pn2.GetQueueIDStatus(TransactionID);
                                   MachineDetection = nr2.MachineDetection;
                                   DigitsPressed = nr2.DigitsPressed;
                                   CallAnswered = nr2.CallAnswered;
                                   CallComplete = nr2.CallComplete;
                                   IVRServiceResponseText = nr2.ResponseText;
                                   Duration = nr2.Duration;
                                   StartTime = nr2.StartTime;
                                   EndTime = nr2.EndTime;
                                   if (CallAnswered == true && DigitsPressed.Contains("*"))
                                   {
                                       Answered = true;
                                       callConnected = true;
                                       notifyComResponse.IsSucceeded = true;
                                       //notifyComResponse.ResponseContent += "\r\n\r\n\r\n\r\n < Phone Call To " + notifyObject.NotifierSettings["PhoneNo"] + "[" + notifyObject.NotifierSettings["CallerName"] + "]" + " Connected (Attempt # " + dailAttempts + " of " + (CDYNE_RETRIES.ToInt() + 1) + ")";
                                       notifyComResponse.ResponseContent += "\r\n\r\n\r\n\r\n < Phone Call To " + notifyObject.NotifierSettings["PhoneNo"] + "[" + notifyObject.NotifierSettings["CallerName"] + "]" + " Connected.  ";
                                       notifyComResponse.ResponseContent += "\r\n\r\n\r\n\r\nAcknowledgement received, " + "'" + DigitsPressed + "' pressed.";
                                       notifyComResponse.ResponseContent += "\r\n\r\n\r\n\r\nCall Complete >";

                                       LogBook.Write("Query Call State Try: " + (QueryCall + 1) + ": Phone Call " + (totalCallAttempts + dailAttempts) + " of " + (CDYNE_RETRIES.ToInt() + 1) + " To " + notifyObject.NotifierSettings["PhoneNo"] + " Answered and Digits: " + DigitsPressed + " were pressed: " + IVRServiceResponseText + " for NotificationID: " + notificationID + " [CDyneTransID: " + TransactionID + "]");
                                       break;
                                   }
                                   else if (CallAnswered == true && !DigitsPressed.Contains("*") && MachineDetection=="HUMAN" && CallComplete)
                                   {
                                       Answered = true;
                                       notifyComResponse.IsSucceeded = false;
                                       //notifyComResponse.ResponseContent += "\r\n\r\n\r\n\r\n < Phone Call To " + notifyObject.NotifierSettings["PhoneNo"] + "[" + notifyObject.NotifierSettings["CallerName"] + "]" + " Connected (Attempt # " + dailAttempts + " of " + (CDYNE_RETRIES.ToInt() + 1) + ")";
                                       notifyComResponse.ResponseContent += "\r\n\r\n\r\n\r\n < Phone Call To " + notifyObject.NotifierSettings["PhoneNo"] + "[" + notifyObject.NotifierSettings["CallerName"] + "]" + " Connected.  ";
                                       notifyComResponse.ResponseContent += "\r\n\r\n\r\n\r\nNo Acknowledgement received, Digits Pressed: "+DigitsPressed+">";
                                       LogBook.Write("Query Call State Try: " + (QueryCall + 1) + ": Phone Call " + (totalCallAttempts + dailAttempts) + " of " + (CDYNE_RETRIES.ToInt() + 1) + " To " + notifyObject.NotifierSettings["PhoneNo"] + " Answered but no digits were pressed: " + IVRServiceResponseText + " for NotificationID: " + notificationID + " [CDyneTransID: " + TransactionID + "]");
                                       break;
                                   }
                                   else if (CallAnswered == true && DigitsPressed == "" && MachineDetection == "HUMAN" && !CallComplete)
                                   {
                                       notifyComResponse.IsSucceeded = false;
                                       //notifyComResponse.ResponseContent += "\r\n\r\n\r\n\r\n < Phone Call To " + notifyObject.NotifierSettings["PhoneNo"] + "[" + notifyObject.NotifierSettings["CallerName"] + "]" + " Connected (Attempt # " + dailAttempts + " of " + (CDYNE_RETRIES.ToInt() + 1) + ")";
                                       notifyComResponse.ResponseContent += "\r\n\r\n\r\n\r\n < Phone Call To " + notifyObject.NotifierSettings["PhoneNo"] + "[" + notifyObject.NotifierSettings["CallerName"] + "]" + " Connected.  ";
                                       notifyComResponse.ResponseContent += "\r\n\r\n\r\n\r\nNo Acknowledgement received >";
                                       LogBook.Write("Query Call State Try: " + (QueryCall + 1) + ": Phone Call " + (totalCallAttempts + dailAttempts) + " of " + (CDYNE_RETRIES.ToInt() + 1) + " To " + notifyObject.NotifierSettings["PhoneNo"] + " Answered but no digits were pressed: " + IVRServiceResponseText + " for NotificationID: " + notificationID + " [CDyneTransID: " + TransactionID + "]");
                                       QueryCall += 1;
                                       
                                   }
                                   else if (CallAnswered == true && DigitsPressed == "" && MachineDetection == "MACHINE")
                                   {
                                       Answered = false;
                                       notifyComResponse.IsSucceeded = false;
                                       //notifyComResponse.ResponseContent += "\r\n\r\n\r\n\r\n < Phone Call To " + notifyObject.NotifierSettings["PhoneNo"] +"["+ notifyObject.NotifierSettings["CallerName"]+"]"+ " Connected (Attempt # " + dailAttempts + " of " + (CDYNE_RETRIES.ToInt() + 1) + ")";
                                       notifyComResponse.ResponseContent += "\r\n\r\n\r\n\r\n < Phone Call To " + notifyObject.NotifierSettings["PhoneNo"] + "[" + notifyObject.NotifierSettings["CallerName"] + "]" + " Connected.  ";
                                       notifyComResponse.ResponseContent += "\r\n\r\n\r\n\r\nAnswering Machine Detected >";
                                       LogBook.Write("Query Call State Try: " + (QueryCall + 1) + ": Phone Call " + (totalCallAttempts + dailAttempts) + " of " + (CDYNE_RETRIES.ToInt() + 1) + " To " + notifyObject.NotifierSettings["PhoneNo"] + " Answered by MACHINE: " + IVRServiceResponseText + " for NotificationID: " + notificationID + " [CDyneTransID: " + TransactionID + "]");
                                       break;
                                   }
                                   else if (IVRServiceResponseText == "In Call/Ringing" || IVRServiceResponseText == "Queued" || IVRServiceResponseText == "Universal Error")
                                   {
                                       notifyComResponse.IsSucceeded = false;
                                       notifyComResponse.IsError = true;
                                       //if (notifyComResponse.ResponseContent != null)
                                       //{
                                       //    if ((!notifyComResponse.ResponseContent.ToString().Contains("Call/Ringing (Attempt # " + dailAttempts)) &&
                                       //        (!notifyComResponse.ResponseContent.ToString().Contains("Queued (Attempt # " + dailAttempts)) &&
                                       //        (!notifyComResponse.ResponseContent.ToString().Contains("Universal Error  (Attempt # " + dailAttempts)))  //only add line if it has not already been logged for this dial attempt
                                       //    {
                                       //        notifyComResponse.ResponseContent += "\r\n\r\n\r\n\r\n < Phone Call To " + notifyObject.NotifierSettings["PhoneNo"] + "[" + notifyObject.NotifierSettings["CallerName"] + "] " + IVRServiceResponseText + " (Attempt # " + dailAttempts + " of " + (CDYNE_RETRIES.ToInt() + 1) + ") >";
                                       //    }
                                       //}
                                       //else
                                       //{
                                       //    notifyComResponse.ResponseContent += "\r\n\r\n\r\n\r\n < Phone Call To " + notifyObject.NotifierSettings["PhoneNo"] + "[" + notifyObject.NotifierSettings["CallerName"] + "] " + IVRServiceResponseText + " (Attempt # " + dailAttempts + " of " + (CDYNE_RETRIES.ToInt() + 1) + ") >";
                                       //}
                                   //    notifyComResponse.ResponseContent = "";
                                       LogBook.Write("Query Call State Try: " + (QueryCall + 1) + ": Phone Call " + (totalCallAttempts + dailAttempts) + " of " + (CDYNE_RETRIES.ToInt() + 1) + " To " + notifyObject.NotifierSettings["PhoneNo"] + " Current Status: " + IVRServiceResponseText + " for NotificationID: " + notificationID + " [CDyneTransID: " + TransactionID + "]");
                                       if (IVRServiceResponseText == "Universal Error")
                                       {
                                           UnivError += 1; //if it continues to return Universl Error allow a way to get out
                                       }
                                       QueryCall += 1;
                                    
                                   }
                                   else
                                   {
                                       notifyComResponse.IsSucceeded = false;
                                       if (notifyComResponse.ResponseContent != null)
                                       {
                                           if (!notifyComResponse.ResponseContent.ToString().Contains("ERROR"))  //only add line if it has not already been logged
                                           {
                                               //notifyComResponse.ResponseContent += "\r\n\r\n\r\n\r\n < Phone Call To " + notifyObject.NotifierSettings["PhoneNo"] + "[" + notifyObject.NotifierSettings["CallerName"] + "]" + " ERROR: " + IVRServiceResponseText + " (Attempt # " + dailAttempts + " of " + (CDYNE_RETRIES.ToInt() + 1) + ") >";
                                               notifyComResponse.ResponseContent += "\r\n\r\n\r\n\r\n < Phone Call To " + notifyObject.NotifierSettings["PhoneNo"] + "[" + notifyObject.NotifierSettings["CallerName"] + "]" + " ERROR: " + IVRServiceResponseText+" >";
                                           }
                                       }
                                       else
                                       {
                                           //notifyComResponse.ResponseContent += "\r\n\r\n\r\n\r\n < Phone Call To " + notifyObject.NotifierSettings["PhoneNo"] + "[" + notifyObject.NotifierSettings["CallerName"] + "]" + " ERROR: " + IVRServiceResponseText + " (Attempt # " + dailAttempts + " of " + (CDYNE_RETRIES.ToInt() + 1) + ") >";
                                           notifyComResponse.ResponseContent += "\r\n\r\n\r\n\r\n < Phone Call To " + notifyObject.NotifierSettings["PhoneNo"] + "[" + notifyObject.NotifierSettings["CallerName"] + "]" + " ERROR: " + IVRServiceResponseText + " >";
                                       }
                                       LogBook.Write("Query Call State Try: " + (QueryCall + 1) + ": Phone Call " + (totalCallAttempts + dailAttempts) + " of " + (CDYNE_RETRIES.ToInt() + 1) + " To " + notifyObject.NotifierSettings["PhoneNo"] + " Current Status: " + IVRServiceResponseText + " for NotificationID: " + notificationID + " [CDyneTransID: " + TransactionID + "]");
                                       QueryCall += 1;
                                   }
                                   if (QueryCall > 2 || Answered || (UnivError > 10))
                                   {
                                      
                                       break;
                                   }
                               }

                              }catch(Exception ex)
                              {
                                  LogBook.Write(ex, "CooperAtkins.NotificationClient.NotificationComposer.CDYNEIvrClientHelper MAkeTheCall", ErrorSeverity.High);
                              }
                            

                            if (!callConnected || !Answered)
                            {
                                //write to TTNotificationLog

                                if (notifyComResponse.ResponseContent != null)
                                {
                                    if (notifyComResponse.ResponseContent.ToString() != "")
                                    {
                                        notificationStyle.RecordNotification(notifyComResponse.ResponseContent.ToString(), notificationID, TransactionID.ToInt(), NotifyStatus.FAIL, NotifyTypes.VOICE);
                                    }
                                }
                                    notifyComResponse.ResponseContent = "";
                                
                                //write to TTIVRNotifications
                                UpdateIVRNotification(ivrID, notifyComResponse.IsSucceeded, notifyComResponse.IsInProcess, (totalCallAttempts + dailAttempts), TransactionID,threadID);
                               // System.Threading.Thread.Sleep(30 * 1000); //wait and try that number 1 more time before trying next person
                                dailAttempts += 1;
                                retValue = false;
                                if (((totalCallAttempts + dailAttempts.ToInt())) > CDYNE_RETRIES.ToInt16() + 1)
                                {//try each person X times from config                                    
                                    LogBook.Write("Exhausted  " + (CDYNE_RETRIES.ToInt()) + " Re-Try Attempts To: " + notifyObject.NotifierSettings["PhoneNo"] + " for NotificationID: " + notificationID + " [CDyneTransID: " + TransactionID + "]");
                                    notifyComResponse.IsInProcess = false;

                                    if (UnivError >= 10) //reset call counter, so it will re-attempt to call person has CDyne never truly made a call
                                    {                    //4th param in below function
                                        UpdateIVRNotification(ivrID, notifyComResponse.IsSucceeded, notifyComResponse.IsInProcess, 0, TransactionID, threadID);
                                    }
                                    else
                                    {
                                        UpdateIVRNotification(ivrID, notifyComResponse.IsSucceeded, notifyComResponse.IsInProcess, (totalCallAttempts + dailAttempts), TransactionID, threadID);
                                    }
                                    retValue = false;
                                    alarmObject.IVRSuccess = false;
                                    break;
                                }
                                else
                                {
                                    LogBook.Write("Trying Retry Call Attempt " + (totalCallAttempts + dailAttempts) + " of " + (CDYNE_RETRIES.ToInt() + 1) + " Attempts To: " + notifyObject.NotifierSettings["PhoneNo"] + " for NotificationID: " + notificationID);
                                }
                            }
                            else
                            {
                                LogBook.Write("Call Complete.  Alert Success during call " + (totalCallAttempts + dailAttempts) + " of " + (CDYNE_RETRIES.ToInt() + 1) + " attempts To: " + notifyObject.NotifierSettings["PhoneNo"] + " for NotificationID: " + notificationID + " [CDyneTransID: " + TransactionID + "]");
                                notifyComResponse.IsInProcess = false;
                                notificationStyle.RecordNotification(notifyComResponse.ResponseContent.ToString(), notificationID, TransactionID.ToInt(), NotifyStatus.PASS, NotifyTypes.VOICE);
                                notifyComResponse.ResponseContent = "";
                                UpdateIVRNotification(ivrID, notifyComResponse.IsSucceeded, notifyComResponse.IsInProcess, (totalCallAttempts + dailAttempts), TransactionID,threadID);
                                retValue = true;
                                alarmObject.IVRSuccess = true;
                            }
                        }
                        break; //end call attempts

                default:
                        break;
                     
            }

           if (method.ToLower() == "cooperatkins.notificationserver.notifyengine.ivrnotificationcom")
                    {
                       NotificationStyle notificationStyle = new NotificationStyle();

                        foreach (string responseLine in response.Split(new string[] { "\n" }, StringSplitOptions.None))
                        {
                            if (responseLine.ToLower().Contains("succeeded"))
                            {
                                succeeded = responseLine.ToLower().Replace("succeeded :", string.Empty);
                            }
                            else if (responseLine.ToLower().Contains("error"))
                            {
                                error = responseLine.ToLower().Replace("error :", string.Empty);
                            }
                        }


                        LogBook.Write("ResponseContent:" + response + ", IsSucceeded:" + succeeded + ", IsError:" + error);


                        /* record the notification information to the database. */
                        if (succeeded == "no")
                        {
                            notificationStyle.RecordNotification(response.Remove(0, 60).Replace("\n", "\r\n"), notificationID, 0, NotifyStatus.FAIL, NotifyTypes.VOICE);

                        }
                        else
                        {
                            notificationStyle.RecordNotification(response.Remove(0, 60).Replace("\n", "\r\n"), notificationID, 0, NotifyStatus.PASS, NotifyTypes.VOICE);
                        }


                        retValue = succeeded == "yes";
                    }
                    
                    return retValue;
            }
       

        /// <summary>
        /// To Record IVRNotification.
        /// </summary>
        private static void UpdateIVRNotification(int ivrID,bool answered,bool inProcess,int numAttempts, long transactionId,int threadID)
        {
            short callSucceeded = 0;
            short ivrInProcess = 0;
            long transID = transactionId;

            if (answered == false)
            {
                callSucceeded = 0;
            }
            else
            {
                callSucceeded = 1;
            }

            if (inProcess == false)
            {
                ivrInProcess = 0;
            }
            else
            {
                ivrInProcess = 1;
            }
            try
            {
                /* assigning the values to object.*/
                TTIvrNotifications tTIVRNotifications = new TTIvrNotifications()
                {
                    Action = "U",
                    LastAttemptTime = DateTime.UtcNow.ToDateTime(),
                    RecID = ivrID,
                    NumAttempts = numAttempts,
                    isSuccess = callSucceeded,
                    isInProcess = ivrInProcess,
                     TransID = transID.ToInt(),
                     ThreadID = threadID
            
                          };

                tTIVRNotifications.Execute();
            }
            catch (Exception ex)
            {
                LogBook.Write(ex, "CooperAtkins.NotificationClient.NotificationComposer.IvrClientHelper");
            }
        }

      

       
        
    }

}
