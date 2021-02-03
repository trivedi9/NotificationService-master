/*
 *  File Name : NotificationProcessResume.cs
 *  Author : Vasu Ravuri 
 *           @ PCC Technology Group LLC
 *  Created Date : 12/16/2010
 *  Description: To update notification status once the process was completed.
 */
namespace CooperAtkins.NotificationClient.Alarm.DataAccess
{
    using EnterpriseModel.Net;

    public class NotificationProcessResume : DomainCommandEx<NotificationProcessResume>
    {
        public int StoreID { get; set; }

        protected override NotificationProcessResume ExecuteCommand()
        {
            CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_DORESUMEPROCESS, System.Data.CommandType.StoredProcedure);
            cmd.AddWithValue("StoreID", StoreID);

            CDAO.ExecCommand(cmd);

            return this;
        }
    }
}
