/*
 *  File Name : NotifyMobileList.cs
 *  Author : Pradeep.I
 *  @ PCC Technology Group LLC
 *  Created Date : 12/29/2010
 */

namespace CooperAtkins.NotificationClient.NotificationComposer.DataAccess
{
    using EnterpriseModel.Net;
    using CooperAtkins.Generic;

    internal class NotifyMobileList : DomainListBase<NotifyMobileList, NotifyMobiles>
    {

        protected override NotifyMobileList LoadList(BaseCriteria criteria)
        {
            try
            {
                /*Initialize the command.*/
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_GETNOTIFYMOBILELIST, System.Data.CommandType.StoredProcedure);
                cmd.AddWithValue("NotifyID", criteria.ID);

                //Execute command 
                CDAO.ExecReader(cmd);

                while (CDAO.DataReader.Read())
                {
                    NotifyMobiles notifyMobiles = new NotifyMobiles();
                    notifyMobiles.MobileNumber = CDAO.DataReader["MobileNumber"].ToStr();
                    notifyMobiles.Name = CDAO.DataReader["Name"].ToStr();
                    this.Add(notifyMobiles);
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