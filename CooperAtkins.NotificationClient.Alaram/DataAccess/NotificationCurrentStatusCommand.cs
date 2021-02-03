/*
 *  File Name : NotificationProcessResume.cs
 *  Author : Pradeep I
 *           @ PCC Technology Group LLC
 *  Created Date : 09/10/2013
 *  Description: To update notification current status once the process is completed.
 */

namespace CooperAtkins.NotificationClient.Alarm.DataAccess
{
    using EnterpriseModel.Net;

    public class NotificationCurrentStatusCommand : DomainCommandEx<NotificationCurrentStatusCommand>
    {
        public string UTID { get; set; }
        public int Probe { get; set; }
                
        protected override NotificationCurrentStatusCommand ExecuteCommand()
        {
            CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_ALARMCURRENTSTATUS);
            cmd.Parameters.AddWithValue("action", "U");
            cmd.Parameters.AddWithValue("UTID", this.UTID);
            cmd.Parameters.AddWithValue("Probe", this.Probe);
            CDAO.ExecScalar(cmd);
            return this;
        }
    }
}
