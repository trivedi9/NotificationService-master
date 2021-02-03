/*
 *  File Name : AlarmCurrentStatusContext.cs
 *  Author : Pradeep.I
 *  @ PCC Technology Group LLC
 *  Created Date : 08/25/2013
 */

namespace CooperAtkins.NotificationClient.EscalationModule
{
    using System;
    using EnterpriseModel.Net;
    using CooperAtkins.Generic;
    using CooperAtkins.Interface.Alarm;
    using CooperAtkins.NotificationClient.EscalationModule.DataAccess;
    
    public class AlarmCurrentStatusContext : DomainContext<AlarmObject, Criteria>
    {
        /// <summary>
        /// Method to insert current alarm status into the database
        /// </summary>
        /// <param name="entityObject"></param>
        /// <returns></returns>
        protected override AlarmObject Insert(AlarmObject entityObject)
        {
            /* Initializing the command object. */
            CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_ALARMCURRENTSTATUS);
            cmd.Parameters.AddWithValue("action", "C");
                                
            /*Bind parameters to command object.*/
            BindParameters(cmd, entityObject);

            /* Executing the procedure.*/
            CDAO.ExecCommand(cmd);
                       
            return entityObject;
        }
        /// <summary>
        /// Bind parameters to object
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="entity"></param>
        private void BindParameters(CSqlDbCommand cmd, AlarmObject entity)
        {
            cmd.Parameters.AddWithValue("UTID", entity.UTID);
            cmd.Parameters.AddWithValue("Probe", entity.Probe);
            cmd.Parameters.AddWithValue("EscalationProfileID", entity.EscalationProfileID);
            // This stored procedure was changed to an integer because on SQL 2000 the bit was being sent as a 'T' or an 'F' instead of a 1 or 0
            cmd.Parameters.AddWithValue("IsElapsed", Convert.ToInt32(entity.IsElapsed));
            cmd.Parameters.AddWithValue("NotificationType", entity.NotificationType);
            cmd.Parameters.AddWithValue("NotificationStartTime", entity.NotificationStartTime);
            cmd.Parameters.AddWithValue("EscalationRecID", entity.EscRecID);
            cmd.Parameters.AddWithValue("ProbeName", entity.ProbeName);
            cmd.Parameters.AddWithValue("ProbeName2", entity.ProbeName2);
            //alarmCurrentStatus.AddtionalFields = alarmObject.AddtionalFields;
            cmd.Parameters.AddWithValue("FactoryID", entity.FactoryID);
            cmd.Parameters.AddWithValue("AlarmType", entity.AlarmType);
            cmd.Parameters.AddWithValue("AlarmID", entity.AlarmID);
            cmd.Parameters.AddWithValue("SensorType", entity.SensorType);
            cmd.Parameters.AddWithValue("SensorClass", entity.SensorClass);
            cmd.Parameters.AddWithValue("SensorAlarmID", entity.SensorAlarmID);
            cmd.Parameters.AddWithValue("AlarmTime", entity.AlarmTime);
            cmd.Parameters.AddWithValue("AlarmStartTime", entity.AlarmStartTime);
            cmd.Parameters.AddWithValue("Value", entity.Value);
            cmd.Parameters.AddWithValue("AlarmMaxValue", entity.AlarmMaxValue);
            cmd.Parameters.AddWithValue("AlarmMinValue", entity.AlarmMinValue);
            cmd.Parameters.AddWithValue("Threshold", entity.Threshold);
            cmd.Parameters.AddWithValue("CondThresholdMins", entity.CondThresholdMins);
            cmd.Parameters.AddWithValue("TimeOutOfRange", entity.TimeOutOfRange);
            cmd.Parameters.AddWithValue("NotificationID", entity.NotificationID);
            cmd.Parameters.AddWithValue("AlarmProfileID", entity.AlarmProfileID);
            cmd.Parameters.AddWithValue("NotifyProfileID", entity.NotifyProfileID);
            cmd.Parameters.AddWithValue("PagerMessage", entity.PagerMessage);
            cmd.Parameters.AddWithValue("SwitchBitmask", entity.SwitchBitmask);
            cmd.Parameters.AddWithValue("Severity", entity.Severity);
            cmd.Parameters.AddWithValue("DisplayValue", entity.DisplayValue);
            // This stored procedure was changed to an integer because on SQL 2000 the bit was being sent as a 'T' or an 'F' instead of a 1 or 0
            cmd.Parameters.AddWithValue("ResetNotifyOnUserAck", Convert.ToInt32(entity.ResetNotifyOnUserAck));
            cmd.Parameters.AddWithValue("ResetNotifyOnSensorNormalRange", Convert.ToInt32(entity.ResetNotifyOnSensorNormalRange));
            cmd.Parameters.AddWithValue("IsCelsius", Convert.ToInt32(entity.IsCelsius));
            cmd.Parameters.AddWithValue("GroupName", entity.GroupName);
            cmd.Parameters.AddWithValue("IsResumedNitification", Convert.ToInt32(entity.IsResumedNitification));
            cmd.Parameters.AddWithValue("NotificationSentCount", entity.NotificationSentCount);
        }

        protected override AlarmObject Load(Criteria criteria)
        {
            throw new NotImplementedException();
        }

        protected override int Remove(Criteria criteria)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Method to update isElapsed value to database based on UTID,Probe for which isElapsed is false.
        /// </summary>
        /// <param name="entityObject"></param>
        /// <returns></returns>
        protected override AlarmObject Update(AlarmObject entityObject)
        {
            CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_ALARMCURRENTSTATUS);
            cmd.Parameters.AddWithValue("action", "U");
            cmd.Parameters.AddWithValue("UTID", entityObject.UTID);
            cmd.Parameters.AddWithValue("Probe", entityObject.Probe);
            CDAO.ExecScalar(cmd);
            return entityObject;
        }
                
       
    }
}
