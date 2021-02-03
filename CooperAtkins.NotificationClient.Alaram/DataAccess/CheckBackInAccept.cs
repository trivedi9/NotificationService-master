using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnterpriseModel.Net;
using CooperAtkins.Interface.Alarm;
using CooperAtkins.Generic;

namespace CooperAtkins.NotificationClient.Alarm.DataAccess
{
    class CheckBackInAccept : DomainCommandEx<CheckBackInAccept>
    {
        public int NotificationID { get; set; }
        public int StoreID { get; set; }
        public bool IsBackInAcceptableRange { get; set; }
        public int LogCount { get; set; }

        protected override CheckBackInAccept ExecuteCommand()
        {
            try
            {
                /*Initialize the command.*/
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_CHECKBACKINACCEPT, System.Data.CommandType.StoredProcedure);

                /*add parameters to command object*/
                cmd.AddWithValue("NotificationID", NotificationID);
                cmd.AddWithValue("StoreID", StoreID);

                /*execute command*/
                CDAO.ExecReader(cmd);

                if (CDAO.DataReader.Read())
                {
                    IsBackInAcceptableRange = CDAO.DataReader["BackInRange"].ToBoolean();
                    LogCount = CDAO.DataReader["LogCount"].ToInt();
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
