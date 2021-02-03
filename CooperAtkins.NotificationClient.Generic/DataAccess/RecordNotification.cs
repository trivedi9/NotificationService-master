/*
 *  File Name : RecordNotification.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/25/2010
 */

namespace CooperAtkins.NotificationClient.Generic.DataAccess
{
    using EnterpriseModel.Net;
    using CooperAtkins.Interface.Alarm;

    public class RecordNotification : DomainCommandEx<RecordNotification>
    {

        public int NotificationID { get; set; }
        public int TransID { get; set; }
        public NotifyStatus Status { get; set; }
        public string LogText { get; set; }
        public NotifyTypes NotifyType { get; set; }

        /// <summary>
        /// Record Notification
        /// </summary>
        /// <returns></returns>
        protected override RecordNotification ExecuteCommand()
        {
            //Initialize the CSqlDbCommand for execute the stored procedure
            CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_RECORDNOTIFICATION,System.Data.CommandType.StoredProcedure);

            cmd.AddWithValue("NotificationID", NotificationID);
            cmd.AddWithValue("TransID", TransID);
            cmd.AddWithValue("Status", (int)Status);
            cmd.AddWithValue("LogText", LogText);
            cmd.AddWithValue("NotifyType", (int)this.NotifyType);

            //Execute command 
            CDAO.ExecCommand(cmd);

            //Close the data reader.
            CDAO.CloseDataReader();

            CDAO.Dispose();
            //return filled object.
            return this;
        }
    }
}
