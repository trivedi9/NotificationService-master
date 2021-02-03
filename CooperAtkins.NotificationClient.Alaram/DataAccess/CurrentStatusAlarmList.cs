

namespace CooperAtkins.NotificationClient.Alarm.DataAccess
{
    using EnterpriseModel.Net;
    using CooperAtkins.Interface.Alarm;
    using CooperAtkins.Generic;
    using System;

    public class CurrentStatusAlarmList : DomainListBase<CurrentStatusAlarmList, AlarmObject>
    {
        protected override CurrentStatusAlarmList LoadList(BaseCriteria baseCriteria)
        {
            try
            {
                Criteria criteria = (Criteria)baseCriteria;
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_ALARMCURRENTSTATUS, System.Data.CommandType.StoredProcedure);
                cmd.Parameters.AddWithValue("action", "L");

                // This stored procedure was changed to an integer because on SQL 2000 the bit was being sent as a 'T' or an 'F' instead of a 1 or 0

                if (criteria.Fields != null)
                    cmd.Parameters.AddWithValue("isServiceRestarted",  Convert.ToInt32(criteria.Fields["isServiceRestarted"]));
                else
                    cmd.Parameters.AddWithValue("isServiceRestarted", DBNull.Value);

                CDAO.ExecReader(cmd);

                while (CDAO.DataReader.Read())
                {
                    AlarmObject alarm = new AlarmObject();
                    alarm.UTID = CDAO.DataReader["UTID"].ToStr();
                    alarm.Probe = CDAO.DataReader["Probe"].ToInt();
                    alarm.ProbeName = CDAO.DataReader["ProbeName"].ToStr();
                    alarm.ProbeName2 = CDAO.DataReader["ProbeName2"].ToStr();
                    alarm.FactoryID = CDAO.DataReader["FactoryID"].ToStr();
                    alarm.AlarmType = CDAO.DataReader["AlarmType"].ToInt16();
                    alarm.AlarmID = CDAO.DataReader["AlarmID"].ToInt();
                    alarm.SensorType = CDAO.DataReader["SensorType"].ToStr();
                    alarm.SensorClass = CDAO.DataReader["SensorClass"].ToStr();
                    alarm.SensorAlarmID = CDAO.DataReader["SensorAlarmID"].ToStr();
                    alarm.AlarmTime = CDAO.DataReader["AlarmTime"].ToDateTime();
                    alarm.AlarmStartTime = CDAO.DataReader["AlarmStartTime"].ToDateTime();
                    alarm.Value = CDAO.DataReader["Value"].ToDecimal();
                    alarm.AlarmMaxValue = CDAO.DataReader["AlarmMaxValue"].ToDecimal();
                    alarm.AlarmMinValue = CDAO.DataReader["AlarmMinValue"].ToDecimal();
                    alarm.Threshold = CDAO.DataReader["Threshold"].ToInt();
                    alarm.CondThresholdMins = CDAO.DataReader["CondThresholdMins"].ToInt();
                    alarm.TimeOutOfRange = CDAO.DataReader["TimeOutOfRange"].ToInt();
                    alarm.NotificationID = CDAO.DataReader["NotificationID"].ToInt();
                    alarm.AlarmProfileID = CDAO.DataReader["AlarmProfileID"].ToInt();
                    alarm.NotifyProfileID = CDAO.DataReader["NotifyProfileID"].ToInt();
                    alarm.EscalationProfileID = CDAO.DataReader["EscalationProfileID"].ToInt();
                    alarm.PagerMessage = CDAO.DataReader["PagerMessage"].ToStr();
                    alarm.SwitchBitmask = CDAO.DataReader["SwitchBitmask"].ToInt16();
                    alarm.Severity = (Severity)CDAO.DataReader["Severity"].ToInt();
                    alarm.ResetNotifyOnUserAck = CDAO.DataReader["ResetNotifyOnUserAck"].ToBoolean();
                    alarm.ResetNotifyOnSensorNormalRange = CDAO.DataReader["ResetNotifyOnSensorNormalRange"].ToBoolean();
                    alarm.DisplayValue = CDAO.DataReader["DisplayValue"].ToStr();
                    // Only allow "ShowCelsius" flag to be set for "TEMP" type sensors
                    alarm.IsCelsius = CDAO.DataReader["IsCelsius"].ToBoolean();
                    alarm.GroupName = CDAO.DataReader["GroupName"].ToStr();
                    alarm.IsResumedNitification = CDAO.DataReader["IsResumedNitification"].ToBoolean();
                    alarm.NotificationStartTime = CDAO.DataReader["NotificationStartTime"].ToDateTime();
                    alarm.IsElapsed = CDAO.DataReader["IsElapsed"].ToBoolean();
                    alarm.NotificationType = CDAO.DataReader["NotificationType"].ToStr();
                    alarm.EscRecID = CDAO.DataReader["EscalationRecID"].ToInt();
                    alarm.NotificationSentCount = CDAO.DataReader["NotificationSentCount"].ToInt();
                    this.Add(alarm);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                CDAO.CloseDataReader();
                CDAO.Dispose();
            }
            return this;
        }
    }
}
