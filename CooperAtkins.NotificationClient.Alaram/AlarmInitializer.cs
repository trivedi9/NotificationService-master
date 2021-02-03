/*
 *  File Name : NotifyObject.cs
 *  Author : Vasu Ravuri 
 *           @ PCC Technology Group LLC
 *  Created Date : 12/01/2010
 */

namespace CooperAtkins.NotificationClient.Alarm
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using EnterpriseModel.Net;
    using CooperAtkins.Interface.Alarm;
    using CooperAtkins.NotificationClient.Alarm.DataAccess;
    using CooperAtkins.Generic;
    using CooperAtkins.NotificationClient.Generic;
    using CooperAtkins.NotificationClient.Generic.DataAccess;
    using CooperAtkins.NotificationClient.NotificationComposer;
    using CooperAtkins.Interface.NotifyCom;
    using System.Diagnostics;
    using CooperAtkins.NotificationClient.IVRNotificationComposer.DataAccess;

    

    /// <summary>
    /// Start point of the notification process
    /// </summary>
    public class AlarmInitializer
    {
        //[System.Runtime.InteropServices.DllImport("kernel32.dll")]
        //static extern IntPtr OpenThread(uint dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        //[System.Runtime.InteropServices.DllImport("kernel32.dll")]
        //static extern bool TerminateThread(IntPtr hThread, uint dwExitCode);

        // [System.Runtime.InteropServices.DllImport("kernel32.dll",EntryPoint = "GetCurrentThreadId",ExactSpelling = true)]
        //static extern Int32 GetCurrentWin32ThreadId();
        Dictionary<string, Thread> threadDictionary = new Dictionary<string, Thread>();
        
             

        private AlarmProcess _alarmProcess;
        private bool _isServiceRestarted;
        // Change on 2/19/2012
        // Srinivas Rao Eranti
        // obj is created only once ir-respective of the call made to ObjectSync function, so moved the code from ObjectSync() to global
        object obj = new object();
        
        /// <summary>
        /// Initializing the objects
        /// </summary>
        public AlarmInitializer()
        {
            LogBook.Write("Enter AlarmInitializer....");
            AlarmQueue.CurrentProcessSensorAlarmID = new List<string>();
            AlarmQueue.AlarmObjectQueue = new Queue<AlarmObject>();
            AlarmQueue.CurrentProcessObjects = new List<AlarmObject>();
            AlarmQueue.DynamicAlarmObjects = new List<AlarmObject>();
            AlarmQueue.AwaitingAcknowledment = new List<AlarmObject>();
            AlarmQueue.AwaitingBackInAccept = new List<AlarmObject>();
            AlarmQueue.WaitingToProcess = new List<AlarmObject>();

            //added by Pradeep I -- Start, on 08/27/2013 for handling cluster environment
            AlarmQueue.CurrentStateObjects = new List<AlarmObject>();
            //added by Pradeep I -- End, on 08/27/2013 for handling cluster environment

            DigitalPagerHelper.DigitalPagerQueue = new List<INotifyObject>();
            SmsHelper.SmsQueue = new List<INotifyObject>();
            List<int> dialList = new List<int>();

            LogBook.Write("new AlarmInitializer init");
            _alarmProcess = new AlarmProcess();
            _isServiceRestarted = true;
        }

        public void Close()
        {
            _alarmProcess.StopProcess();
        }

        /// <summary>
        /// Fetch Latest Sensors data
        /// Construct the objects        
        /// </summary>
        public void ConstructSensorObjects()
        {
            AlarmList alarmList = new AlarmList();

            /*Added on 08/27/2013, by Pradeep I -- Start for fetching and storing currentStatus alarm objects*/
            CurrentStatusAlarmList currentStatusAlarmList = new CurrentStatusAlarmList();
            /*Added on 08/27/2013, by Pradeep I-- End for fetchig and storing currentStatus alarm objects*/

            try
            {
                /*Added on 08/27/2013, by Pradeep I -- Start for fetching and storing currentStatus alarm objects*/
                /*When the service restarts we are updating the process status and IsElapsed values in thh database
                 to avoid notifications when the sensor comes back to normal range during failover.*/
                /*When the service is running without any interruption we need to skip the updation in the database
                 so we are sending the isServiceRestarted bool flag to avoid updation in the database */
                Criteria criteria = new Criteria();
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("isServiceRestarted", _isServiceRestarted);
                criteria.Fields = dict;

                currentStatusAlarmList.Load(criteria);
                currentStatusAlarmList.Dispose();
          //      LogBook.Write("Current Alarm List Contains ["+currentStatusAlarmList.Count+"] Alarm Entries");
                LogBook.Write("...");
                CheckCurrentStatusObjectExistInCurrentProcessObjects(currentStatusAlarmList);

                _isServiceRestarted = false;
                /*Added on 08/27/2013, by Pradeep I-- End for fetchig and storing currentStatus alarm objects*/

                /* Check for the cleared queued notifications */
                CheckClearedAlarmObjects();

                /* Load latest sensor information from database.*/
                alarmList.Load(null);
                alarmList.Dispose();

                /* Log current list information.*/
                if (alarmList.Count > 0)
                {
                    LogBook.Write(String.Format("Latest sensor Records Count: {0}", alarmList.Count.ToString()));
                }
                // This does not currently do anything from what i can tell
                LogBook.Debug(this, alarmList);
                /* Synchronizing the alarm queue and the Waiting Queue */
                if (AlarmQueue.WaitingToProcess.Count > 0)
                    ObjectSyncWaitingToProcess(alarmList);

                /*Added on 08/27/2013, by Pradeep I -- Start for cluster */
                /* Synchronizing the alarm queue and the Current State Alarm Queue */
                if (AlarmQueue.CurrentStateObjects.Count > 0)
                    ObjectSyncCurrentStateObjects(alarmList);
                /*Added on 08/27/2013, by Pradeep I -- End for cluster */

                /* Synchronizing the alarm queue when adding and removing the objects*/
                if (alarmList.Count > 0)
                    ObjectSync(alarmList);
            }
            catch (Exception ex)
            {
                LogBook.Write(ex, "CooperAtkins.NotificationClient.Alarm.AlarmInitializer", ErrorSeverity.High);
            }

        }

        private void CheckCurrentStatusObjectExistInCurrentProcessObjects(CurrentStatusAlarmList currentStatusAlarmList)
        {
         //   #if DEBUG
         //   LogBook.Write("In CheckCurrentStatusObjectExistInCurrentProcessObjects");
         //   #endif
	    /*Added on 08/27/2013, by Pradeep I -- Start, to check if the alarm object exist in current process list*/
            //for (int index = 0; index < currentStatusAlarmList.Count; index++)
            // MR 10-24-14 you cannot remove these from the list when processing them - it will cause a problem and will skip the one after the remove
            for (int index = currentStatusAlarmList.Count - 1; 0 <= index; index--)
            {
#if DEBUG
               LogBook.Write(String.Format("Current Status Alarm List HasDynamicTypes = {0} HasPreviousSwitch = {1} HasPreviousMessageBoard = {2}",
                   currentStatusAlarmList[index].HasDynamicTypes, currentStatusAlarmList[index].HasPreviousSwitch, currentStatusAlarmList[index].HasPreviousMessageBoard));
#endif
                if (AlarmQueue.CurrentProcessSensorAlarmID.Contains(currentStatusAlarmList[index].SensorAlarmID))
                {
#if DEBUG
                    LogBook.Write(String.Format("Removing SensorAlarmID = {0} from currentStatusAlarmList because it exists in CurrentProcessSensorAlarmID", currentStatusAlarmList[index].AlarmID));
#endif                   
                    currentStatusAlarmList.Remove(currentStatusAlarmList[index]);
                }
                else
                {
#if DEBUG
                    LogBook.Write(String.Format("Adding SensorAlarmID = {0} from currentStatusAlarmList to CurrentStateObjects because it does not exist in CurrentProcesSensorAlarm", currentStatusAlarmList[index].AlarmID));
#endif                    
                    AlarmQueue.CurrentStateObjects.Add(currentStatusAlarmList[index]);
                 }
            }
            /*Added on 08/27/2013, by Pradeep I -- End, to check if the alarm was cleared or acknowledged*/
        }

        /// <summary>
        /// Set server time to all active message boards.
        /// </summary>
        public void SetServerTime()
        {
            try
            {
                new CooperAtkins.NotificationClient.NotificationComposer.MessageBoardHelper().SetServerTime();
            }
            catch (Exception ex)
            {
                LogBook.Write(ex, "CooperAtkins.NotificationClient.Alarm.AlarmInitializer", ErrorSeverity.High);
            }
        }

        /// <summary>
        /// Processing all the queued alarm objects
        /// </summary>
        public void ProcessAlarmObjects()
        {
            try
            {
                /* Process all the queued objects */
                while (AlarmQueue.AlarmObjectQueue.Count > 0)
                {
                    AlarmObject alarmObject = new AlarmObject();

                    /* Remove the first object from the queue */
                    alarmObject = ObjectSync(null);

                    /* Start processing the alarm object to send the notification */
                    _alarmProcess.StartProcess(alarmObject);
                }
            }
            catch (Exception ex)
            {
                LogBook.Write(ex, "CooperAtkins.NotificationClient.Alarm.AlarmInitializer", ErrorSeverity.High);
            }
        }

        /// <summary>
        /// Remove the unused/completed alarm objects from current process queue.        
        /// </summary>
        public void AlarmGarbageCollector()
        {
            try
            {
                /* ProcessDynamicAlarmQueue before clearing the objects.*/
                ProcessDynamicAlarmQueue();
                ProcessAwaitingAcknowledmentObjects();
                ProcessBackInAcceptObjects();

                /* Process all the objects from the CurrentProcessObjects*/
                for (int index = AlarmQueue.CurrentProcessObjects.Count - 1; index >= 0; index--)
                {
                    AlarmObject alarmObject = AlarmQueue.CurrentProcessObjects[index];
                    bool removeObject = false;
                    bool resetDynamicNotification = false;



                    /* Check the global settings for the dynamic alarm types */
                    if (alarmObject.HasDynamicTypes && alarmObject.NotificationSentCount > 0 && !alarmObject.IsDynamicNotificationCleared)
                    {
                        /* if user configured for "Reset Notification Condition if User Acknowledges Alarm" and user cleared the alarm*/
                        /* or */
                        /* if user configured for "Reset Notification Condition if Sensor Returns To Normal Status:" and sensor comes to the normal range*/
                        if ((alarmObject.ResetNotifyOnUserAck && alarmObject.IsAlarmAcknowledgedOrCleared) || (alarmObject.ResetNotifyOnSensorNormalRange && !alarmObject.IsInAlarmState) || alarmObject.IsAlarmClearedByUser)
                        {
                            LogBook.Write(AlarmHelper.BasicAlarmInformation(alarmObject) + ", alarm cleared or sensor comes to normal range and alarm removing process started.");
                            resetDynamicNotification = true;
                        }
                    }

                    bool isObjectAddedInDynamicQueue = false;

                    /* Check for the process completion */
                    if ((alarmObject.IsProcessCompleted && alarmObject.NotificationSentCount > 0) || (!alarmObject.IsInAlarmState && alarmObject.IsProcessCompleted))
                    {
                        /* process completed and if it has dynamic notification then add it to dynamic alarm object queue and removing from current process queue. */
                        /* object is not marked to remove the only add it dynamic alarm queue.*/
                        if (alarmObject.HasDynamicTypes && !resetDynamicNotification)
                        {
                            isObjectAddedInDynamicQueue = true;
                            LogBook.Write(AlarmHelper.BasicAlarmInformation(alarmObject) + ", Process completed but object has dynamic alarm and adding it to dynamic alarm queue.");
                            AlarmQueue.DynamicAlarmObjects.Add(alarmObject);
                        }
                        removeObject = true;
                    }

                    /* Alarm generated and it comes to normal range*/
                    if ((alarmObject.NotificationSentCount == 0 && !alarmObject.IsInAlarmState) || (alarmObject.IsAlarmAcknowledgedOrCleared && !alarmObject.IsInAlarmState))
                    {
                        removeObject = true;
                    }

                    /*object is ready to remove but if the dynamic notification was not cleared then clear it*/
                    if (removeObject == true && alarmObject.HasDynamicTypes && !alarmObject.IsDynamicNotificationRemoved && !resetDynamicNotification)
                    {
                        /* if the current object was not is dynamic alarm queue.*/
                        if (GetQueuedDynamicAlarmObject(alarmObject.SensorAlarmID) == null)
                        {
                            LogBook.Write("Object is ready to remove but the dynamic notification was not cleared, clearing the dynamic notification.");
                            resetDynamicNotification = true;
                        }
                    }


                    if (resetDynamicNotification)
                    {
                        LogBook.Write(AlarmHelper.BasicAlarmInformation(alarmObject) + ", Clearing the dynamic type.");
                        /* Set the dynamic notifications field to clear */
                        alarmObject.IsDynamicNotificationCleared = true;
                        _alarmProcess.StartProcess(alarmObject);
                        alarmObject.IsDynamicNotificationRemoved = true;

                    }



                    /* Remove the object from the queue if marked to remove. */
                    if (removeObject)
                    {
                        LogBook.Write(AlarmHelper.BasicAlarmInformation(alarmObject) + ", Removing the object from the process.");

                        /* mark the current alarm object as completed.*/
                        if (!isObjectAddedInDynamicQueue)
                        {
                            UpdateNotificationStatus(alarmObject.NotificationID, true);
                        }

                        /* current alarm object does not have escalation and alarm not cleared but object is ready to remove from the process. */
                        if (!alarmObject.IsAlarmAcknowledgedOrCleared && !isObjectAddedInDynamicQueue)
                        {
                            AlarmQueue.AwaitingAcknowledment.Add(alarmObject);
                        }

                        if (!alarmObject.BackInAcceptLogged && !isObjectAddedInDynamicQueue)
                        {
                            AlarmQueue.AwaitingBackInAccept.Add(alarmObject);
                        }

                        AlarmQueue.CurrentProcessSensorAlarmID.Remove(alarmObject.SensorAlarmID);
                        AlarmQueue.CurrentProcessObjects.RemoveAt(index);

                        LogBook.Write("Current Process Queue Count: " + AlarmQueue.CurrentProcessObjects.Count.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                LogBook.Write(ex, "CooperAtkins.NotificationClient.Alarm.AlarmInitializer", ErrorSeverity.High);
            }
        }

        private void ProcessAwaitingAcknowledmentObjects()
        {
            /* Process all the objects from the AwaitingAcknowledment objects*/
            for (int index = AlarmQueue.AwaitingAcknowledment.Count - 1; index >= 0; index--)
            {
                AlarmObject alarmObject = AlarmQueue.AwaitingAcknowledment[index];
                CheckClearedAlarmForCurrObject(alarmObject);
                if (alarmObject.IsAlarmAcknowledgedOrCleared)
                {
                    AlarmQueue.AwaitingAcknowledment.Remove(alarmObject);
                }
            }
        }

        private void ProcessBackInAcceptObjects()
        {
            try
            {
                /* Process all the objects from the AwaitingAcknowledment objects*/
                for (int index = AlarmQueue.AwaitingBackInAccept.Count - 1; index >= 0; index--)
                {

                    AlarmObject alarmObject = AlarmQueue.AwaitingBackInAccept[index];

                    if (alarmObject.BackInAcceptLogged)
                    {
                        AlarmQueue.AwaitingBackInAccept.Remove(alarmObject);
                        continue;
                    }

                    //Thread.Sleep(13 * 1000);
                    using (CheckBackInAccept checkBackInAccept = new CheckBackInAccept())
                    {
                        checkBackInAccept.NotificationID = alarmObject.NotificationID;
                        checkBackInAccept.StoreID = alarmObject.StoreID;
                        checkBackInAccept.Execute();

                        if (checkBackInAccept.LogCount > 0)
                        {
                            AlarmQueue.AwaitingBackInAccept.Remove(alarmObject);
                            continue;
                        }


                        if (checkBackInAccept.IsBackInAcceptableRange && checkBackInAccept.LogCount == 0)
                        {

                            alarmObject.BackInAcceptLogged = true;
                            RecordNotification(NotifyTypes.NONE, alarmObject.NotificationID, NotifyStatus.STATUS, "Exit Alarm State - Back In Acceptable Range");
                            AlarmQueue.AwaitingBackInAccept.Remove(alarmObject);

                        }
                    }

                }
            }
            catch (Exception ex)
            {
                LogBook.Write(ex, "CooperAtkins.NotificationClient.Alarm.AlarmInitializer", ErrorSeverity.High);
            }
        }



        private void ProcessDynamicAlarmQueue()
        {
            /* Process all the objects from the CurrentProcessObjects*/
            for (int index = AlarmQueue.DynamicAlarmObjects.Count - 1; index >= 0; index--)
            {

                AlarmObject alarmObject = AlarmQueue.DynamicAlarmObjects[index];
#if DEBUG
                LogBook.Write(String.Format("AlarmID = {0} ResetNotifyOnUserAck = {1} AlarmAcknowledgedOrCleared = {2}\r\nResetNotifyOnSensorNormalRange = {3} IsInAlarmState = {4} IsAlarmClearedByuser = {5}\r\nIsDynamicNotificationCleared = {6} IsDynamicNotificationRemoved = {7} IsDynamicNotificationClearProcessStarted = {8}",
                    alarmObject.AlarmID, alarmObject.ResetNotifyOnUserAck, alarmObject.IsAlarmAcknowledgedOrCleared, alarmObject.ResetNotifyOnSensorNormalRange, alarmObject.IsInAlarmState, alarmObject.IsAlarmClearedByUser, alarmObject.IsDynamicNotificationCleared, alarmObject.IsDynamicNotificationRemoved, alarmObject.IsDynamicNotificationClearProcessStarted));
#endif
                if ((alarmObject.ResetNotifyOnUserAck && alarmObject.IsAlarmAcknowledgedOrCleared) || (alarmObject.ResetNotifyOnSensorNormalRange && !alarmObject.IsInAlarmState) || alarmObject.IsAlarmClearedByUser)
                {
                    LogBook.Write(AlarmHelper.BasicAlarmInformation(alarmObject) + ", alarm cleared or sensor comes to normal range and alarm removing process started.");
                    LogBook.Write(AlarmHelper.BasicAlarmInformation(alarmObject) + ", Clearing the dynamic type.");
                    /* Set the dynamic notifications field to clear */
                    alarmObject.IsDynamicNotificationCleared = true;
                    _alarmProcess.StartProcess(alarmObject);
                    alarmObject.IsDynamicNotificationRemoved = true;

                    /* remove object from the process.*/
                    UpdateNotificationStatus(alarmObject.NotificationID, true);
                    AlarmQueue.DynamicAlarmObjects.Remove(alarmObject);

                    //if (!alarmObject.BackInAcceptLogged)
                    //{
                    //    alarmObject.BackInAcceptLogged = true;
                    //    RecordNotification(NotifyTypes.NONE, alarmObject.NotificationID, NotifyStatus.STATUS, "Exit Alarm State - Back In Acceptable Range");
                    //}
                }
            }
        }

        public void ProcessDigitalPager()
        {
            //for (int index =  - 1; index >= 0; index--)
            Thread.Sleep(10000);
            for (int index = 0; index < DigitalPagerHelper.DigitalPagerQueue.Count; index++)
            {
                INotifyObject notifyObject = DigitalPagerHelper.DigitalPagerQueue[index];

                if (notifyObject == null)
                    continue;

                AlarmStatus alarmStatus = new AlarmStatus();
                try
                {
                    LogBook.Write("Checking whether alarm is cleared or not");
                    alarmStatus.NotificationID = notifyObject.NotifierSettings["NotificationID"].ToInt();
                    alarmStatus.StoreID = GenStoreInfo.GetInstance().StoreID;
                    alarmStatus.Execute();
                }
                catch (Exception ex)
                {
                    LogBook.Write(ex, "CooperAtkins.NotificationClient.Alarm.AlarmInitializer", ErrorSeverity.High);
                }
                finally
                {
                    alarmStatus.Dispose();
                }

                if (alarmStatus.IsAlarmClearedOrBackInRange)
                {
                    DigitalPagerHelper.DigitalPagerQueue.Remove(notifyObject);
                    continue;
                }

                bool isSucceeded = DigitalPagerProcessor.SendDigitalPage(notifyObject);
                if (isSucceeded)
                {
                    DigitalPagerHelper.DigitalPagerQueue.Remove(notifyObject);
                    index--;
                }

                /*wait for some time till the COM port was released.*/
                Thread.Sleep(5 * 1000);
            }
        }

        public void ProcessSms()
        {
            //for (int index =  - 1; index >= 0; index--)
            Thread.Sleep(10000);
            for (int index = 0; index < SmsHelper.SmsQueue.Count; index++)
            {
                INotifyObject notifyObject = SmsHelper.SmsQueue[index];

                if (notifyObject == null)
                    continue;

                AlarmStatus alarmStatus = new AlarmStatus();
                try
                {
                    LogBook.Write("Checking whether alarm is cleared or not");
                    alarmStatus.NotificationID = notifyObject.NotifierSettings["NotificationID"].ToInt();
                    alarmStatus.StoreID = GenStoreInfo.GetInstance().StoreID;
                    alarmStatus.Execute();
                }
                catch (Exception ex)
                {
                    LogBook.Write(ex, "CooperAtkins.NotificationClient.Alarm.AlarmInitializer", ErrorSeverity.High);
                }
                finally
                {
                    alarmStatus.Dispose();
                }

                if (alarmStatus.IsAlarmClearedOrBackInRange)
                {
                    SmsHelper.SmsQueue.Remove(notifyObject);
                    continue;
                }

                bool isSucceeded = SmsProcessor.SendSms(notifyObject);
                if (isSucceeded)
                {
                    SmsHelper.SmsQueue.Remove(notifyObject);
                    index--;
                }

                /*wait for some time till the COM port was released.*/
                Thread.Sleep(5 * 1000);
            }
        }

        public void ProcessIVRNotification()
        {
            Thread.Sleep(10000);


                string currentThreadId = String.Empty;
                //drop any previous call threads that are complete
                if (threadDictionary.Count > 0)
                {
                    try
                    {
                        //call to abort spawned threads for calling
                        Process currentProcess = Process.GetCurrentProcess();

                        TTIVRNotificationThreads ivrAlarmThreadList = new TTIVRNotificationThreads();
                        ProcessThreadCollection threadlist = currentProcess.Threads;

                        ivrAlarmThreadList.Load(null);
                        ivrAlarmThreadList.Dispose();
                        //known thread used for dialing that are complete
                        foreach (IvrAlarm threadObject in ivrAlarmThreadList)
                        {
                            string id = Convert.ToString(threadObject.ThreadID);
                            currentThreadId = id;
                            //first make sure this id is still in dictionary
                            if (threadDictionary.ContainsKey(currentThreadId) && (threadDictionary[id].ThreadState == System.Threading.ThreadState.Stopped))
                            {
                                LogBook.Write(" *** A Dialing Thread [" + currentThreadId + "] Is About To Be Released ***");
                                threadDictionary[id].Abort();
                                LogBook.Write(" *** A Dialing Thread [" + currentThreadId + "] Has Been Released ***");
                                LogBook.Write("Removing Thread ID: " + currentThreadId + " from Dictionary");
                                threadDictionary.Remove(currentThreadId);
                            }

                        }
                    }

                    catch (ThreadAbortException ex)
                    {
                        //This is an expected exception. The thread is being aborted
                        LogBook.Write(" *** A Dialing Thread [" + currentThreadId + "] Has Been Released ***");

                        LogBook.Write("Removing Thread ID: " + currentThreadId + " from Dictionary");
                        threadDictionary.Remove(currentThreadId);

                    }
                    finally
                    {

                    }
                }
            
           // LogBook.Write("ProcessIVR check");
            IvrAlarmList alarmList = new IvrAlarmList();
            try
            {
                string vcElement;
                string CDYNE_RETRIES = GetCustomVoiceSettings("CDyneRetries", out vcElement);              
                alarmList.NumAttempts = (CDYNE_RETRIES.ToInt() + 1);
                alarmList.Load(null);
                alarmList.Dispose();

   
                /* Process all the queued objects */
                foreach (IvrAlarm alarmObject in alarmList)
                {
                    bool InRangeCleared = false;
                    bool okMakeCall = true;
                    DateTime currentTime = DateTime.UtcNow;

                    LogBook.Write("Started IVR Process, processing ["+alarmList.Count.ToString()+"] objects");

                        IvrIsInProcess ivrInProcess = new IvrIsInProcess();
                        try
                        {

                               /* if (alarmObject.AlarmID > 0)
                                LogBook.Write("Checking whether alarm with id: [" + alarmObject.AlarmID + "] is in process already");
                                else
                                LogBook.Write("Checking whether notification with id: [" + alarmObject.NotificationID + "] is in process already");*/
                                //ivrInProcess.AlarmID = alarmObject.AlarmID;
                                ivrInProcess.phoneNumber = alarmObject.IVRPhoneNumber;
                                //ivrInProcess.NotificationID = alarmObject.NotificationID;
                                ivrInProcess.StoreID = GenStoreInfo.GetInstance().StoreID;
                                //ivrInProcess.IsSucceeded = false;  //find one that has failed
                                //ivrInProcess.numAttempts = 3;
                                ivrInProcess.Execute();
                         
                        }
                        catch (Exception ex)
                        {
                            LogBook.Write(ex, "CooperAtkins.NotificationClient.Alarm.AlarmInitializer", ErrorSeverity.High);
                        }
                        finally
                        {
                            ivrInProcess.Dispose();
                        }

                        #region Commented - no need to check alarm status here
                        //10-27-15if (!alarmObject.IsFailsafeEscalationNotification)
                        //10-27-15{
                        //10-27-15   IvrAlarmStatus ivrAlarmStatus = new IvrAlarmStatus();
                        //10-27-15   try
                        //10-27-15  {
                        /*  if (alarmObject.AlarmID > 0)
                          LogBook.Write("NOT FAIL SAFE: Checking whether alarm with id: [" + alarmObject.AlarmID + "] is cleared or back in range");
                          else
                          LogBook.Write("NOT FAIL SAFE: Checking whether notification with id: [" + alarmObject.NotificationID + "] is cleared or back in range");*/

                        //10-27-15 ivrAlarmStatus.NotificationID = alarmObject.NotificationID;
                        //10-27-15ivrAlarmStatus.StoreID = GenStoreInfo.GetInstance().StoreID;
                        //10-27-15ivrAlarmStatus.Execute();
                        //10-27-15}
                        //10-27-15catch (Exception ex)
                        //10-27-15{
                        //10-27-15LogBook.Write(ex, "CooperAtkins.NotificationClient.Alarm.AlarmInitializer", ErrorSeverity.High);
                        //10-27-15}
                        //10-27-15finally
                        //10-27-15{
                        //10-27-15if (ivrAlarmStatus.IsAlarmClearedOrBackInRange)
                        //10-27-15{
                        //10-27-15InRangeCleared = true;
                        //10-27-15}
                        //10-27-15ivrAlarmStatus.Dispose();
                        //10-27-15}

                        //10-27-15} 
                        #endregion


                        #region Commented - no need to check for in process here
                        //check to be sure call is not truly in-process as inprocess flag can be set when it should not be
                        // check to see if thread id is actually running
                        //if (ivrInProcess.IsAlarmInIVRProcess)
                        //{
                        //    foreach (IvrAlarm alarmObject2 in alarmList)
                        //    {
                        //        //get thread list
                        //        // compare against current alarmobjects, if none exists in thread list kill thread
                        //        Process processInfo = Process.GetCurrentProcess();
                        //        ProcessThreadCollection threadlist = processInfo.Threads;

                        //        if (threadlist.Count > 0)
                        //        {
                        //            bool isPhoneCallThreadActive = false;
                        //            for (int i = 0; i < threadlist.Count; i++)
                        //            {
                        //                if (threadlist[i].Id == ivrInProcess.threadID)
                        //                {
                        //                    //thread is still active, so it is truly in-process
                        //                    isPhoneCallThreadActive = true;
                        //                }
                        //            }
                        //            if (!isPhoneCallThreadActive && alarmObject2.ThreadID == ivrInProcess.threadID) //make sure objects match before setting vlaue
                        //            {
                        //                ivrInProcess.IsAlarmInIVRProcess = false;
                        //            }
                        //        }
                        //    }
                        //}
                        // TimeSpan intervalBetweenAttempts = currentTime.Subtract(ivrInProcess.LastAttemptTime);
                        // double Minutes = intervalBetweenAttempts.Minutes; 
                        #endregion


                        #region Commented - check already occurs before decision to call
                        //comment start 12/10/15
                        //AlarmStatus alarmStatus = new AlarmStatus();
                        //try
                        //{
                        //    LogBook.Write("IVR Checking whether ALARM[" + alarmObject.AlarmID + "] is cleared or back in range");
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
                        //}

                        //if (alarmStatus.IsAlarmClearedOrBackInRange && !alarmObject.IsFailsafeEscalationNotification)
                        //{
                        //    alarmList.Remove(alarmObject);
                        //    LogBook.Write(AlarmHelper.BasicAlarmInformation(alarmObject) + ", [ALARM: "+alarmObject.AlarmID+" CLEARED] Removing the IVR notification record from the ttIVRNotification table SensorID: " + alarmObject.UTID + ", UniqueID: " + alarmObject.SensorAlarmID + ", NotificationID: " + alarmObject.NotificationID.ToString());
                        //    using (IVRNotificationRemover IVRnotificationRemover = new IVRNotificationRemover())
                        //    {
                        //        IVRnotificationRemover.StoreID = alarmObject.StoreID;
                        //        IVRnotificationRemover.NotificationID = alarmObject.NotificationID;
                        //        IVRnotificationRemover.Execute();

                        //    }
                        //    break;
                        //comment end 12/10/15 } 
                   
                     

                       //10-27-15 if (( !InRangeCleared ) 
                        //10-27-15&& ( !ivrInProcess.IsAlarmInIVRProcess )) //InRangeCleared will be false when FailSafe mode is active, so this block will always be called during FailSafe
                        #endregion

                        if (( !ivrInProcess.IsAlarmInIVRProcess) || (ivrInProcess.numAttempts == 0))    
                    {
                            if (!alarmObject.IsFailsafeEscalationNotification)
                            {
                                LogBook.Write("IVR Alarm [" + alarmObject.AlarmID + "] AlarmType [" +alarmObject.AlarmType+ "] was not cleared, not in-range, and not in-process, dialing the number: " + alarmObject.IVRPhoneNumber.ToStr());
                            }
                            else
                            {
                                LogBook.Write("**IVR FSE: Alarm [" + alarmObject.AlarmID + "] AlarmType [" + alarmObject.AlarmType + "] FAIL SAFE ESCALATION ACTIVE, and not in-process, Dialing the number: " + alarmObject.IVRPhoneNumber.ToStr());
                            }

                            /* Start processing the alarm object to send voice notification */
                            int X = 0;
                            X = alarmObject.AlarmID;


                            var dialThread = new Thread(() => { DialNumber(alarmObject); });



                            int tId = dialThread.ManagedThreadId;
                            alarmObject.ThreadID = tId;
                            dialThread.Start();
                            int tId2 = dialThread.ManagedThreadId;
                            dialThread.Name = tId.ToString();
                            threadDictionary.Add(tId.ToString(), dialThread);

                        #region Commented - not necessary code
                        //   Thread.Sleep(15000);



                        //   foreach (IvrAlarm alarmObject2 in alarmList)
                        //  {
                        //get thread list
                        //compare against current alarmobjects, if none exists in thread list kill thread
                        //Process processInfo = Process.GetCurrentProcess();
                        //ProcessThreadCollection threadlist = processInfo.Threads;

                        //if (threadlist.Count>0)
                        //{
                        //}

                        //  } 
                        #endregion

                        }
                        #region Commented - unnecessary logging
                        //else
                        //{
                        //    if (InRangeCleared)
                        //    {

                        //        LogBook.Write("Alarm [" + alarmObject.AlarmID + "] WAS CLEARED for " + alarmObject.IVR_SensorName + " Alarm Type: "+alarmObject.AlarmType+ ". Stopping process of dialing the number: " + alarmObject.IVRPhoneNumber);
                        //    }
                        //    if (ivrInProcess.IsAlarmInIVRProcess)
                        //    {
                        //        LogBook.Write("Alarm [" + alarmObject.AlarmID + "] CALL TO ["+alarmObject.IVRPhoneNumber+"] IS IN-PROCESS for " + alarmObject.IVR_SensorName + " Alarm Type: " + alarmObject.AlarmType + ". Stopping process of dialing the number: " + alarmObject.IVRPhoneNumber);
                        //    }
                        //    else
                        //    {
                        //      //  LogBook.Write(".");
                        //    }

                        //} 
                        #endregion

                    
                    System.Threading.Thread.Sleep(2000);
                }
            }
            catch (Exception ex)
            {
                LogBook.Write(ex, "CooperAtkins.NotificationClient.Alarm.AlarmInitializer", ErrorSeverity.High);
            }
            finally
            {
                alarmList.Dispose();
            }
        }

        private string GetCustomVoiceSettings(string name, out string customElements)
        {
            string customName = "";
            customElements = null;
            foreach (VoiceCustomElement element in AlarmModuleConfiguration.Instance.Configuration.VoiceCustoms)
            {
                if (element.Name.ToLower() == name.ToLower())
                {
                    customName = element.Name;
                    if (element.Name != null)
                    {
                        customElements = element.Value;
                    }
                    break;
                }
            }
            return customElements;
        }
        private void DialNumber(IvrAlarm alarmObject)           
        {

                TimeSpan ts = DateTime.UtcNow.Subtract(alarmObject.AlarmStartTime);
                string timeOutofRange = String.Empty;

                if (ts.TotalSeconds < 60)
                {

                    timeOutofRange = ts.TotalSeconds.ToString("##.#", System.Globalization.CultureInfo.InvariantCulture) + " seconds";
                }
                else if ((ts.TotalSeconds >= 60) && (ts.TotalMinutes < 60))
                {

                    timeOutofRange = ts.TotalMinutes.ToString("##.#", System.Globalization.CultureInfo.InvariantCulture) + " minutes";
                }
                else if ((ts.TotalMinutes >= 60) && (ts.TotalHours < 24))
                {
                    timeOutofRange = ts.TotalHours.ToString("##.#", System.Globalization.CultureInfo.InvariantCulture) + " hours";
                }
                else if (ts.TotalHours >= 24)
                {
                    timeOutofRange = ts.TotalDays.ToString("##.#", System.Globalization.CultureInfo.InvariantCulture) + " days";
                }

                string promptMessage = "";

                if (alarmObject.AlarmType == 0)
                {
                    promptMessage = @"This is the " + alarmObject.StoreName + " Temp Track voice notification system  ~\\PlaySilence(0.2)~ There is an alarm with a sensor named "
                    + alarmObject.IVR_SensorName + " ~\\PlaySilence(0.2)~ The Last Reading was " + SensorValueToTTS(alarmObject.UTID, alarmObject.Probe, alarmObject.SensorType, (decimal)alarmObject.Value, alarmObject.IsCelsius)
                   + " ~\\PlaySilence(0.2)~ Sensor has been operating out of range for  ~\\PlaySilence(0.1)~ " + timeOutofRange;
                    // LogBook.Write("Voice Prompt Message: " + promptMessage);
                }
                else
                {
                    promptMessage = @"This is the " + alarmObject.StoreName + " Temp Track voice notification system  ~\\PlaySilence(0.2)~ There is a non-communication alarm with a sensor named "
                    + alarmObject.IVR_SensorName + " ~\\PlaySilence(0.2)~ The Last Reading was " + SensorValueToTTS(alarmObject.UTID, alarmObject.Probe, alarmObject.SensorType, (decimal)alarmObject.Value, alarmObject.IsCelsius)
                   + " ~\\PlaySilence(0.2)~ Sensor has not been heard from for  ~\\PlaySilence(0.1)~ " + timeOutofRange;
                }

                try
                {

                    bool isSuccessed = CooperAtkins.NotificationClient.NotificationComposer.IvrClientHelper.Send(promptMessage, new System.Collections.Hashtable()
                { 
                    {"PhoneNo", alarmObject.IVRPhoneNumber },
                     {"StorePhoneNo", alarmObject.StoreNumber },
                     {"TimeOutOfRange", (DateTime.UtcNow - alarmObject.AlarmStartTime)},
                     {"CallerName", (alarmObject.PersonName)},
                     {"Attempts",(alarmObject.AttemptCount)},
                     {"ThreadID",(alarmObject.ThreadID)},
                     {"CDYNE_ID", ("")},
                     {"CDYNE_RETRIES", ("")},
                     {"CDYNE_VOICEID", ("")},
                     {"CDYNE_VOLUME", ("")},

                }, alarmObject.NotificationID, alarmObject.IvrID, alarmObject.AlarmID, alarmObject);



                    if (isSuccessed)
                    // if (alarmObject.IVRSuccess)
                    {
                        LogBook.Write("Last Call attempt succeeded for AlarmID[" + alarmObject.AlarmID + "]: " + alarmObject.IVRPhoneNumber);
                    }
                    else
                    {
                        LogBook.Write("Last attempt did NOT SUCCEED for AlarmID[" + alarmObject.AlarmID + "]: " + alarmObject.IVRPhoneNumber);

                    }
                }
                catch (Exception ex)
                {
                    LogBook.Write("Exception when attempting to send notification to IVRClientHelper-Send for AlarmID[" + alarmObject.AlarmID + "]: " + alarmObject.IVRPhoneNumber+"  ERROR = "+ex.Message);
                }
           
        }

    private string SensorValueToTTS(string UTID, int Probe, string SensorType, decimal SensorValue, bool isCelsius)
        {
            string returnValue;
            if (((SensorType.IndexOf("TEMP") + 1)
                        + (SensorType.IndexOf("THERMO") + 1)
                        + (SensorType.IndexOf("NAFEM:31") + 1)
                        + (SensorType.IndexOf("NAFEM:9") + 1))
                        > 0)
            {
                returnValue = "Temperature " + (TempFormat(SensorValue, isCelsius) + TempUOMToTTS(isCelsius));
            }
            else if ((SensorType.IndexOf("HUMID") + 1) > 0)
            {
                returnValue = "Humidity " + (Math.Round(SensorValue, 1) + " Percent");
            }
            else
            {
                returnValue = Math.Round(SensorValue, 1).ToString();
            }

            return returnValue;
        }

        private decimal TempFormat(decimal TempFahrenheit, bool isCelsius)
        {
            decimal returnValue;

            if (!isCelsius)
            {
                returnValue = Math.Round(TempFahrenheit, 1);
            }
            else
            {
                returnValue = Math.Round((TempFahrenheit - 32) * 5 / 9, 1);
            }
            return returnValue;
        }


        private string TempUOMToTTS(bool isCelsius)
        {
            return (isCelsius ? " degrees centigrade" : " degrees Fahrenheit");
        }

        /// <summary>
        /// Resume the notification process.
        /// </summary>
        public void ResumeNotificationProcess()
        {
            using (NotificationProcessResume notificationProcessResume = new NotificationProcessResume())
            {
                notificationProcessResume.StoreID = GenStoreInfo.GetInstance().StoreID;
                notificationProcessResume.Execute();
            }
        }

        /// <summary>
        /// Updates the notification status.
        /// </summary>
        /// <param name="notificationID"></param>
        private void UpdateNotificationStatus(int notificationID, bool isCompleted)
        {
            using (NotificationStatusUpdate notificationStatusUpdate = new NotificationStatusUpdate())
            {
                notificationStatusUpdate.NotificationID = notificationID;
                notificationStatusUpdate.IsProcessCompleted = isCompleted;
                notificationStatusUpdate.Execute();
            }
        }

        /// <summary>
        /// Add the unique alarm objects to the queue.
        /// </summary>
        /// <param name="list"></param>
        private void LoadUniqueAlarmObjects(AlarmList list)
        {
            NotificationCurrentStatusCommand notificationCurrentStatusCommand = new NotificationCurrentStatusCommand();
            /* Insert alarm objects into queue.*/
            foreach (AlarmObject alarmInfo in list)
            {
                bool flag = false;

                /*New Code - 03/05/2011*/
                AlarmObject tempObj = null;
                /*New Code - 03/05/2011*/
               

                /* if the alarm is in reset mode then do not add the alarm object to the queue.*/
                if (alarmInfo.AlarmType != AlarmType.RESETCOMMUNICATIONS && alarmInfo.AlarmType >= AlarmType.RESETMODE)
                {
                    flag = true;
                }

                /* Check for objects which are being processed, if same sensor is in process don't add it into queue*/
                if (AlarmQueue.CurrentProcessSensorAlarmID.Contains(alarmInfo.SensorAlarmID))
                {
                    flag = true;
                }
                //if (!flag && tempObj != null && tempObj.IsInAlarmState) /*New Code- 03/05/2011*/
                if (!flag) /* old code - 03/05/2011*/
                {
                    /* Duplication check and maintain Unique sensor data.*/
                    AlarmObject[] objects = AlarmQueue.AlarmObjectQueue.ToArray();

                    /* Find object whether object exists in the Queue*/
                    foreach (AlarmObject obj in objects)
                    {
                        if (obj.SensorAlarmID == alarmInfo.SensorAlarmID)
                        {
                            flag = true;
                            break;
                        }
                    }

                    /*If the object did not found in alarm queue, find in the Current Process Queue*/
                    if (!flag)
                        flag = AlarmQueue.CurrentProcessSensorAlarmID.Contains(alarmInfo.SensorAlarmID);

                }
                /*  Add new object into queue */
                if (!flag)
                {
                    LogBook.Write(AlarmHelper.BasicAlarmInformation(alarmInfo) + ", Adding the alarm object into queue.");

                    /* mark the current alarm as completed in the database.*/
                    UpdateNotificationStatus(alarmInfo.NotificationID, false);


                    /* Marking the current object for alarm state*/
                    alarmInfo.IsInAlarmState = true;

                    AlarmQueue.AlarmObjectQueue.Enqueue(alarmInfo);


                    LogBook.Write("Alarm Queue Count: " + AlarmQueue.AlarmObjectQueue.Count.ToString());
                }
                else
                {

                    ///* mark the current alarm as completed in the database.*/
                    UpdateNotificationStatus(alarmInfo.NotificationID, false);

                    /* Get alarm object from the queue or current process object. Then only the below changes will be affected to reference object. */
                    AlarmObject alarmObject = GetQueuedAlarmObject(alarmInfo.SensorAlarmID);

                    /* if the alarm type is not a reset type then add the note to the exiting notification and remove the current notification.*/
                    if (alarmInfo.AlarmType == AlarmType.SENSOR || alarmInfo.AlarmType == AlarmType.COMMUNICATIONS || alarmInfo.AlarmType == AlarmType.BATTERY)
                    {
                        // this shouldn't happen too frequently but it will on occassion - need to wait for the existing one to be removed from queue before processing this one
                        // to simulate it clear an alarm and immediately click reset on transmitter when it is out of range
                        //LogBook.Write(String.Format("Old Alarm NotificationId = {0}, New Alarm NotificationId = {1}", alarmObject.NotificationID, alarmInfo.NotificationID));

                        LogBook.Write(AlarmHelper.BasicAlarmInformation(alarmInfo) + ", Adding the alarm object into Waiting to Process Queue - Previous alarm should be removed and then this will be processed.");
                        AlarmQueue.WaitingToProcess.Add(alarmInfo);
                        //if (alarmObject.IsProcessCompleted == false)
                        //{
                        //    // we should wait because we got a new alarm here, 
                        //    Thread.Sleep(5 * 1000);
                        //    alarmObject = GetQueuedAlarmObject(alarmInfo.SensorAlarmID);
                        //}

                        //LogBook.Write(String.Format("Old Alarm NotificationId = {0}, New Alarm NotificationId = {1}", alarmObject.NotificationID, alarmInfo.NotificationID));
                        //// This is a new alarm but there was an existing one - should only happen when the existing one is still being processed 
                        //// just after clearing it

                        ///*Log information about the current operation.*/
                        //LogBook.Write(AlarmHelper.BasicAlarmInformation(alarmInfo) + ", Already processing an alarm for type " + AlarmType.GetAlarmTypeString(alarmInfo.AlarmType) + " -- ignoring" + ", \r\nActive Types: " + GetActiveAlarmTypes(alarmInfo.UTID));
                        //// we should wait here for some time to see if we need to process it

                        /////*New Code - 03/05/2011*/
                        ////tempObj = GetQueuedAlarmObject(alarmInfo.SensorAlarmID);
                        /////*New Code- 03/05/2011*/
                        /////* if the object is in alarm state then only insert the below note (it may be in the range but not removed because the alarm was not cleared.) */
                        ////if (tempObj.IsInAlarmState)
                        ////{
                        ////    ///*Add note to the exiting notification.*/
                        ////    //using (AlarmNote alarmNote = new AlarmNote())
                        ////    //{
                        ////    //    alarmNote.StoreID = alarmInfo.StoreID;
                        ////    //    alarmNote.AlarmID = alarmInfo.AlarmID;
                        ////    //    alarmNote.NoteType = "ALERT";
                        ////    //    alarmNote.Message = "Notification Processing Ignored: Still Processing Previous Sensor Notification/Escalation, Type=" + AlarmType.GetAlarmTypeString(alarmInfo.AlarmType)
                        ////    //                        + "\n\r Active Types: " + GetActiveAlarmTypes(alarmInfo.UTID)
                        ////    //                        + "\n\r" + ProcessStatus(alarmInfo.SensorAlarmID);
                        ////    //    alarmNote.Execute();
                        ////    //}

                        ////    RecordNotification(NotifyTypes.NONE, alarmInfo.NotificationID, NotifyStatus.STATUS, "Still processing escalation for previous alarm (Is In Alarm State). NotificationID = " + alarmInfo.NotificationID );
                        ////}
                        ////else
                        ////{
                        ////    RecordNotification(NotifyTypes.NONE, alarmInfo.NotificationID, NotifyStatus.STATUS, "Still processing escalation for previous alarm (Is Not In Alarm State). NotificationID = " + alarmInfo.NotificationID);
                        ////}


                        //alarmObject.IsInAlarmState = true;
                        ////R
                        //AlarmQueue.AlarmObjectQueue.Enqueue(alarmInfo);

                        //LogBook.Write("Alarm Queue Count: " + AlarmQueue.AlarmObjectQueue.Count.ToString());


                    }
                    else
                    {
                        DateTime alarmTime = alarmInfo.AlarmTime;

                        int notificationID = alarmInfo.NotificationID;

                        /* if the alarm object not in the queue then assigning the new object to process further*/
                        if (alarmInfo.AlarmType >= AlarmType.RESETMODE && alarmObject == null)
                        {
                            alarmObject = alarmInfo;
                        }


                        /*if the current object is in alarm state then Ignore and remove the notification from notification table.*/
                        if (!alarmObject.IsInAlarmState)
                        {
                            LogBook.Write(AlarmHelper.BasicAlarmInformation(alarmObject) + ", Updateing the notification record from the ttNotification table SensorID: " + alarmObject.UTID + ", UniqueID: " + alarmObject.SensorAlarmID + ", NotificationID: " + alarmObject.NotificationID.ToString()+" To COMPLETE");
                            using (NotificationRemover notificationRemover = new NotificationRemover())
                            {
                                notificationRemover.StoreID = alarmObject.StoreID;
                                notificationRemover.NotificationID = notificationID;
                                //notificationRemover.Execute();
                                UpdateNotificationStatus(alarmInfo.NotificationID, true);
                            }
                        }

                        LogBook.Write(AlarmHelper.BasicAlarmInformation(alarmObject) + ", exiting from the alarm state.");

                        /* exiting from the alarm state*/
                        ExitAlarmState(alarmObject);
                        
                        /* updating the alarm exit time. */
                        alarmObject.AlarmStateExitTime = alarmTime;

                        /*Added by Pradeep I on 09/10/2013 -- Start. 
                         * When the sensor comes back to acceptble range we need to update the isElapsed status to 1*/
                        notificationCurrentStatusCommand.UTID = alarmObject.UTID;
                        notificationCurrentStatusCommand.Probe = alarmObject.Probe;
                        notificationCurrentStatusCommand.Execute(notificationCurrentStatusCommand);
                        /*Added by Pradeep I on 09/10/2013 -- End*/

                        /*Log current alarm state.*/
                        LogBook.Write(AlarmHelper.BasicAlarmInformation(alarmObject) + ", Alarm already exited alarm state for type " + AlarmType.GetAlarmTypeString(alarmObject.AlarmType) + " -- ignoring, Active Types: " + GetActiveAlarmTypes(alarmObject.UTID));
                    }
                }
            }
        }

        /// <summary>
        /// To get alarm object based on sensor alarm id.
        /// </summary>
        /// <param name="sensorAlarmID"></param>
        /// <returns></returns>
        private AlarmObject GetQueuedAlarmObject(string sensorAlarmID)
        {
            AlarmObject alarmObject = null;

            /* Find object whether object exists in the Queue*/
            AlarmObject[] objects = AlarmQueue.AlarmObjectQueue.ToArray();
            foreach (AlarmObject obj in objects)
            {
                if (obj.SensorAlarmID == sensorAlarmID)
                {
                    alarmObject = obj;
                    break;
                }
            }

            /* If we found object AlarmQueue.AlarmObjectQueue then return that */
            if (alarmObject != null)
                return alarmObject;

            /* Find object whether object exists in the Queue*/
            objects = AlarmQueue.CurrentProcessObjects.ToArray();
            foreach (AlarmObject obj in objects)
            {
                if (obj.SensorAlarmID == sensorAlarmID)
                {
                    alarmObject = obj;
                    break;
                }
            }

            /* If we found object AlarmQueue.AlarmObjectQueue then return that */
            if (alarmObject != null)
                return alarmObject;

            /* Find object whether object exists in the Queue*/
            objects = AlarmQueue.DynamicAlarmObjects.ToArray();
            foreach (AlarmObject obj in objects)
            {
                if (obj.SensorAlarmID == sensorAlarmID)
                {
                    alarmObject = obj;
                    break;
                }
            }

            return alarmObject;
        }

        /// <summary>
        /// To get queued dynamic alarm object based on sensor alarm id.
        /// </summary>
        /// <param name="sensorAlarmID"></param>
        /// <returns></returns>
        private AlarmObject GetQueuedDynamicAlarmObject(string sensorAlarmID)
        {
            AlarmObject alarmObject = null;

            /* Find object whether object exists in the Queue*/
            AlarmObject[] objects = AlarmQueue.DynamicAlarmObjects.ToArray();

            foreach (AlarmObject obj in objects)
            {
                if (obj.SensorAlarmID == sensorAlarmID)
                {
                    alarmObject = obj;
                    break;
                }
            }

            return alarmObject;
        }

        /// <summary>
        /// exiting from the alarm state for current alarm object.
        /// </summary>
        /// <param name="alarmObj"></param>
        private void ExitAlarmState(AlarmObject alarmObj)
        {
            alarmObj.IsInAlarmState = false;

            LogBook.Write(AlarmHelper.BasicAlarmInformation(alarmObj) + ",  ExitAlarmState - " + alarmObj.ProbeName + " @ " + (alarmObj.AlarmClearedTime == DateTime.MinValue ? DateTime.Now.ToString() : alarmObj.AlarmClearedTime.ToString()));
            LogBook.Debug(this, alarmObj);

            //alarmObj.BackInAcceptLogged = true; /*Commented on 10/4/2012*/
            /*Changed by Pradeep I on 10/4/2012. If the contact sensor is out of range and back into acceptable within the delay interval no need 
             to log into ttNotificationLog*/
            if (!alarmObj.BackInAcceptLogged)/*Added new 10/4/2012*/
            {
                alarmObj.BackInAcceptLogged = true;
                /*End Change 10/4/2012*/
                if (alarmObj.NotificationSentCount > 0)
                    RecordNotification(NotifyTypes.NONE, alarmObj.NotificationID, NotifyStatus.STATUS, "Exit Alarm State - Back In Acceptable Range");
            }

        }

        /// <summary>
        /// To record the notification for current alarm.
        /// </summary>
        /// <param name="notifyType"></param>
        /// <param name="notificationID"></param>
        /// <param name="status"></param>
        /// <param name="logText"></param>
        private void RecordNotification(NotifyTypes notifyType, int notificationID, NotifyStatus status, string logText)
        {
            /* Record the notification information */
            using (RecordNotification recordNotification = new RecordNotification())
            {
                recordNotification.NotifyType = notifyType;
                recordNotification.NotificationID = notificationID;
                recordNotification.Status = status;
                recordNotification.LogText = logText;
                recordNotification.Execute();
            }
        }

        /// <summary>
        /// Check the queued alarm objects are acknowledged or cleared.
        /// Check the processing alarm objects are acknowledged or cleared
        /// </summary>
        private void CheckClearedAlarmObjects()
        {
            /* Checking the notification queued objects are cleared or acknowledged */
            AlarmObject[] QueuedAlarmObjects = AlarmQueue.AlarmObjectQueue.ToArray();
            for (int index = 0; index < QueuedAlarmObjects.Length; index++)
            {
                /* Do not check if it is already cleared */
                if (!QueuedAlarmObjects[index].IsAlarmAcknowledgedOrCleared || !QueuedAlarmObjects[index].IsAlarmClearedByUser)
                    CheckClearedAlarmForCurrObject(QueuedAlarmObjects[index]);
            }

            /* Checking the current notification processing objects are cleared or acknowledged */
            for (int index = 0; index < AlarmQueue.CurrentProcessObjects.Count; index++)
            {
                /* Do not check if it is already cleared */
                if (!AlarmQueue.CurrentProcessObjects[index].IsAlarmAcknowledgedOrCleared || !AlarmQueue.CurrentProcessObjects[index].IsAlarmClearedByUser)
                    CheckClearedAlarmForCurrObject(AlarmQueue.CurrentProcessObjects[index]);
            }

            /* Checking the current notification processing objects are cleared or acknowledged */
            for (int index = 0; index < AlarmQueue.DynamicAlarmObjects.Count; index++)
            {
                /* Do not check if it is already cleared */
                if (!AlarmQueue.DynamicAlarmObjects[index].IsAlarmAcknowledgedOrCleared || !AlarmQueue.DynamicAlarmObjects[index].IsAlarmClearedByUser)
                    CheckClearedAlarmForCurrObject(AlarmQueue.DynamicAlarmObjects[index]);
            }

            /*Added  on 08/27/2013, by Pradeep I -- Start*/
            /* Checking the current status processing objects are cleared or acknowledged */
            for (int index = 0; index < AlarmQueue.CurrentStateObjects.Count; index++)
            {
                /* Do not check if it is already cleared */
                if (!AlarmQueue.CurrentStateObjects[index].IsAlarmAcknowledgedOrCleared || !AlarmQueue.CurrentStateObjects[index].IsAlarmClearedByUser)
                    CheckClearedAlarmForCurrObject(AlarmQueue.CurrentStateObjects[index]);
            }
            /*Added  on 08/27/2013, by Pradeep I -- End*/
        }

        /// <summary>        
        /// Updates the clearance information for the given alarm object.
        /// </summary>
        /// <param name="alarmObj"></param>
        private void CheckClearedAlarmForCurrObject(AlarmObject alarmObj)
        {
            if (alarmObj.AlarmID == 0)
                return;
            /* checking for current object */
            using (NotificationAcknowledgement notificationAcknowledgement = new NotificationAcknowledgement())
            {
                /* Check the database for the alarm clearance */
                notificationAcknowledgement.AlarmID = alarmObj.AlarmID;
                notificationAcknowledgement.StoreID = alarmObj.StoreID;
                notificationAcknowledgement.Execute();

                if (alarmObj.IsAlarmAcknowledgedOrCleared && !alarmObj.IsAlarmClearedByUser)
                {
                    // TODO determine if we should actually return here or go to process below
                    // This may be an issue that causes several minute delay in logging the alarm acknowledged by user msg in an escalation
                    alarmObj.IsAlarmClearedByUser = notificationAcknowledgement.IsAlarmClearedByUser;
                    return;
                }

                /* Update the current object information if the alarm was cleared */
                //if (notificationAcknowledgement.IsAlarmClearedByUser)
                if (notificationAcknowledgement.IsAlarmCleared)
                {
                    /* update the IsAlarmCleared to true */
                    alarmObj.IsAlarmAcknowledgedOrCleared = true;
                    alarmObj.IsAlarmClearedByUser = notificationAcknowledgement.IsAlarmClearedByUser;

                    if (alarmObj.IsAlarmClearedByUser)
                    {
                        /* Assign the current time to cleared time if there is no cleared time from the database.*/
                        if (notificationAcknowledgement.ClearedTime == DateTime.MinValue)
                        {
                            alarmObj.AlarmClearedTime = DateTime.UtcNow;
                        }
                        else
                        {
                            alarmObj.AlarmClearedTime = notificationAcknowledgement.ClearedTime;
                        }
                        /* Log the information */
                        LogBook.Write(AlarmHelper.BasicAlarmInformation(alarmObj) + ", Alarm Cleared By User - " + alarmObj.ProbeName + " @ " + alarmObj.AlarmClearedTime.ToShortDateString() + " " + alarmObj.AlarmClearedTime.ToShortTimeString());
                    }
                    else
                    {
                        /* Log the information */
                        LogBook.Write(AlarmHelper.BasicAlarmInformation(alarmObj) + ", Alarm Acked - " + alarmObj.ProbeName + " @ " + alarmObj.AlarmClearedTime.ToShortDateString() + " " + alarmObj.AlarmClearedTime.ToShortTimeString());
                    }

                   


                    /* Record the notification information */
                    /*Changed verbiage by Pradeep I on 10/8/2012*/
                    if (notificationAcknowledgement.IsAlarmClearedByUser)
                        RecordNotification(NotifyTypes.NONE, alarmObj.NotificationID, NotifyStatus.STATUS, "Alarm Record Cleared by User");
                    else
                        RecordNotification(NotifyTypes.NONE, alarmObj.NotificationID, NotifyStatus.STATUS, "Alarm Record Acknowledged by User");
                }
            }
        }

        /// <summary>
        /// To get all active alarm type for the current sensor.
        /// </summary>
        /// <param name="UTID"></param>
        /// <returns></returns>
        private string GetActiveAlarmTypes(string UTID)
        {
            string activeTypes = string.Empty;
            /* checking whether the sensor object in queued objects */
            AlarmObject[] QueuedAlarmObjects = AlarmQueue.AlarmObjectQueue.ToArray();

            for (int index = 0; index < QueuedAlarmObjects.Length; index++)
            {
                if (QueuedAlarmObjects[index].UTID.ToLower().Trim() == UTID.ToLower().Trim() && QueuedAlarmObjects[index].IsInAlarmState)
                {
                    activeTypes = (activeTypes == string.Empty ? "" : activeTypes + ", ") + AlarmType.GetAlarmTypeString(QueuedAlarmObjects[index].AlarmType);
                }
            }

            /* checking whether the sensor object in currently processing objects */
            for (int index = 0; index < AlarmQueue.CurrentProcessObjects.Count; index++)
            {
                if (AlarmQueue.CurrentProcessObjects[index].UTID.ToLower().Trim() == UTID.ToLower().Trim() && AlarmQueue.CurrentProcessObjects[index].IsInAlarmState)
                {
                    activeTypes = (activeTypes == string.Empty ? "" : activeTypes + ", ") + AlarmType.GetAlarmTypeString(AlarmQueue.CurrentProcessObjects[index].AlarmType);
                }
            }

            return activeTypes;
        }

        /// <summary>
        /// To get current status of the alarm object (current location in the way).
        /// </summary>
        /// <param name="alarmObject"></param>
        /// <returns></returns>
        private string ProcessStatus(string sensorAlarmID)
        {
            /* Get alarm object from the queue or current process object, it will have all the changes. */
            AlarmObject alarmInfo = GetQueuedAlarmObject(sensorAlarmID);


            string processState = "Alarm Process Status: AlarmID=" + alarmInfo.AlarmID.ToString() + @", AlarmProfileRecID=" + alarmInfo.AlarmProfileID.ToString() + @", NotifyProfileID=" + alarmInfo.NotifyProfileID.ToString() + @",
                    NotificationRecID=" + alarmInfo.NotificationID.ToString() + @", Name=" + alarmInfo.ProbeName.ToString() + @",
                    Escalation Profile Flags:
                    bStopEscalationOnExitAlarm=" + alarmInfo.StopEscalationOnExitAlarm.ToString() + @",  bStopEscalationOnUserAck=" + alarmInfo.StopEscalationOnUserAck.ToString() + @" 
                    Notification processing Flags:
                    ResetDynamicOnExitAlarm=" + alarmInfo.ResetNotifyOnSensorNormalRange.ToString() + @", ResetDynamicOnUserAck=" + alarmInfo.ResetNotifyOnUserAck.ToString() + @"
                    Alarm Status:
                    bIsInAlarmState=" + alarmInfo.IsInAlarmState.ToString() + @"
                    Still processing failsafe: " + alarmInfo.IsFailsafeEscalationNotification.ToString() + @"
                    Alarm Cleared by User: " + alarmInfo.IsAlarmAcknowledgedOrCleared.ToString() + @"
                    Next Escalation Profile Set: " + alarmInfo.IsEscalationNotification.ToString();

            return processState;
        }

        /// <summary>
        /// To synchronize the alarm queue when adding and removing the objects.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="alarmObject"></param>
        private AlarmObject ObjectSync(AlarmList list)
        {
            lock (obj)
            {
                AlarmObject alarmObject = null;

                if (list != null)
                {
                    LoadUniqueAlarmObjects(list);
                }
                else
                {
                    alarmObject = AlarmQueue.AlarmObjectQueue.Dequeue();                   
                    /* Adding the current alarm object's  SensorAlarmID which is a unique key for the process 
                    * to skip/ignore the new notifications of same type while the current notification is in progress */
                    /*possibly need to store the sensor ID in the database for current process alarm objects on 8/15/2013 by Pradeep I*/
                    AlarmQueue.CurrentProcessSensorAlarmID.Add(alarmObject.SensorAlarmID);

                    /* Add the current alarm object to CurrentProcessObjects to process further operations */
                    AlarmQueue.CurrentProcessObjects.Add(alarmObject);
                }
                return alarmObject;
            }
        }

        /// <summary>
        /// To synchronize the alarm queue with those waiting to process
        /// </summary>
        /// <param name="list"></param>        
        private void ObjectSyncCurrentStateObjects(AlarmList list)
        {
            lock (obj)
            {
                bool flag = false;
                AlarmObject[] CurrentStateAlarmObjects = AlarmQueue.CurrentStateObjects.ToArray();

                for (int index = 0; index < CurrentStateAlarmObjects.Length; index++)
                {

                    /* Check for objects which are being processed, if same sensor is in process don't add it into queue*/
                    if (AlarmQueue.CurrentProcessSensorAlarmID.Contains(CurrentStateAlarmObjects[index].SensorAlarmID))
                    {
                        flag = true;
                    }
                    if (!flag)
                    {
                        /* Duplication check and maintain Unique sensor data.*/
                        AlarmObject[] objects = AlarmQueue.AlarmObjectQueue.ToArray();

                        /* Find whether object exists in the Queue*/
                        foreach (AlarmObject alarmObj in objects)
                        {
                            if (alarmObj.SensorAlarmID == CurrentStateAlarmObjects[index].SensorAlarmID)
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag)
                    {
                        // Add it to the AlarmObjectQueue                        
                        AlarmQueue.CurrentStateObjects.Remove(CurrentStateAlarmObjects[index]);

                        /*Added by Pradeep I 09/02/2013*/
                        //if the list already contains the alarm object which is loaded through alarmList.Load() method,
                        //remove that object and place the object from the CurrentStateObjects queue
                        for (int i = list.Count -1; i >= 0; i--)
                        {
                            if (list[i].SensorAlarmID == CurrentStateAlarmObjects[index].SensorAlarmID)
                                list.RemoveAt(i);

                        }
                        list.Add(CurrentStateAlarmObjects[index]);
                        /*Added by Pradeep I 09/02/2013*/
                    }
                }
            }
        }

        /// <summary>
        /// To synchronize the alarm queue with those waiting to process
        /// </summary>
        /// <param name="list"></param>        
        private void ObjectSyncWaitingToProcess(AlarmList list)
        {
            lock (obj)
            {
                bool flag = false;
                AlarmObject[] WaitingToProcessAlarmObjects = AlarmQueue.WaitingToProcess.ToArray();

                for (int index = 0; index < WaitingToProcessAlarmObjects.Length; index++)
                {

                    /* Check for objects which are being processed, if same sensor is in process don't add it into queue*/
                    if (AlarmQueue.CurrentProcessSensorAlarmID.Contains(WaitingToProcessAlarmObjects[index].SensorAlarmID))
                    {
                        flag = true;
                    }
                    if (!flag)
                    {
                        /* Duplication check and maintain Unique sensor data.*/
                        AlarmObject[] objects = AlarmQueue.AlarmObjectQueue.ToArray();

                        /* Find object whether object exists in the Queue*/
                        foreach (AlarmObject alarmObj in objects)
                        {
                            if (alarmObj.SensorAlarmID == WaitingToProcessAlarmObjects[index].SensorAlarmID)
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag)
                    {
                        // Add it to the AlarmObjectQueue                        
                        AlarmQueue.WaitingToProcess.Remove(WaitingToProcessAlarmObjects[index]);
                        list.Add(WaitingToProcessAlarmObjects[index]);
                    }
                }
            }
        }
        AlarmObject missedCommAlarmObject = null;

        /// <summary>
        /// To check for missed communicating sensors
        /// </summary>
        public void MissedCommunicationNotification()
        {
            // Currently this throws an error, disabled this functionality for now due to this.
            return;
            //    /*Check for missed communication*/
            //    MissedCommunication missedCommunication = new MissedCommunication();
            //    string[] missedCommContent = missedCommunication.GetMissedCommunicationData();
            //    string missedCommSensorInfo = "";
            //    int missedCommSensorCount = 0;
            //    /*Missed Communication sensor information*/
            //    missedCommSensorInfo = missedCommContent[0];

            //    /*Missed Communication sensor count*/
            //    missedCommSensorCount = missedCommContent[1].ToInt();

            //    /*Send missed communication notifications*/
            //    if (missedCommSensorCount > 0)
            //    {
            //        /*Clearing the existing missed communication notifications*/
            //        if (missedCommAlarmObject != null)
            //        {
            //            missedCommAlarmObject.IsDynamicNotificationCleared = true;
            //            missedCommAlarmObject.IsDynamicNotificationRemoved = false;
            //            NotificationComposer.NotificationClient.GetInstance().Send(missedCommAlarmObject);
            //        }

            //        /*Get missed communication general settings*/
            //        MissedCommInfo missedCommInfo = new MissedCommInfo();
            //        missedCommInfo = missedCommInfo.Execute();

            //        missedCommAlarmObject = new AlarmObject()
            //        {
            //            IsMissCommNotification = true,
            //            NotifyProfileID = missedCommInfo.NotificationProfileID,
            //            PagerMessage = missedCommInfo.PagerPrompt,
            //            SwitchBitmask = missedCommInfo.SwitchBitmask,
            //            MissedCommSensorCount = missedCommSensorCount,
            //            MissedCommSensorInfo = missedCommSensorInfo,
            //            IsDynamicNotificationCleared = false,
            //            IsDynamicNotificationRemoved = false,
            //            SensorAlarmID = "999999" //Send unique SensorAlarmID to Message Board to set/reset the missed comm. notification.
            //        };

            //        /*Get Notification Profile information*/
            //        NotificationComposer.NotificationClient.GetInstance().Send(missedCommAlarmObject);
            //    }
            //    else
            //    {
            //        missedCommAlarmObject.IsDynamicNotificationCleared = true;
            //        missedCommAlarmObject.IsDynamicNotificationRemoved = false;
            //        NotificationComposer.NotificationClient.GetInstance().Send(missedCommAlarmObject);
            //    }
        }
    }
}
