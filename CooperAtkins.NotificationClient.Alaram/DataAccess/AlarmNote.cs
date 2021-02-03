/*
 *  File Name : AlarmNote.cs
 *  Author : Vasu Ravuri 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/25/2010
 *  Description: To add note to the existing notifications.
 */

namespace CooperAtkins.NotificationClient.Alarm.DataAccess
{
    using System;
    using EnterpriseModel.Net;

    /// <summary>
    /// To add note to existing notification.
    /// </summary>
    public class AlarmNote : DomainCommandEx<AlarmNote>
    {
        public int StoreID { get; set; }
        public int AlarmID { get; set; }
        public string Message { get; set; }
        public string NoteType { get; set; }

        protected override AlarmNote ExecuteCommand()
        {
            /*Initialize the command.*/
            CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_ADDNOTE, System.Data.CommandType.StoredProcedure);

            /*add parameters to command object*/
            cmd.AddWithValue("AlarmID", AlarmID);
            cmd.AddWithValue("Message", Message);
            cmd.AddWithValue("NoteType", NoteType);
            cmd.AddWithValue("StoreID", StoreID);

            /*execute command*/
            CDAO.ExecCommand(cmd);

            return this;
        }
    }
}
