/*
 *  File Name : NotificationAcknowledgement.cs
 *  Author : Vasu Ravuri 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/25/2010
 *  Description: To add note to the existing notifications.
 */

namespace CooperAtkins.NotificationClient.Generic.DataAccess
{
    using System;
    using EnterpriseModel.Net;
    using CooperAtkins.Generic;

    public class NotificationAcknowledgement : DomainCommandEx<NotificationAcknowledgement>
    {
        public int AlarmID { get; set; }
        public int StoreID { get; set; }
        /// <summary>
        /// Indicates whether the alarm was acknowledged or cleared
        /// </summary>
        public bool IsAlarmCleared { get; set; }

        /// <summary>
        /// Indicates whether the alarm cleared by user or not.
        /// </summary>
        public bool IsAlarmClearedByUser { get; set; }
        public DateTime ClearedTime { get; set; }


        protected override NotificationAcknowledgement ExecuteCommand()
        {
            try
            {
                /* Create command object for sql operation*/
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_IS_NOTIFICATION_ACKNOWLEDGED, System.Data.CommandType.StoredProcedure);

                cmd.AddWithValue("AlarmID", AlarmID);
                cmd.AddWithValue("StoreID", StoreID);

                /* execute the command */
                CDAO.ExecReader(cmd);

                if (CDAO.DataReader.Read())
                {
                    IsAlarmCleared = CDAO.DataReader["IsAlarmCleared"].ToBoolean();
                    ClearedTime = CDAO.DataReader["ClearedTime"].ToDateTime();
                    IsAlarmClearedByUser = CDAO.DataReader["AlarmClearedByUser"].ToBoolean();
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

