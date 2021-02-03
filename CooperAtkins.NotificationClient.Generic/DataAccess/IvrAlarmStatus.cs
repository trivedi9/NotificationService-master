/*
* File : IvrAlarmStatus.cs
* Author : Vasu
* Created Date :1/3/2011
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
    public class IvrAlarmStatus : DomainCommandEx<IvrAlarmStatus>
    {

        public int AlarmID { get; set; }       
        public int NotificationID { get; set; }
        public int StoreID { get; set; }
        public bool IsAlarmClearedOrBackInRange { get; set; }
        public bool IsSucceeded { get; set; }
        public bool Completed { get; set; }
       


        protected override IvrAlarmStatus ExecuteCommand()
        {
            try
            {
                //Initialize the CSqlDbCommand for execute the stored procedure
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_ISALARMCLEARDORBACKINRANGE);

                //Bind IvrAlarmStatus  property values as input parameters for procedure
               // cmd.AddWithValue("AlarmID"    , AlarmID);
                cmd.AddWithValue("NotificationID", NotificationID);
                cmd.AddWithValue("StoreID", StoreID);
                // This stored procedure was change to an integer because on SQL 2000 the bit was being sent as a 'T' or an 'F' instead of a 1 or 0
                //cmd.AddWithValue("IsSucceeded", Convert.ToInt32(IsSucceeded)); //call successed or not
             
               

                //Execute command 
                CDAO.ExecReader(cmd);

                //Bind output values to IvrAlarmStatus object
                //if stored procedure returns a record, then it is either cleared,in-range, or in-process (call alert is not complete)
                if (CDAO.DataReader.Read())
                {
                    //if (CDAO.DataReader.HasRows)
                    IsAlarmClearedOrBackInRange = true;
                }
                else
                {
                    IsAlarmClearedOrBackInRange = false;
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

            //return IvrAlarmStatus object
            return this;
        }


    }
}