/*
 *  File Name : IVRNotificationRemover.cs
 *  Author : Jeff Hardy
 *           @Cooper-Atkins
 *  Created Date : 11/16/2015
 *  Description: To remove notification from the ttIVRNotification table.
 */
namespace CooperAtkins.NotificationClient.Alarm.DataAccess
{
    using EnterpriseModel.Net;

    public class IVRNotificationRemover : DomainCommandEx<IVRNotificationRemover>
    {
        public int StoreID { get; set; }
        public int NotificationID { get; set; }

        protected override IVRNotificationRemover ExecuteCommand()
        {
            /*Initialize the command.*/
            CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_REMOVE_IVRNOTIFICATION, System.Data.CommandType.StoredProcedure);

            /*add parameters to command object*/
            cmd.AddWithValue("StoreID", StoreID);
            cmd.AddWithValue("NotificationID", NotificationID);

            /*execute command*/
            CDAO.ExecCommand(cmd);

            return this;
        }
    }
}
