/*
* File : AlarmStatus.cs
* Author : Vasu
* Created Date :04/20/2011
* File Version : 1 .0
*/
namespace CooperAtkins.NotificationClient.Generic.DataAccess
{
    using System;
    using System.Collections.Generic;
    using EnterpriseModel.Net;

    /// <summary>
    /// To know the current alarm status (cleared by user or not).
    /// </summary>
    public class AlarmStatus : DomainCommandEx<AlarmStatus>
    {

        public int NotificationID { get; set; }
        public int StoreID { get; set; }
        public bool IsAlarmClearedOrBackInRange { get; set; }


        protected override AlarmStatus ExecuteCommand()
        {
            try
            {
                //Initialize the CSqlDbCommand for execute the stored procedure
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_ISALARMCLEARDORBACKINRANGE);

                //Bind IvrAlarmStatus  property values as input parameters for procedure
                cmd.AddWithValue("NotificationID", NotificationID);
                cmd.AddWithValue("StoreID", StoreID);

                //Execute command 
                CDAO.ExecReader(cmd);

                //Bind output values to IvrAlarmStatus object
                if (CDAO.DataReader.Read())
                    IsAlarmClearedOrBackInRange = true;                   
                else
                    IsAlarmClearedOrBackInRange = false;
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

            //return IvrAlarmStatus object
            return this;
        }


    }
}