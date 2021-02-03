/*
 *  File Name : NotificationRemover.cs
 *  Author : Vasu Ravuri 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/25/2010
 *  Description: To remove notification from the ttNotifications table.
 */
namespace CooperAtkins.NotificationClient.Alarm.DataAccess
{
    using EnterpriseModel.Net;

    public class NotificationRemover : DomainCommandEx<NotificationRemover>
    {
        public int StoreID { get; set; }
        public int NotificationID { get; set; }

        protected override NotificationRemover ExecuteCommand()
        {
            /*Initialize the command.*/
            CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_REMOVE_NOTIFICATION, System.Data.CommandType.StoredProcedure);

            /*add parameters to command object*/
            cmd.AddWithValue("StoreID", StoreID);
            cmd.AddWithValue("NotificationID", NotificationID);

            /*execute command*/
            CDAO.ExecCommand(cmd);

            return this;
        }
    }
}
