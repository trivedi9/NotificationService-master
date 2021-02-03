

namespace CooperAtkins.NotificationClient.Generic.DataAccess
{
    using EnterpriseModel.Net;
    using CooperAtkins.Generic;

    public class RelaySwitchConfigContext : DomainContext<RelaySwitchConfig,Criteria>
    {

        protected override RelaySwitchConfig Insert(RelaySwitchConfig entityObject)
        {
            throw new System.NotImplementedException();
        }

        protected override RelaySwitchConfig Load(Criteria criteria)
        {
            RelaySwitchConfig relaySwitchConfig = new RelaySwitchConfig();

            try
            {
                //Initialize the CSqlDbCommand for execute the stored procedure
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_GETNOTIFYSWITCH);
                cmd.Parameters.AddWithValue("SwitchNotifyID", criteria.ID);
                //Execute reader 
                CDAO.ExecReader(cmd);
                while (CDAO.DataReader.Read())
                {
                    relaySwitchConfig.NotifyID = CDAO.DataReader["NotifyID"].ToInt();
                    relaySwitchConfig.IsEnabled = CDAO.DataReader["isEnabled"].ToBoolean();
                    relaySwitchConfig.Name = CDAO.DataReader["Name"].ToStr();
                    relaySwitchConfig.SwitchInfo = CDAO.DataReader["SwitchInfo"].ToStr();

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

            return relaySwitchConfig;
        }

        protected override int Remove(Criteria criteria)
        {
            throw new System.NotImplementedException();
        }

        protected override RelaySwitchConfig Update(RelaySwitchConfig entityObject)
        {
            throw new System.NotImplementedException();
        }
    }
}
