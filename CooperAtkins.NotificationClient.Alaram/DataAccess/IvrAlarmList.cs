/*
 *  File Name : IvrAlarmList.cs
 *  Author : Vasu Ravuri 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/24/2010
 *  Description: To get IVR alarm list.
 */
namespace CooperAtkins.NotificationClient.Alarm.DataAccess
{
    using System;
    using CooperAtkins.Interface.Alarm;
    using EnterpriseModel.Net;
    using CooperAtkins.Generic;
    using CooperAtkins.NotificationClient.Generic.DataAccess;

    public class IvrAlarmList : DomainListBase<IvrAlarmList, AlarmObject>
    {
        public int StoreID { get; set; }
        public int NumAttempts { get; set; }
        /// <summary>
        /// Gets all active IVR alarms.
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        protected override IvrAlarmList LoadList(BaseCriteria criteria)
        {
            try
            {
                Criteria listCriteria = (Criteria)criteria;

                //Initialize the CSqlDbCommand for execute the stored procedure
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_GET_IVRALARMLIST, System.Data.CommandType.StoredProcedure);
                cmd.AddWithValue("numAttempts", NumAttempts );
                //Execute reader 
                CDAO.ExecReader(cmd);                 

                /*fill the object and add to list.*/
                while (CDAO.DataReader.Read())
                {

                    IvrAlarm alarm = new IvrAlarm();

                    alarm.IVRPhoneNumber = CDAO.DataReader["PhoneNumber"].ToStr();
                    alarm.AlarmID = CDAO.DataReader["AlarmID"].ToInt();
                    alarm.IsSuccess = CDAO.DataReader["isSuccess"].ToBoolean();                   
                    alarm.NotificationID = CDAO.DataReader["Notification_RecID"].ToInt();
                    alarm.AttemptCount = CDAO.DataReader["numAttempts"].ToInt16();
                    alarm.QueueTime = TypeCommonExtensions.IfNull(CDAO.DataReader["QueueTime"], DateTime.UtcNow).ToDateTime();
                    alarm.LastAttemptTime = TypeCommonExtensions.IfNull(CDAO.DataReader["LastAttemptTime"], DateTime.UtcNow).ToDateTime();
                    alarm.IvrAlarmID = CDAO.DataReader["RecID"].ToInt();

                    alarm.UTID = CDAO.DataReader["UTID"].ToStr();
                    alarm.Probe = CDAO.DataReader["Probe"].ToInt();
                    alarm.SensorType = CDAO.DataReader["SensorType"].ToStr();
                    if (CDAO.DataReader["IVR_SensorName"] != DBNull.Value)
                    {
                        alarm.IVR_SensorName = CDAO.DataReader["IVR_SensorName"].ToStr();
                    }
                    else
                    {
                        alarm.IVR_SensorName = (CDAO.DataReader["PuckName"].ToStr() == string.Empty ? "Sensor" : CDAO.DataReader["PuckName"].ToStr());
                    }

                    alarm.Value = CDAO.DataReader["AlarmData"].ToDecimal();

                    alarm.AlarmMaxValue = CDAO.DataReader["CondMaxValue"].ToDecimal();
                    alarm.AlarmMinValue = CDAO.DataReader["CondMinValue"].ToDecimal();
                    alarm.CondThresholdMins = CDAO.DataReader["CondThresholdMins"].ToInt();
                    alarm.AlarmTime = TypeCommonExtensions.IfNull(CDAO.DataReader["AlarmTime"], DateTime.UtcNow).ToDateTime();

                    alarm.IVRUserID = CDAO.DataReader["UserID"].ToInt();
                    alarm.PersonName = CDAO.DataReader["FirstName"].ToString() +" "+ CDAO.DataReader["LastName"].ToString();

                    alarm.LanguageID = (CDAO.DataReader["LanguageID"].ToInt() == 0 ? 1 : CDAO.DataReader["LanguageID"].ToInt());
                    alarm.IsCelsius = CDAO.DataReader["isCelsius"].ToBoolean();
                    alarm.StoreName = GenStoreInfo.GetInstance().StoreName;                    
                    alarm.IvrID = CDAO.DataReader["RecID"].ToInt();
                    alarm.StoreNumber = GenStoreInfo.GetInstance().StorePhoneNumber.ToString();
                    alarm.AlarmStartTime = TypeCommonExtensions.IfNull(CDAO.DataReader["AlarmStartTime"], DateTime.UtcNow).ToDateTime();

                    //if (alarm.AlarmID > 0)
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
