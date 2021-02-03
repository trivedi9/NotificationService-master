/*
 *  File Name : EscalationProcess.cs
 *  Author : Vasu Ravuri 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/25/2010
 *  Description: To do the escalation process.
 */

namespace CooperAtkins.NotificationClient.EscalationModule
{

    using System;
    using System.Collections.Generic;
    using CooperAtkins.Interface.Alarm;
    using CooperAtkins.NotificationClient.EscalationModule.DataAccess;
    using CooperAtkins.NotificationClient.Generic;
    using System.Threading;
    using CooperAtkins.Generic;
    using CooperAtkins.NotificationClient.Generic.DataAccess;
    using EnterpriseModel.Net;
    /*Changed by Pradeep on 09/30/2012*/
    using System.Linq;

    public class EscalationProcess
    {

        /// <summary>
        /// to continue the escalation process.
        /// </summary>
        /// <param name="alarmObject"></param>
        internal void DoEscalationProcess(object state)
        {
            /// <summary>
            /// To check whether the alarm was cleared in previous escalation.
            /// </summary>
            bool _isAlarmCleared = false;
            bool _hasReturnedToNormal = false;
            AlarmObject alarmCurrentStatus = new AlarmObject();
            AlarmCurrentStatusContext alarmCurrentStatusContext = new AlarmCurrentStatusContext();

            EscalationState escState = (EscalationState)state;
            /* if we lost the object */
            if (escState.AlarmObject == null)
            {
                LogBook.Write("ESCALATION stopped, alarm was cleared or acknowledged.");
            }

            try
            {
                /* check whether the sensor type is contact */
                if (AlarmHelper.IsContactSensor(escState.AlarmObject.SensorType) && escState.AlarmObject.IsInAlarmState)
                {
                    LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", sensor detected and waiting to complete delay time: " + escState.AlarmObject.CondThresholdMins + " Minutes");

                    /*Added on 08/25/2013 by pradeep I-- Begin. For cluster. Recording start of dely process to database*/
                    //this condition is true when the service restarts and alarm object holds the previous notification start time.
                    if (escState.AlarmObject.NotificationStartTime != DateTime.MinValue && (escState.AlarmObject.NotificationType != null && escState.AlarmObject.NotificationType == "DLY"))
                    {
                        DateTime currTime = DateTime.UtcNow;
                        double mints = Math.Round(currTime.Subtract(escState.AlarmObject.NotificationStartTime).TotalMinutes,0);
                        if ((escState.AlarmObject.CondThresholdMins - (int)mints) < 0)
                            escState.AlarmObject.CondThresholdMins = 0;
                        else
                            escState.AlarmObject.CondThresholdMins = escState.AlarmObject.CondThresholdMins - (int)mints;

                        alarmCurrentStatus = escState.AlarmObject;
                        /* Service resumes and the condition holds we need to reset the below two values, as we need to continue the*/
                        /* process of storing the escalation record in the database*/
                        escState.AlarmObject.NotificationStartTime = DateTime.MinValue;
                        escState.AlarmObject.NotificationType = null;
                    }
                    else
                    {
                        /*previous general notification is sent and now the service restarted, after the general notification is sent and escalation is set
                         no need to record, we can directly fo to escalation process */
                        if (escState.AlarmObject.NotificationSentCount > 0 && (escState.AlarmObject.NotificationType != null && escState.AlarmObject.NotificationType != "DLY"))
                            goto EscalationStart;
                        //Set values to alarmcurrent status object
                        alarmCurrentStatus = new AlarmObject();
                        SetAlarmCurrentStatusObject(alarmCurrentStatus, escState.AlarmObject, "DLY", 0);
                        /* Save the current status of alarm object to database*/
                        alarmCurrentStatusContext.Save(alarmCurrentStatus);
                        /*Added on 08/25/2013 by pradeep I-- End */
                    }
                                      

                    /* wait till the delay time completes */
                    int waitSecs = 0;

                    while (true)
                    {
                        if (waitSecs >= escState.AlarmObject.CondThresholdMins * 60 * 10)
                        {
                            LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", delay completed.");
                            break;
                        }
                        if (!escState.AlarmObject.IsInAlarmState)
                        {
                            // bRemove
                            _hasReturnedToNormal = true;
                            LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", Sensor not in alarm state anymore");

                        }
                        using (NotificationAcknowledgement notificationAcknowledgement = new NotificationAcknowledgement())
                        {

                            notificationAcknowledgement.AlarmID = escState.AlarmObject.AlarmID;
                            notificationAcknowledgement.StoreID = escState.AlarmObject.StoreID;
                            notificationAcknowledgement.Execute();

                            if (notificationAcknowledgement.IsAlarmCleared)
                            {
                                //escState.AlarmObject.IsAlarmAcknowledgedOrCleared = true;
                                _hasReturnedToNormal = true;
                            }
                        }

                        if (_hasReturnedToNormal)
                        {
                            break;
                        }
                        //Thread.Sleep(5 * 1000);
                        //waitSecs += 5;
                        Thread.Sleep(1 * 100);
                        waitSecs += 1;
                    }

                    if (!escState.AlarmObject.IsInAlarmState || _hasReturnedToNormal)
                    {
                        PuckAlarm puckAlarm = new PuckAlarm();
                        puckAlarm.AlarmID = escState.AlarmObject.AlarmID;

                        /* Remove existing alarm since delay was not hit */
                        using (PuckAlarmContext context = new PuckAlarmContext())
                        {
                            Criteria criteria = new Criteria();
                            criteria.ID = escState.AlarmObject.AlarmID;
                            LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", delay completed or short-circuited - Not in Alarm State any longer.");
                            //context.Delete(criteria);
                            //Save(puckAlarm, EnterpriseModel.Net.ObjectAction.Edit);
                        }

                        LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ",  exitting delay notification, sensor comes to normal range.");

                        /* object no longer in alarm state.*/
                        escState.AlarmObject.IsInAlarmState = false;
                        /* Stopping the further process*/
                        AlarmHelper.MarkAsCompleted(escState.AlarmObject, " not in esc alarm state OR has returned to normal ");

                        if (alarmCurrentStatus.UTID != null && alarmCurrentStatus.Probe != 0)
                        {
                            alarmCurrentStatusContext.Save(alarmCurrentStatus, EnterpriseModel.Net.ObjectAction.Edit);
                            alarmCurrentStatus = null;
                        }

                        return;


                    }

                    LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", delay completed and checking whether the sensor comes to normal range or not.");

                    /*Changed by Pradeep on 09/30/2012*/
                    /*For contact sensor if the user opens and closes the transmitter withing the delay minutes interval
                       notification has to be stopped*/
                    if (!escState.AlarmObject.IsInAlarmState)
                    {
                        LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", sensor comes to normal range.");
                        /* Added by Pradeep I on 08/25/2013 -- Start. Update isElapsed to true for current alarm object*/
                        if (alarmCurrentStatus.UTID != null && alarmCurrentStatus.Probe != 0)
                        {
                            alarmCurrentStatusContext.Save(alarmCurrentStatus, EnterpriseModel.Net.ObjectAction.Edit);
                            alarmCurrentStatus = null;
                        }
                        /* Added by Pradeep I on 08/25/2013 -- End.*/
                        return;
                    }

                    /* Check whether the alarm was cleared or not */
                    CheckClearedAlarm(escState, null, true);

                    if (escState.AlarmObject.IsAlarmAcknowledgedOrCleared && !escState.AlarmObject.IsInAlarmState)
                    {
                        LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", stopping delay notification, notification cleared or sensor comes to normal range.");
                        /* Added by Pradeep I on 08/25/2013 -- Start. Update isElapsed to true for current alarm object*/
                        if (alarmCurrentStatus.UTID != null && alarmCurrentStatus.Probe != 0)
                        {
                            alarmCurrentStatusContext.Save(alarmCurrentStatus, EnterpriseModel.Net.ObjectAction.Edit);
                            alarmCurrentStatus = null;
                        }
                        /* Added by Pradeep I on 08/25/2013 -- End.*/
                        return;
                    }

                    /* if the alarm was cleared or sensor comes to normal range */
                    if (escState.AlarmObject.IsAlarmAcknowledgedOrCleared || !escState.AlarmObject.IsInAlarmState)
                    {
                        LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", stopping delay notification, notification cleared or sensor comes to normal range.");
                        if (escState.EscalationList.Count == 0)
                        {
                            AlarmHelper.MarkAsCompleted(escState.AlarmObject, " no normal escalation condition met ");
                            escState.AlarmObject.NotificationSentCount = 1;

                            /* Added by Pradeep I on 08/25/2013 -- Start. Update isElapsed to true for current alarm object*/
                            if (alarmCurrentStatus.UTID != null && alarmCurrentStatus.Probe != 0)
                            {
                                alarmCurrentStatusContext.Save(alarmCurrentStatus, EnterpriseModel.Net.ObjectAction.Edit);
                                alarmCurrentStatus = null;
                            }
                            /* Added by Pradeep I on 08/25/2013 -- End.*/

                            return;
                        }
                        else
                        {
                            /*wait for failsafe notification*/
                            _isAlarmCleared = true;
                        }
                    }

                    DelayProcess delayProcess = new DelayProcess();

                    /* check whether the  reset type received before the delay time completed.*/
                    delayProcess.RecordAlarm(escState.AlarmObject);

                    /* if the alarm was cleared then do not send the notification. Stop the process.*/
                    if (escState.AlarmObject.IsAlarmAcknowledgedOrCleared)
                    {
                        LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ",  stopping delay notification, notification cleared or sensor comes to normal range.");

                        /* object no longer in alarm state.*/
                        escState.AlarmObject.IsInAlarmState = false;
                        /* Stopping the further process*/
                        AlarmHelper.MarkAsCompleted(escState.AlarmObject, " cleared or acknowledged ");

                        /* Added by Pradeep I on 08/25/2013 -- Start. Update isElapsed to true for current alarm object*/
                        if (alarmCurrentStatus.UTID != null && alarmCurrentStatus.Probe != 0)
                        {
                            alarmCurrentStatusContext.Save(alarmCurrentStatus, EnterpriseModel.Net.ObjectAction.Edit);
                            alarmCurrentStatus = null;
                        }
                        /* Added by Pradeep I on 08/25/2013 -- End.*/

                        return;
                    }

                    LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", sensor not in the range and sending the notification.");
                    /* sending notification after the delay*/

                    try
                    {
                        escState.NotificationClient.Send(escState.AlarmObject);
                    }
                    catch (Exception ex)
                    {
                        LogBook.Write(ex, "CooperAtkins.NotificationClient.EscalationModule.EscalationProcess", ErrorSeverity.High);
                    }
                }


                /* wait till the notification sent out */
                while (true)
                {
                    if (escState.AlarmObject.NotificationSentCount > 0)
                        break;

                    Thread.Sleep(1000);
                }

                /* Added by Pradeep I on 08/25/2013 -- Start. Update isElapsed to true for current alarm object*/
                if (alarmCurrentStatus.UTID != null && alarmCurrentStatus.Probe != 0)
                {
                    alarmCurrentStatusContext.Save(alarmCurrentStatus, EnterpriseModel.Net.ObjectAction.Edit);
                    alarmCurrentStatus = null;
                }
                /* Added by Pradeep I on 08/25/2013 -- End.*/

                EscalationStart:

                LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", Notification sent, checking for escalation profiles.");
                
                /* Start escalation process */
                IEnumerable<EscalationInfo> list = escState.EscalationList;
                LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", Escalation profile(s) found, processing the escalation profile(s).");


                /*Changed by Pradeep on 09/30/2012*/
                bool isProfileHasFailSafe = list.Where(info => info.IsFailSafe == true).Count() > 0;

                /* process all the escalations */
                bool skippedElapsedEscalations = false;
                foreach (EscalationInfo escInfoObj in list)
                {

                    LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", started new escalation process, escalation level: " + escInfoObj.EscalationLevel + ", escalation id: " + escInfoObj.EscalationID + ", escalation profile name: " + escInfoObj.ProfileName + ", escalation failsafe? " + escInfoObj.IsFailSafe.ToBoolean());

                    /* wait till the configured escalation time */
                    int waitSecs = 0;
                    /*Added on 09/02/2013 by pradeep I-- Begin. For cluster. */
                    if (escState.AlarmObject.NotificationStartTime != DateTime.MinValue && (escState.AlarmObject.NotificationType != null && escState.AlarmObject.NotificationType == "ESC"))
                    {
                        //while the service resumes, we need to skip already processed escalation profiles
                        if (escState.AlarmObject.EscRecID != escInfoObj.EscalationID && !skippedElapsedEscalations)
                            continue;
                        else
                            skippedElapsedEscalations = true;

                        DateTime currTime = DateTime.UtcNow;
                        double secs = Math.Round(currTime.Subtract(escState.AlarmObject.NotificationStartTime).TotalSeconds, 0);
                        if (secs > waitSecs)
                            waitSecs = (int)secs;

                        alarmCurrentStatus = escState.AlarmObject;

                        /* Service resumes and the condition holds we need to reset the below two values, as we need to continue the*/
                        /* process of storing the escalation record in the database*/
                        escState.AlarmObject.NotificationStartTime = DateTime.MinValue;
                        escState.AlarmObject.NotificationType = null;
                    }/*Added on 09/02/2013 by pradeep I-- End  */
                    else
                    {
                        /*Added by Pradeep I on 09/16/2013. During failsafe we need to skip escalation records*/
                        //while the service resumes, we need to skip already processed escalation profiles
                        if (escState.AlarmObject.NotificationStartTime != DateTime.MinValue && (escState.AlarmObject.NotificationType != null && escState.AlarmObject.NotificationType == "FSE"))
                        {
                            if (escState.AlarmObject.EscRecID != escInfoObj.EscalationID && !skippedElapsedEscalations)
                                continue;
                            else
                                skippedElapsedEscalations = true;
                        }
                        /*Added on 08/25/2013 by pradeep I-- Begin. For cluster. Recording start of escalation process to database*/
                        //Set values to alarmcurrent status object
                        if (!escInfoObj.IsFailSafe)
                        {
                            alarmCurrentStatus = new AlarmObject();
                            SetAlarmCurrentStatusObject(alarmCurrentStatus, escState.AlarmObject, "ESC", escInfoObj.EscalationID);
                            /* Save the current status of alarm object to database*/
                            alarmCurrentStatusContext.Save(alarmCurrentStatus);
                        }
                        else
                        {
                            /*added on 09/16/2013 by pradeep I, while resume the service after failesafe, we need not to insert a new record as we laready have one in the DB*/
                            if (escState.AlarmObject.NotificationStartTime == DateTime.MinValue && escState.AlarmObject.NotificationType == null)
                            {
                                alarmCurrentStatus = new AlarmObject();
                                SetAlarmCurrentStatusObject(alarmCurrentStatus, escState.AlarmObject, "FSE", escInfoObj.EscalationID);
                                /* Save the current status of alarm object to database*/
                                alarmCurrentStatusContext.Save(alarmCurrentStatus);
                                escState.AlarmObject.SaveCurrentState = false;
                            }
                            else/*Added by Pradeep I, to handle failsafe during resume.*/
                            {
                                DateTime currTime = DateTime.UtcNow;
                                double secs = Math.Round(currTime.Subtract(escState.AlarmObject.NotificationStartTime).TotalSeconds, 0);
                                if (secs > waitSecs)
                                    waitSecs = (int)secs;
                                alarmCurrentStatus = escState.AlarmObject;
                            }
                        }
                        /*Added on 08/25/2013 by pradeep I-- End */
                    }

                    while (true)
                    {
                        if (waitSecs >= escInfoObj.WaitSecs)
                        {
                            break;
                        }
                        Thread.Sleep(5 * 1000);
                        waitSecs += 5;

                        /*Changed by Pradeep on 09/30/2012 - start*/
                        /*Reason: profile has been set to stop escalation if user acknowledges and user has cleared the alarm and fail safe is NOT set.
                         we need to remove the object from the processing queue and process any new alarms from the same sensor as fresh alarm*/
                        //else if (escInfoObj.StopEscOnUserAck && escState.AlarmObject.IsAlarmAcknowledgedOrCleared )//&& !isProfileHasFailSafe)/*Commented on 10/5/2012 by Pradeep I. AS we need to stop all escalations and fail safe escalations as well if the user clears and alarm. Any further alarm sholuld be processed as new*/
                        if (escState.AlarmObject.IsAlarmClearedByUser)
                        {
                            AlarmHelper.MarkAsCompleted(escState.AlarmObject, " esc state cleared by user ");
                            if (!escState.AlarmObject.StopPendingEsc)
                            {
                                RecordNotification(NotifyTypes.NONE, escState.AlarmObject.NotificationID, NotifyStatus.STATUS, "Stopping all pending Escalations");
                                escState.AlarmObject.StopPendingEsc = true;

                                /* Added by Pradeep I on 08/25/2013 -- Start. Update isElapsed to true for current alarm object*/
                                if (alarmCurrentStatus.UTID != null && alarmCurrentStatus.Probe != 0)
                                {
                                    alarmCurrentStatusContext.Save(alarmCurrentStatus, EnterpriseModel.Net.ObjectAction.Edit);
                                    alarmCurrentStatus = null;
                                }
                                /* Added by Pradeep I on 08/25/2013 -- End.*/

                            }
                            return;
                        }

                        else if (escInfoObj.IsFailSafe && !escState.AlarmObject.IsInAlarmState)
                        {
                            LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", " + escInfoObj.EscalationLevel + ", sensor comes to normal range and exiting from failsafe escalation process.");
                            AlarmHelper.MarkAsCompleted(escState.AlarmObject, " exiting from FSE - back to normal ");
                            if (!escState.AlarmObject.StopPendingEsc)
                            {
                                RecordNotification(NotifyTypes.NONE, escState.AlarmObject.NotificationID, NotifyStatus.STATUS, "Stopping all pending Escalations");
                                escState.AlarmObject.StopPendingEsc = true;

                                /* Added by Pradeep I on 08/25/2013 -- Start. Update isElapsed to true for current alarm object*/
                                if (alarmCurrentStatus.UTID != null && alarmCurrentStatus.Probe != 0)
                                {
                                    alarmCurrentStatusContext.Save(alarmCurrentStatus, EnterpriseModel.Net.ObjectAction.Edit);
                                    alarmCurrentStatus = null;
                                }
                                /* Added by Pradeep I on 08/25/2013 -- End.*/

                            }
                            return;
                        }
                        /*Alarm ack'd or cleared and sensor comes to normal range - this condition would stop an escalation and failsafe*/
                        else if (escState.AlarmObject.IsAlarmAcknowledgedOrCleared && !escState.AlarmObject.IsInAlarmState)
                        {
                            AlarmHelper.MarkAsCompleted(escState.AlarmObject, " esc stopped acked or cleared AND canme into normal range ");
                            LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", escalation level:" + escInfoObj.EscalationLevel + ", Alarm acknowledged or cleared and sensor came to normal range and exiting the process.");
                            if (!escState.AlarmObject.StopPendingEsc)
                            {
                                RecordNotification(NotifyTypes.NONE, escState.AlarmObject.NotificationID, NotifyStatus.STATUS, "Stopping all pending Escalations");
                                escState.AlarmObject.StopPendingEsc = true;

                                /* Added by Pradeep I on 08/25/2013 -- Start. Update isElapsed to true for current alarm object*/
                                if (alarmCurrentStatus.UTID != null && alarmCurrentStatus.Probe != 0)
                                {
                                    alarmCurrentStatusContext.Save(alarmCurrentStatus, EnterpriseModel.Net.ObjectAction.Edit);
                                    alarmCurrentStatus = null;
                                }
                                /* Added by Pradeep I on 08/25/2013 -- End.*/

                            }
                            return;
                        }

                        /*Changed by Pradeep on 09/30/2012 - end*/

                        /* if the sensor comes to the normal range, stop the complete process */
                        if (!escState.AlarmObject.IsInAlarmState && escInfoObj.StopEscOnSesnorNormalState)
                        {
                            LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", " + escInfoObj.EscalationLevel + ", sensor comes to normal range and exiting from escalation process.");
                            AlarmHelper.MarkAsCompleted(escState.AlarmObject, " esc is now normal and esc profile is set to stop esc on normal event ");
                            if (!escState.AlarmObject.StopPendingEsc)
                            {
                                RecordNotification(NotifyTypes.NONE, escState.AlarmObject.NotificationID, NotifyStatus.STATUS, "Stopping all pending Escalations");
                                escState.AlarmObject.StopPendingEsc = true;

                                /* Added by Pradeep I on 08/25/2013 -- Start. Update isElapsed to true for current alarm object*/
                                if (alarmCurrentStatus.UTID != null && alarmCurrentStatus.Probe != 0)
                                {
                                    alarmCurrentStatusContext.Save(alarmCurrentStatus, EnterpriseModel.Net.ObjectAction.Edit);
                                    alarmCurrentStatus = null;
                                }
                                /* Added by Pradeep I on 08/25/2013 -- End.*/

                            }
                            return;
                        }

                    }


                    /* if the alarm was cleared and still the sensor in out of range then wait for failsafe escalation, but do not send notification for normal escalations*/
                    if (_isAlarmCleared && escInfoObj.StopEscOnUserAck && !escInfoObj.IsFailSafe)
                    {
                        LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", escalation level:" + escInfoObj.EscalationLevel + ", time completed and alarm was cleared by user, waiting for failsafe escalation.");
                        continue;
                    }

                    LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", escalation level:" + escInfoObj.EscalationLevel + ", checking whether the alarm was cleared or not.");

                    /* Alarm was not cleared*/
                    if (!_isAlarmCleared)
                    {
                        /* Check whether the alarm was cleared or not */
                        CheckClearedAlarm(escState, escInfoObj, false);
                    }


                    if (escState.AlarmObject.IsAlarmAcknowledgedOrCleared && !escState.AlarmObject.IsInAlarmState)
                    {
                        AlarmHelper.MarkAsCompleted(escState.AlarmObject, " exiting esc - cleared or acked and came into normal range ");
                        LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", escalation level:" + escInfoObj.EscalationLevel + ", Alarm cleared and sensor came to normal range and exiting the process.");

                        if (!escState.AlarmObject.StopPendingEsc)
                        {
                            RecordNotification(NotifyTypes.NONE, escState.AlarmObject.NotificationID, NotifyStatus.STATUS, "Stopping all pending Escalations");
                            escState.AlarmObject.StopPendingEsc = true;

                            /* Added by Pradeep I on 08/25/2013 -- Start. Update isElapsed to true for current alarm object*/
                            if (alarmCurrentStatus.UTID != null && alarmCurrentStatus.Probe != 0)
                            {
                                alarmCurrentStatusContext.Save(alarmCurrentStatus, EnterpriseModel.Net.ObjectAction.Edit);
                                alarmCurrentStatus = null;
                            }
                            /* Added by Pradeep I on 08/25/2013 -- End.*/

                        }
                        return;
                    }


                    /* if the current escalation is failsafe escalation*/
                    if (escInfoObj.IsFailSafe)
                    {
                        // initial failsafe is set to WaitSecs so it triggers
                        waitSecs = escInfoObj.WaitSecs;
                        /* continue the failsafe escalation till the sensor come to the normal range, 
                        * failsafe will continue irrespective to the alarm clear / acknowledgement */

                        while (true)
                        {
                            //this condition is true when the service restarts and alarm object holds the previous notification start time.
                            if (escState.AlarmObject.NotificationStartTime != DateTime.MinValue && (escState.AlarmObject.NotificationType != null && escState.AlarmObject.NotificationType == "FSE"))
                            {
                                DateTime currTime = DateTime.UtcNow;
                                double secs = Math.Round(currTime.Subtract(escState.AlarmObject.NotificationStartTime).TotalSeconds, 0);
                                if (secs > waitSecs)
                                    waitSecs = (int)secs;

                                /* Service resumes and the condition holds we need to reset the below two values, as we need to continue the*/
                                /* process of storing the escalation record in the database*/
                                escState.AlarmObject.NotificationStartTime = DateTime.MinValue;
                                escState.AlarmObject.NotificationType = null;
                            }
                            else
                            {
                                if (escState.AlarmObject.SaveCurrentState == true)
                                {
                                    /*Added on 08/25/2013 by pradeep I-- Begin. For cluster. Recording start of escalation process to database*/
                                    //Set values to alarmcurrent status object
                                    alarmCurrentStatus = new AlarmObject();
                                    SetAlarmCurrentStatusObject(alarmCurrentStatus, escState.AlarmObject, "FSE", escInfoObj.EscalationID);
                                    /* Save the current status of alarm object to database*/
                                    alarmCurrentStatusContext.Save(alarmCurrentStatus);
                                    escState.AlarmObject.SaveCurrentState = false;
                                    /*Added on 08/25/2013 by pradeep I-- End */
                                }
                            }

                            if (waitSecs >= escInfoObj.WaitSecs)
                            {
                                // reset waitSecs
                                waitSecs = 0;
                                /*Changed by Pradeep I on 10/10/2012. Added new code to skip failsafe and all escalations if user clears an alarm. */
                                if (escState.AlarmObject.IsAlarmClearedByUser)
                                {
                                    AlarmHelper.MarkAsCompleted(escState.AlarmObject, " exiting FSE - alarm cleared by user ");
                                    LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", processing failsafe escalation, Alarm cleared - exiting the process.");
                                    if (!escState.AlarmObject.StopPendingEsc)
                                    {
                                        RecordNotification(NotifyTypes.NONE, escState.AlarmObject.NotificationID, NotifyStatus.STATUS, "Stopping all pending Escalations");
                                        escState.AlarmObject.StopPendingEsc = true;

                                        /* Added by Pradeep I on 08/25/2013 -- Start. Update isElapsed to true for current alarm object*/
                                        if (alarmCurrentStatus.UTID != null && alarmCurrentStatus.Probe != 0)
                                        {
                                            alarmCurrentStatusContext.Save(alarmCurrentStatus, EnterpriseModel.Net.ObjectAction.Edit);
                                            alarmCurrentStatus = null;
                                            escState.AlarmObject.SaveCurrentState = true;
                                        }
                                        /* Added by Pradeep I on 08/25/2013 -- End.*/

                                    }
                                    return;
                                }
                                LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", processing failsafe escalation, checking for sensor status.");

                                /*if sensor comes to normal range, stop the failsafe escalation*/
                                //if (IsSensorInRange(escState.AlarmObject) || !escState.AlarmObject.IsInAlarmState)
                                if (!escState.AlarmObject.IsInAlarmState)
                                {
                                    LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", processing failsafe escalation, sensor is in range and stopping the notification process for current alarm.");
                                    AlarmHelper.MarkAsCompleted(escState.AlarmObject, " exiting FSE - in normal range ");
                                    if (!escState.AlarmObject.StopPendingEsc)
                                    {
                                        RecordNotification(NotifyTypes.NONE, escState.AlarmObject.NotificationID, NotifyStatus.STATUS, "Stopping all pending Escalations");
                                        escState.AlarmObject.StopPendingEsc = true;

                                        /* Added by Pradeep I on 08/25/2013 -- Start. Update isElapsed to true for current alarm object*/
                                        if (alarmCurrentStatus.UTID != null && alarmCurrentStatus.Probe != 0)
                                        {
                                            alarmCurrentStatusContext.Save(alarmCurrentStatus, EnterpriseModel.Net.ObjectAction.Edit);
                                            alarmCurrentStatus = null;
                                            escState.AlarmObject.SaveCurrentState = true;
                                        }
                                        /* Added by Pradeep I on 08/25/2013 -- End.*/

                                    }
                                    // return or break here?
                                    return;
                                }
                                else
                                {
                                    RecordNotification(NotifyTypes.NONE, escState.AlarmObject.NotificationID, NotifyStatus.STATUS, "ESCALATING FAILSAFE NOTIFICATION");
                                    LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", processing failsafe escalation, sending notification.");
                                    DoEscalation(escState, escInfoObj);

                                    /* Added by Pradeep I on 08/25/2013 -- Start. Update isElapsed to true for current alarm object*/
                                    if (alarmCurrentStatus.UTID != null && alarmCurrentStatus.Probe != 0)
                                    {
                                        alarmCurrentStatusContext.Save(alarmCurrentStatus, EnterpriseModel.Net.ObjectAction.Edit);
                                        alarmCurrentStatus = null;
                                        escState.AlarmObject.SaveCurrentState = true;
                                    }
                                    /* Added by Pradeep I on 08/25/2013 -- End.*/
                                }

                            }
                            // sleep for 5 seconds
                            Thread.Sleep(5 * 1000);
                            waitSecs += 5;


                            //Thread.Sleep(escInfoObj.WaitSecs * 1000);
                            if (escState.AlarmObject.IsAlarmClearedByUser)
                            {
                                AlarmHelper.MarkAsCompleted(escState.AlarmObject, " exiting FSE - cleared by user ");
                                LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", processing failsafe escalation, Alarm cleared - exiting the process.");
                                if (!escState.AlarmObject.StopPendingEsc)
                                {
                                    RecordNotification(NotifyTypes.NONE, escState.AlarmObject.NotificationID, NotifyStatus.STATUS, "Stopping all pending Escalations");
                                    escState.AlarmObject.StopPendingEsc = true;

                                    /* Added by Pradeep I on 08/25/2013 -- Start. Update isElapsed to true for current alarm object*/
                                    if (alarmCurrentStatus.UTID != null && alarmCurrentStatus.Probe != 0)
                                    {
                                        alarmCurrentStatusContext.Save(alarmCurrentStatus, EnterpriseModel.Net.ObjectAction.Edit);
                                        alarmCurrentStatus = null;
                                        escState.AlarmObject.SaveCurrentState = true;
                                    }
                                    /* Added by Pradeep I on 08/25/2013 -- End.*/

                                }
                                return;
                            }
                            if (!escState.AlarmObject.IsInAlarmState)
                            {
                                LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", processing failsafe escalation, sensor is in range and stopping the notification process for current alarm.");
                                AlarmHelper.MarkAsCompleted(escState.AlarmObject, " exiting FSE - sensor in normal range ");
                                if (!escState.AlarmObject.StopPendingEsc)
                                {
                                    RecordNotification(NotifyTypes.NONE, escState.AlarmObject.NotificationID, NotifyStatus.STATUS, "Stopping all pending Escalations");
                                    escState.AlarmObject.StopPendingEsc = true;

                                    /* Added by Pradeep I on 08/25/2013 -- Start. Update isElapsed to true for current alarm object*/
                                    if (alarmCurrentStatus.UTID != null && alarmCurrentStatus.Probe != 0)
                                    {
                                        alarmCurrentStatusContext.Save(alarmCurrentStatus, EnterpriseModel.Net.ObjectAction.Edit);
                                        alarmCurrentStatus = null;
                                        escState.AlarmObject.SaveCurrentState = true;
                                    }
                                    /* Added by Pradeep I on 08/25/2013 -- End.*/

                                }
                                // return or break here?
                                return;
                            }


                        }

                        /* failsafe completed, coming out from the process*/
                        //return;
                    }


                    /* if the alarm was cleared and user configured for StopEscOnUserAck*/
                    if (escState.AlarmObject.IsAlarmAcknowledgedOrCleared && escInfoObj.StopEscOnUserAck && !escInfoObj.IsFailSafe)
                    {
                        LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", " + escInfoObj.EscalationLevel + " escalation, current alarm was cleared, skipping the regular escalation process.");
                        _isAlarmCleared = true;

                        /* Added by Pradeep I on 08/25/2013 -- Start. Update isElapsed to true for current alarm object*/
                        if (alarmCurrentStatus.UTID != null && alarmCurrentStatus.Probe != 0)
                        {
                            alarmCurrentStatusContext.Save(alarmCurrentStatus, EnterpriseModel.Net.ObjectAction.Edit);
                            alarmCurrentStatus = null;
                        }
                        /* Added by Pradeep I on 08/25/2013 -- End.*/
                    }
                    else
                    {
                        /* sending notification....*/
                        RecordNotification(NotifyTypes.NONE, escState.AlarmObject.NotificationID, NotifyStatus.STATUS, "ESCALATION NOTIFICATION");
                        LogBook.Write(AlarmHelper.BasicAlarmInformation(escState.AlarmObject) + ", " + escInfoObj.EscalationLevel + " escalation, sending notification.");
                        DoEscalation(escState, escInfoObj);

                        /* Added by Pradeep I on 08/25/2013 -- Start. Update isElapsed to true for current alarm object*/
                        if (alarmCurrentStatus.UTID != null && alarmCurrentStatus.Probe != 0)
                        {
                            alarmCurrentStatusContext.Save(alarmCurrentStatus, EnterpriseModel.Net.ObjectAction.Edit);
                            alarmCurrentStatus = null;
                        }
                        /* Added by Pradeep I on 08/25/2013 -- End.*/

                    }
                }

                AlarmHelper.MarkAsCompleted(escState.AlarmObject, " esc process end of for loop ");
            }
            catch (Exception ex)
            {
                LogBook.Write(ex, "CooperAtkins.NotificationClient.EscalationModule.EscalationProcess", ErrorSeverity.High);
            }
        }
        /// <summary>
        /// Method to assign values to AlarmCurrentStatus Object
        /// </summary>
        /// <param name="alarmCurrentStatus"></param>
        /// Added on 08/25/2013 by Pradeep I
        private void SetAlarmCurrentStatusObject(AlarmObject alarmCurrentStatus, AlarmObject alarmObject, string notifytype, int escRecID)
        {

            alarmCurrentStatus.NotificationStartTime = DateTime.UtcNow;
            alarmCurrentStatus.IsElapsed = false;
            alarmCurrentStatus.NotificationType = notifytype;

            alarmCurrentStatus.AddtionalFields = alarmObject.AddtionalFields;
            alarmCurrentStatus.UTID = alarmObject.UTID;
            alarmCurrentStatus.Probe = alarmObject.Probe;
            alarmCurrentStatus.ProbeName = alarmObject.ProbeName;
            alarmCurrentStatus.ProbeName2 = alarmObject.ProbeName2;
            alarmCurrentStatus.FactoryID = alarmObject.FactoryID;
            alarmCurrentStatus.AlarmType = alarmObject.AlarmType;
            alarmCurrentStatus.AlarmID = alarmObject.AlarmID;
            alarmCurrentStatus.SensorType = alarmObject.SensorType;
            alarmCurrentStatus.SensorClass = alarmObject.SensorClass;
            alarmCurrentStatus.SensorAlarmID = alarmObject.SensorAlarmID;
            alarmCurrentStatus.AlarmTime = alarmObject.AlarmTime;
            alarmCurrentStatus.AlarmStartTime = alarmObject.AlarmStartTime;
            alarmCurrentStatus.Value = alarmObject.Value;
            alarmCurrentStatus.AlarmMaxValue = alarmObject.AlarmMaxValue;
            alarmCurrentStatus.AlarmMinValue = alarmObject.AlarmMinValue;
            alarmCurrentStatus.Threshold = alarmObject.Threshold;
            alarmCurrentStatus.CondThresholdMins = alarmObject.CondThresholdMins;
            alarmCurrentStatus.TimeOutOfRange = alarmObject.TimeOutOfRange;
            alarmCurrentStatus.NotificationID = alarmObject.NotificationID;
            alarmCurrentStatus.AlarmProfileID = alarmObject.AlarmProfileID;
            alarmCurrentStatus.NotifyProfileID = alarmObject.NotifyProfileID;
            alarmCurrentStatus.EscalationProfileID = alarmObject.EscalationProfileID;
            alarmCurrentStatus.PagerMessage = alarmObject.PagerMessage;
            alarmCurrentStatus.SwitchBitmask = alarmObject.SwitchBitmask;
            alarmCurrentStatus.Severity = alarmObject.Severity;
            alarmCurrentStatus.DisplayValue = alarmObject.DisplayValue;
            alarmCurrentStatus.ResetNotifyOnUserAck = alarmObject.ResetNotifyOnUserAck;
            alarmCurrentStatus.ResetNotifyOnSensorNormalRange = alarmObject.ResetNotifyOnSensorNormalRange;
            // Only allow "ShowCelsius" flag to be set for "TEMP" type sensors
            alarmCurrentStatus.IsCelsius = alarmObject.IsCelsius;
            alarmCurrentStatus.GroupName = alarmObject.GroupName;
            alarmCurrentStatus.IsResumedNitification = alarmObject.IsResumedNitification;
            alarmCurrentStatus.EscRecID = escRecID;
            alarmCurrentStatus.NotificationSentCount = alarmObject.NotificationSentCount;
        }

        /// <summary>
        /// To do the escalation process.
        /// </summary>
        /// <param name="escState"></param>
        /// <param name="escInfoObj"></param>
        private void DoEscalation(EscalationState escState, EscalationInfo escInfoObj)
        {
            /* checking for escalation type */
            if (escInfoObj.IsFailSafe)
            {
                escState.AlarmObject.IsFailsafeEscalationNotification = true;
                escState.AlarmObject.IsEscalationNotification = false;
            }
            else
            {
                escState.AlarmObject.IsFailsafeEscalationNotification = false;
                escState.AlarmObject.IsEscalationNotification = true;
            }

            /* Override NotifyProfileID, Severity and other information based on the escalation profile*/
            escState.AlarmObject.NotifyProfileID = escInfoObj.NotifyProfileID;
            escState.AlarmObject.PagerMessage = escInfoObj.PagerPrompt;
            escState.AlarmObject.Severity = escInfoObj.Severity;
            escState.AlarmObject.SwitchBitmask = escInfoObj.SwitchBitmask;

            /* Invoke notification client to send notification */
            escState.NotificationClient.Send(escState.AlarmObject);
        }

        /// <summary>
        /// Returns true if the sensor comes to the normal range.
        /// </summary>
        /// <returns></returns>
        private bool IsSensorInRange(AlarmObject alarmObject)
        {
            FailsafeEscalation failsafeEscalation = new FailsafeEscalation()
            {
                AlarmID = alarmObject.AlarmID,
                AlarmType = alarmObject.AlarmType,
                Probe = alarmObject.Probe,
                StoreID = alarmObject.StoreID,
                UTID = alarmObject.UTID
            };

            failsafeEscalation = failsafeEscalation.Execute();
            failsafeEscalation.Dispose();

            return failsafeEscalation.SensorIsInRange;
        }

        /// <summary>
        /// To update the alarm clearance information for given object.
        /// </summary>
        /// <param name="alarmObj"></param>
        private void CheckClearedAlarm(EscalationState escState, EscalationInfo escInoObj, bool isDelayType)
        {
            if (escState.AlarmObject.AlarmID == 0)
                return;
            /* checking for current object */
            using (NotificationAcknowledgement notificationAcknowledgement = new NotificationAcknowledgement())
            {

                notificationAcknowledgement.AlarmID = escState.AlarmObject.AlarmID;
                notificationAcknowledgement.StoreID = escState.AlarmObject.StoreID;
                notificationAcknowledgement.Execute();

                /* do not check for escalation if it is from contact sensor for the first time. it need to start escalation after the delay time. */
                if (isDelayType)
                {
                    /*if the alarm was cleared by user.*/
                    if (notificationAcknowledgement.IsAlarmCleared)
                        escState.AlarmObject.IsAlarmAcknowledgedOrCleared = true;



                    return;
                }

                escState.AlarmObject.IsAlarmClearedByUser = notificationAcknowledgement.IsAlarmClearedByUser;

                /* if the alarm was cleared for current object, update the information to current object */
                if (notificationAcknowledgement.IsAlarmCleared)
                {
                    escState.AlarmObject.IsAlarmAcknowledgedOrCleared = true;


                    /* User cleared alarm and sensor come back to the normal range, stop the whole process but not the dynamic notifications.  */
                    if (escInoObj.StopEscOnUserAck && !escState.AlarmObject.IsInAlarmState)
                    {
                        AlarmHelper.MarkAsCompleted(escState.AlarmObject, " esc profile set to stop on user ack AND sensor is back to normal range ");
                    }

                    //AlarmHelper.MarkAsCompleted(escState.AlarmObject);

                    /* if user configured to stop escalation once the alarm was cleared. */
                    if (escInoObj.StopEscOnUserAck)
                    {
                        if (!escState.AlarmObject.StopPendingEsc)
                        {
                            escState.AlarmObject.StopPendingEsc = true;
                            RecordNotification(NotifyTypes.NONE, escState.AlarmObject.NotificationID, NotifyStatus.STATUS, "Stopping all pending Escalations");
                        }
                    }


                    /* if there is no cleared time then assigning the current time as cleared time.*/
                    if (notificationAcknowledgement.ClearedTime == DateTime.MinValue)
                    {
                        escState.AlarmObject.AlarmClearedTime = DateTime.UtcNow;
                    }
                    else
                    {
                        escState.AlarmObject.AlarmClearedTime = notificationAcknowledgement.ClearedTime;
                    }

                    /* Log the information */
                    LogBook.Write("AlarmClearedByUser - " + escState.AlarmObject.ProbeName + " @ " + escState.AlarmObject.AlarmClearedTime.ToShortDateString() + " " + escState.AlarmObject.AlarmClearedTime.ToShortTimeString());

                    /* Record the notification information */
                    //RecordNotification(NotifyTypes.NONE, escState.AlarmObject.NotificationID, NotifyStatus.STATUS, "Alarm Record Acknowledged by User");
                }
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
        /// Check whether the user cleared the alarm or not.
        /// </summary>
        /// <param name="alarmObject"></param>
        /// <param name="escInoObj"></param>
        /// <returns></returns>
        private bool IsAlarmClearedOrAcknowledged(AlarmObject alarmObject, EscalationInfo escInoObj)
        {
            bool flag = false;
            /* if the alarm was cleared and user configured to stop the escalation on user acknowledgement then stop the escalation*/
            if (alarmObject.IsInAlarmState && escInoObj.StopEscOnUserAck)
            {
                LogBook.Write("ESCALATING  stopped Alarm cleared / unacknowledged by user. " + AlarmHelper.BasicAlarmInformation(alarmObject));
                LogBook.Debug(this, alarmObject);
                flag = true;
            }

            return flag;
        }
    }
}

