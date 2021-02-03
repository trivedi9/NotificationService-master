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
    using CooperAtkins.NotificationClient.Generic.DataAccess;
    using CooperAtkins.Generic;


    /// <summary>
    /// To know the current alarm status (cleared by user or not).
    /// </summary>
    public class IvrIsInProcess : DomainCommandEx<IvrIsInProcess>
    {

        //public int AlarmID { get; set; }
        public int NotificationID { get; set; }
        public int StoreID { get; set; }
        public bool IsAlarmInIVRProcess { get; set; }
        public bool IsSucceeded { get; set; }
        public string phoneNumber { get; set; }
        //public DateTime LastAttemptTime { get; set; }
        public int numAttempts { get; set; }
        //public int threadID { get; set; }      

        protected override IvrIsInProcess ExecuteCommand()
        {
            try
            {
                //Initialize the CSqlDbCommand for execute the stored procedure
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_IVR_ISINPROCESS);


              //  cmd.AddWithValue("NotificationID", NotificationID);
                cmd.AddWithValue("StoreID", StoreID);
                // This stored procedure was changed to an integer because on SQL 2000 the bit was being sent as a 'T' or an 'F' instead of a 1 or 0

                cmd.AddWithValue("PhoneNumber", phoneNumber);
             //   cmd.AddWithValue("AlarmID", AlarmID);
            //    cmd.AddWithValue("isSucceeded", IsSucceeded.ToInt());
             //   cmd.AddWithValue("numAttempts", numAttempts);
                
                


                //Execute command 
                CDAO.ExecReader(cmd);
                //while (CDAO.DataReader.Read())
                //{
                //    IsAlarmInIVRProcess = CDAO.DataReader["IsInProcess"].ToBoolean();  
                //    LastAttemptTime = TypeCommonExtensions.IfNull(CDAO.DataReader["LastAttemptTime"], DateTime.UtcNow).ToDateTime();
                //    phoneNumber = CDAO.DataReader["PhoneNumber"].ToString();
                //    IsSucceeded = CDAO.DataReader["IsSuccess"].ToBoolean();
                //    numAttempts = CDAO.DataReader["numAttempts"].ToInt();
                //    threadID = CDAO.DataReader["ThreadID"].ToInt();

                //}
               
                // Bind output values to IvrAlarmStatus object
               // if stored procedure returns a record, then it is either cleared,in-range, or in-process (call alert is not complete)
                if (CDAO.DataReader.Read())
                {
                    IsAlarmInIVRProcess = true;
                   // IsSucceeded = CDAO.DataReader["IsSuccess"].ToBoolean();
                  //  numAttempts = CDAO.DataReader["numAttempts"].ToInt();
                }
                else
                    IsAlarmInIVRProcess = false;
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