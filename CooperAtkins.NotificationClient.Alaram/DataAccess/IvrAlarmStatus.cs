/*
* File : IvrAlarmStatus.cs
* Author : Vasu
* Created Date :1/3/2011
* File Version : 1 .0
*/
namespace CooperAtkins.NotificationClient.Alarm.DataAccess
{
    using System;
    using System.Collections.Generic;
    using EnterpriseModel.Net;

    /// <summary>
    /// To know the current alarm status (cleared by user or not).
    /// </summary>
    public class IvrAlarmStatus : DomainCommandEx<IvrAlarmStatus>
    {

        public int AlarmID { get; set; }
        public int StoreID { get; set; }
        public bool IsAlarmCleared { get; set; }
        public bool IsSucceeded { get; set; }
        

        protected override IvrAlarmStatus ExecuteCommand()
        {
            //Initialize the CSqlDbCommand for execute the stored procedure
            CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_IVR_ALARMCLEARED);

            //Bind IvrAlarmStatus  property values as input parameters for procedure
            cmd.AddWithValue("AlarmID", AlarmID);
            cmd.AddWithValue("StoreID", StoreID);
            cmd.AddWithValue("IsSucceeded", IsSucceeded);

            //Execute command 
            CDAO.ExecReader(cmd);

            //Bind output values to IvrAlarmStatus object
            if (CDAO.DataReader.Read())
                IsAlarmCleared = true;
            else
                IsAlarmCleared = false;

            //return IvrAlarmStatus object
            return this;
        }


    }
}