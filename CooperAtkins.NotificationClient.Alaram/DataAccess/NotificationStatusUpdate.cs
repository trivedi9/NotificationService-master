/*
 *  File Name : NotificationStatusUpdate.cs
 *  Author : Vasu Ravuri 
 *           @ PCC Technology Group LLC
 *  Created Date : 12/16/2010
 *  Description: To update notification status once the process was completed.
 */
namespace CooperAtkins.NotificationClient.Alarm.DataAccess
{
    using System;
    using EnterpriseModel.Net;

    public class NotificationStatusUpdate : DomainCommandEx<NotificationStatusUpdate>
    {
        public int StoreID { get; set; }
        public int NotificationID { get; set; }
        public bool IsProcessCompleted { get; set; }

        protected override NotificationStatusUpdate ExecuteCommand()
        {
            /*Initialize the command.*/
            CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_UPDATENOTIFICATIONSTATUS, System.Data.CommandType.StoredProcedure);

            /*add parameters to command object*/
            cmd.AddWithValue("StoreID", StoreID);
            cmd.AddWithValue("NotificationID", NotificationID);
            // This stored procedure was changed to an integer because on SQL 2000 the bit was being sent as a 'T' or an 'F' instead of a 1 or 0
            cmd.AddWithValue("IsProcessCompleted", Convert.ToInt32(IsProcessCompleted));

            /*execute command*/
            CDAO.ExecCommand(cmd);

            return this;
        }
    }
}
