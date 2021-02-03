/*
 *  File Name : NotifyEmailAddress.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/23/2010
 */

namespace CooperAtkins.NotificationClient.NotificationComposer.DataAccess
{
    using EnterpriseModel.Net;
    using CooperAtkins.Generic;


    internal class NotifyPagerAddressList : DomainListBase<NotifyPagerAddressList, NotifyPagerAddress>
    {
        protected override NotifyPagerAddressList LoadList(BaseCriteria criteria)
        {
            try
            {
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_GETNOTIFYPAGERADDRESSLIST, System.Data.CommandType.StoredProcedure);
                cmd.AddWithValue("NotifyID", criteria.ID);

                //Execute command 
                CDAO.ExecReader(cmd);

                while (CDAO.DataReader.Read())
                {
                    NotifyPagerAddress notifyPagerAddress = new NotifyPagerAddress();
                    notifyPagerAddress.PagerName = CDAO.DataReader["Name"].ToStr();
                    notifyPagerAddress.PhoneNumber = CDAO.DataReader["PagerPhone"].ToStr();
                    notifyPagerAddress.PagerDelay = CDAO.DataReader["PagerDelay"].ToInt();
                    notifyPagerAddress.DeliveryMethod = CDAO.DataReader["PagerDeliveryMethod"].ToInt();
                    notifyPagerAddress.PagerMessage = CDAO.DataReader["PagerMessage"].ToStr();

                    this.Add(notifyPagerAddress);
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

