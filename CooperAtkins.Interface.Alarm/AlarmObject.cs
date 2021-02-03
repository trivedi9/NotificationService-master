/*
 *  File Name : AlarmObject.cs
 *  Author : Vasu 
 *           @ PCC Technology Group LLC
 *  Created Date : 12/04/2010
 *  
 */
namespace CooperAtkins.Interface.Alarm
{
    using System;
    using System.Collections.Generic;
    using EnterpriseModel.Net;
    using System.Collections;

    public class AlarmObject : DomainEntity
    {
        /// <summary>
        /// Current location ID
        /// </summary>
        public int StoreID { get; set; }


        /// <summary>
        /// Sensor alarm type information identification unique id
        /// The combination of UTID, Probe and AlarmType,
        /// used to determine whether the same type alarm was generated or not.
        /// </summary>
        public string SensorAlarmID { get; set; }

        /// <summary>
        /// Used to insert the note, when the same alarm type is in process
        /// </summary>
        public int AlarmID { get; set; }

        /// <summary>
        /// Type of the alarm. 
        /// 0=Sensor, 1=Communications, 2=Battery, 100=Reset Sensor,
        /// 101=Reset Communications, 102=Reset Battery.
        /// </summary>
        public Int16 AlarmType { get; set; }

        /// <summary>
        /// Type of the sensor. Ex Temp, Contact etc
        /// </summary>
        public string SensorType { get; set; }

        /// <summary>
        /// Sensor's unique id.
        /// </summary>
        public string UTID { get; set; }

        /// <summary>
        /// Returns if alarm cleared or acknowledged.
        /// </summary>
        public bool IsAlarmAcknowledgedOrCleared { get; set; }

        /// <summary>
        /// Returns true if user clears the alarm.
        /// </summary>
        public bool IsAlarmClearedByUser { get; set; }

        /// <summary>
        /// if the notification was cleared/acknowledged by the user, update the cleared time.
        /// </summary>
        public DateTime AlarmClearedTime { get; set; }

        /// <summary>
        /// The alarm object will be removed from the process queue when it is set to true.
        /// Set as true once the alarm process is completed.
        /// </summary>
        public bool IsProcessCompleted { get; set; }


        /// <summary>
        /// To store the alarm exit time.
        /// </summary>
        public DateTime AlarmStateExitTime { get; set; }


        /* Sensor related information. */
        public int Probe { get; set; }
        public string ProbeName { get; set; }
        public string ProbeName2 { get; set; }
        public string FactoryID { get; set; }
        public string SensorClass { get; set; }


        //Need to verify it need or not.
        public string AlarmMessage { get; set; }

        /// <summary>
        /// Max temperature value for the sensor.
        /// </summary>
        public decimal AlarmMaxValue { get; set; }

        /// <summary>
        /// Min temperature value for the sensor.
        /// </summary>
        public decimal AlarmMinValue { get; set; }

        /// <summary>
        /// To display user defined format message.
        /// </summary>
        public long Threshold { get; set; }


        /// <summary>
        /// To display user defined format message.
        /// </summary>
        public int CondThresholdMins { get; set; }

        /// <summary>
        /// To display user defined format message.
        /// </summary>
        public long TimeOutOfRange { get; set; }

        //public bool IsNewAlarmRecordWasCreated { get; set; }
               
        public string DisplayValue { get; set; }
        public int CurrentAlarmMinutes { get; set; }

        /// <summary>
        /// Current notification ID.
        /// </summary>
        public int NotificationID { get; set; }

        /// <summary>
        /// Current Transaction ID from 3rd party IVR service.
        /// </summary>
        public int TransID { get; set; }

        /// <summary>
        /// Sensor's Alarm Profile ID.
        /// </summary>
        public int AlarmProfileID { get; set; }

        /// <summary>
        /// Sensor's notification Profile ID.
        /// </summary>
        public int NotifyProfileID { get; set; }

        /// <summary>
        /// Sensor's Escalation Profile ID.
        /// </summary>
        public int EscalationProfileID { get; set; }


        /// <summary>
        /// Indicates the current notification Severity.
        /// </summary>
        public Severity Severity { get; set; }


        public string GroupName { get; set; }

        public bool IsCelsius { get; set; }

        public bool HasEscalations { get; set; }
        /// <summary>
        /// Returns true if the notification type is an escalation.
        /// </summary>
        public bool IsEscalationNotification { get; set; }

        /// <summary>
        /// Returns true if the notification type is failsafe escalation.
        /// </summary>
        public bool IsFailsafeEscalationNotification { get; set; }



        public DateTime AlarmTime { get; set; }
        public DateTime AlarmStartTime { get; set; }

        /// <summary>
        /// Current sensor data.
        /// </summary>
        public decimal? Value { get; set; }

        /// <summary>
        /// Additional fields other than the specific alarm object fields.
        /// which may require for custom modules (plug-ins).
        /// </summary>
        public Hashtable AddtionalFields { get; set; }

        /// <summary>
        /// To know the status/location of the current object on the way.
        /// </summary>
        public List<string> ProcessStatus { get; set; }

        /// <summary>
        /// Returns to true once the sensor goes to the alarm state and 
        /// returns false if sensor comes to normal range. Do not continue the notification process if it is False.
        /// </summary>
        public bool IsInAlarmState { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public int NotificationSentCount { get; set; }


        /// <summary>
        /// To hold pager message for specific escalation profile, it will be changed based on the escalation profile.
        /// </summary>
        public string PagerMessage { get; set; }

        /// <summary>
        /// To hold switch info for specific escalation profile, it will be changed based on the escalation profile.
        /// </summary>
        public Int16 SwitchBitmask { get; set; }

        /// <summary>
        /// To whether current alarm has any dynamic notification types (Message Board or Relay switch).
        /// </summary>
        public bool HasDynamicTypes { get; set; }

        /// <summary>
        /// To know  whether current alarm has any previous dynamic notification types (Message Board).
        /// </summary>
        public bool HasPreviousMessageBoard { get; set; }

        /// <summary>
        /// To know  whether current alarm has any previous dynamic notification types (Relay switch).
        /// </summary>
        public bool HasPreviousSwitch { get; set; }

        /// <summary>
        /// Returns true if it is the server popup.
        /// </summary>
        public bool IsServerPopup { get; set; }

        /// <summary>
        /// Returns true if the dynamic notification was cleared.
        /// </summary>
        public bool IsDynamicNotificationCleared { get; set; }

        /// <summary>
        /// Returns true if the dynamic notification was removed from the message board or switch or for other devices.
        /// </summary>
        public bool IsDynamicNotificationRemoved { get; set; }


        /// <summary>
        /// Returns true if user configured for StopEscalationOnExitAlarm
        /// </summary>
        public bool StopEscalationOnExitAlarm { get; set; }

        /// <summary>
        /// Returns true if user configured for StopEscalationOnUserAck
        /// </summary>
        public bool StopEscalationOnUserAck { get; set; }

        /// <summary>
        /// Configuration for dynamic notification 
        /// </summary>
        public bool ResetNotifyOnUserAck { get; set; }

        /// <summary>
        /// Configuration for dynamic notification 
        /// </summary>
        public bool ResetNotifyOnSensorNormalRange { get; set; }

        /// <summary>
        /// Returns true if the notification type is missed communication error.
        /// </summary>
        public bool IsMissCommNotification { get; set; }

        /// <summary>
        /// To hold the missed communication sensor count.
        /// </summary>
        public int MissedCommSensorCount { get; set; }

        /// <summary>
        /// To hold the missed communication sensors detailed information.
        /// </summary>
        public string MissedCommSensorInfo { get; set; }

        /// <summary>
        /// Returns the phone number for IVR Notification
        /// </summary>
        public string IVRPhoneNumber { get; set; }

        /// <summary>
        /// Returns the phone number for IVR Notification
        /// </summary>
        public string PersonName { get; set; }

        /// <summary>
        /// Returns the user id for IVR Notification
        /// </summary>
        public int IVRUserID { get; set; }

        /// <summary>
        /// Returns the success for IVR Notification
        /// </summary>
        public bool IVRSuccess { get; set; }

        /// <summary>
        /// Returns the ThreadID spawned when fired dial thread for alarm
        /// </summary>
        public int ThreadID { get; set; }

        /// <summary>
        /// Returns if it is a resumed notification.
        /// </summary>
        public bool IsResumedNitification { get; set; }

        /// <summary>
        /// to indentify whether DynamicNotificationClearProcessStarted
        /// </summary>
        public bool IsDynamicNotificationClearProcessStarted { get; set; }

        /// <summary>
        /// to indentify whether DynamicNotificationClearProcessStarted
        /// </summary>
        public bool IsSwitchNotificationClearProcessStarted { get; set; }

        /// <summary>
        /// to store the database name in, multi db scenario
        /// </summary>
        public string InstanceDBName { get; set; }

        /// <summary>
        /// To send server time to all configured message boards
        /// </summary>
        public bool SetServerTime { get; set; } /* added on 02/28/2011 to set server time to message boards*/


        public bool BackInAcceptLogged { get; set; }


        public bool StopPendingEsc { get; set; }
        /// <summary>
        /// Use in cluster environment to flag whether to save the current state to database
        /// </summary>
        /// Added on 08/25/2013
        public bool SaveCurrentState { get; set; }
        /// <summary>
        /// flag to check if escalation elapsed, or sent out
        /// </summary>
        /// Added on 08/25/2013
        public bool IsElapsed { get; set; }
        /// <summary>
        /// to know which type of escalation is being sent out
        /// </summary>
        /// Added on 08/25/2013
        public string NotificationType { get; set; }
        /// <summary>
        /// time at which the notification started
        /// </summary>
        /// Added on 08/25/2013
        public DateTime NotificationStartTime { get; set; }
        /// <summary>
        /// store the current escalation record id to continue from that record while the service restarts
        /// </summary>
        /// Added on 09/02/2013
        public int EscRecID { get; set; }

        

    }
}
