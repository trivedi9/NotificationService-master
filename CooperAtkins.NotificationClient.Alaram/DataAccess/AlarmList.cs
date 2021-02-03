/*
 *  File Name : AlarmList.cs
 *  Author : Vasu Ravuri 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/24/2010
 *  Description: To get alarm list.
 */
namespace CooperAtkins.NotificationClient.Alarm.DataAccess
{
    using System;
    using System.Collections.Generic;
    using CooperAtkins.Interface.Alarm;
    using EnterpriseModel.Net;
    using CooperAtkins.Generic;
    using CooperAtkins.NotificationClient.Generic;

    public class AlarmList : DomainListBase<AlarmList, AlarmObject>
    {
        /// <summary>
        /// Gets alarm list.
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        protected override AlarmList LoadList(BaseCriteria criteria)
        {
            try
            {
                Criteria listCriteria = (Criteria)criteria;

                //Initialize the CSqlDbCommand for execute the stored procedure
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_GET_ALARMLIST, System.Data.CommandType.StoredProcedure);

                //Execute reader 
                CDAO.ExecReader(cmd);
                bool flag = true;

                System.Collections.Hashtable extFields = null;
                List<string> fields = new List<string>();

                while (CDAO.DataReader.Read())
                {
                    //For the first time get all the field names
                    if (flag)
                    {
                        for (int index = 0; index < CDAO.DataReader.FieldCount; index++)
                        {
                            fields.Add(CDAO.DataReader.GetName(index));
                        }

                        //Remove known fields from the list.
                        fields.Remove("UTID");
                        fields.Remove("Probe");
                        fields.Remove("ProbeName");
                        fields.Remove("PuckDesc2");
                        fields.Remove("FactoryID");
                        fields.Remove("AlarmType");
                        fields.Remove("AlarmID");
                        fields.Remove("SensorType");
                        fields.Remove("SensorClass");
                        fields.Remove("Options");
                        fields.Remove("AlarmData");
                        fields.Remove("CondMaxValue");
                        fields.Remove("CondMinValue");
                        fields.Remove("CondThresholdMins");
                        fields.Remove("NotificationRecID");
                        fields.Remove("AlarmProfileRecID");
                        fields.Remove("NotificationProfileID");
                        fields.Remove("EscalationProfileID");
                        fields.Remove("AlarmTime");
                        fields.Remove("AlarmStartTime");
                        fields.Remove("PagerMessage");
                        fields.Remove("SwitchBitmask");
                        fields.Remove("Severity");
                        fields.Remove("GroupName");
                        fields.Remove("NotifyResetFlags");


                        flag = false;
                    }


                    extFields = new System.Collections.Hashtable();

                    //Adding all extra fields in to a hash table.
                    foreach (string item in fields)
                    {
                        extFields.Add(item, CDAO.DataReader[item]);
                    }


                    AlarmObject alarm = new AlarmObject();
                    alarm.AddtionalFields = extFields;


                    alarm.UTID = CDAO.DataReader["UTID"].ToStr();
                    alarm.Probe = CDAO.DataReader["Probe"].ToInt();
                    alarm.ProbeName = CDAO.DataReader["ProbeName"].ToStr();
                    alarm.ProbeName2 = CDAO.DataReader["PuckDesc2"].ToStr();
                    alarm.FactoryID = CDAO.DataReader["FactoryID"].ToStr();
                    alarm.AlarmType = CDAO.DataReader["AlarmType"].ToInt16();
                    alarm.AlarmID = CDAO.DataReader["AlarmID"].ToInt();
                    alarm.SensorType = CDAO.DataReader["SensorType"].ToStr();
                    alarm.SensorClass = CDAO.DataReader["SensorClass"].ToStr();
                    //alarm.IsNewAlarmRecordWasCreated = (CDAO.DataReader["Options"].ToInt() == 0 ? false : true);


                    if (alarm.AlarmType >= AlarmType.RESETMODE)
                    {
                        //if the alarm type is reset, then 
                        alarm.SensorAlarmID = alarm.UTID + "_" + alarm.Probe + "_" + (alarm.AlarmType - AlarmType.RESETMODE).ToString();
                    }
                    else
                    {
                        alarm.SensorAlarmID = alarm.UTID + "_" + alarm.Probe + "_" + alarm.AlarmType.ToString();
                    }

                    alarm.AlarmTime = TypeCommonExtensions.IfNull(CDAO.DataReader["AlarmTime"], DateTime.UtcNow).ToDateTime();

                    alarm.AlarmStartTime = TypeCommonExtensions.IfNull(CDAO.DataReader["AlarmStartTime"], alarm.AlarmTime).ToDateTime();

                    if (alarm.AlarmStartTime == DateTime.MinValue || alarm.AlarmStartTime < DateTime.Parse("1/1/2000"))
                        alarm.AlarmStartTime = alarm.AlarmTime;

                    alarm.Value = CDAO.DataReader["AlarmData"].ToDecimal();

                    alarm.AlarmMaxValue = CDAO.DataReader["CondMaxValue"].ToDecimal();
                    alarm.AlarmMinValue = CDAO.DataReader["CondMinValue"].ToDecimal();
                    alarm.Threshold = Common.DateDiff("n", alarm.AlarmStartTime, alarm.AlarmTime);
                    alarm.CondThresholdMins = CDAO.DataReader["CondThresholdMins"].ToInt();
                    alarm.TimeOutOfRange = Common.DateDiff("n", alarm.AlarmStartTime, alarm.AlarmTime);
                    alarm.NotificationID = CDAO.DataReader["NotificationRecID"].ToInt();
                    alarm.AlarmProfileID = CDAO.DataReader["AlarmProfileRecID"].ToInt();
                    alarm.NotifyProfileID = CDAO.DataReader["NotificationProfileID"].ToInt();
                    alarm.EscalationProfileID = CDAO.DataReader["EscalationProfileID"].ToInt();
                    alarm.PagerMessage = CDAO.DataReader["PagerPrompt"].ToStr();
                    alarm.SwitchBitmask = CDAO.DataReader["SwitchBitmask"].ToInt16();
                    alarm.Severity = (Severity)CDAO.DataReader["Severity"].ToInt();
                    alarm.DisplayValue = alarm.Value.ToString();
                    alarm.ResetNotifyOnUserAck = (CDAO.DataReader["NotifyResetFlags"].ToInt() & 1).ToBoolean();
                    alarm.ResetNotifyOnSensorNormalRange = (CDAO.DataReader["NotifyResetFlags"].ToInt() & 2).ToBoolean();

                    if (AlarmHelper.IsContactSensor(alarm.SensorType))
                    {
                        if (alarm.Value == 0)
                            alarm.DisplayValue = "CLOSED";
                        else
                            alarm.DisplayValue = "OPEN";
                    }


                    // Only allow "ShowCelsius" flag to be set for "TEMP" type sensors
                    alarm.IsCelsius = (AlarmHelper.IsTempSensor(alarm.SensorType) ? (((CDAO.DataReader["NotifyFlags"].ToInt()) & 0x1) != 0) : false);

                    alarm.GroupName = CDAO.DataReader["GroupName"].ToStr();

                    //ProcessStatus will be returned as "O" if it is a old notification.  
                    if (CDAO.DataReader["ProcessStatus"].ToStr() == "O")
                        alarm.IsResumedNitification = true;
                    else
                        alarm.IsResumedNitification = false;



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
                // Added on 2/19/2012
                // To dispose the data reader object
                // Srinivas Rao E
                CDAO.Dispose();
            }
            return this;
        }


    }
}
