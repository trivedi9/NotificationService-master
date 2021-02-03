/*
 *  File Name : NotificationPopupAddressList.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 12/13/2010
 */
namespace CooperAtkins.NotificationClient.NotificationComposer.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using EnterpriseModel.Net;
    using CooperAtkins.Generic;

    public class NotifyPopupAddressList : DomainListBase<NotifyPopupAddressList, NotifyPopupAddress>
    {
        protected override NotifyPopupAddressList LoadList(BaseCriteria criteria)
        {
            try
            {
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_GETNOTIFYPOPUPADDRESSLIST, System.Data.CommandType.StoredProcedure);
                cmd.AddWithValue("NotifyID", criteria.ID);

                //Execute command 
                CDAO.ExecReader(cmd);

                while (CDAO.DataReader.Read())
                {
                    if (CDAO.DataReader["NetSendTo"].ToStr().Contains(";") == true)
                    {
                        string[] notifyPopupAddressEntries = CDAO.DataReader["NetSendTo"].ToStr().Split(';');
                        foreach (string notifyPopupAddressEntry in notifyPopupAddressEntries)
                        {
                            NotifyPopupAddress notifyPopupAddress = new NotifyPopupAddress();
                            notifyPopupAddress.NetSendTo = notifyPopupAddressEntry.Trim();
                            notifyPopupAddress.Name = CDAO.DataReader["Name"].ToStr().Trim();
                            this.Add(notifyPopupAddress);
                        }
                    }

                    else if (CDAO.DataReader["NetSendTo"].ToStr().Contains(",") == true)
                    {
                        string[] notifyPopupAddressEntries = CDAO.DataReader["NetSendTo"].ToStr().Split(',');
                        foreach (string notifyPopupAddressEntry in notifyPopupAddressEntries)
                        {
                            NotifyPopupAddress notifyPopupAddress = new NotifyPopupAddress();

                            notifyPopupAddress.NetSendTo = notifyPopupAddressEntry.Trim();
                            notifyPopupAddress.Name = CDAO.DataReader["Name"].ToStr().Trim();
                            this.Add(notifyPopupAddress);
                        }
                    }
                    else
                    {
                        NotifyPopupAddress notifyPopupAddress = new NotifyPopupAddress();
                        notifyPopupAddress.NetSendTo = CDAO.DataReader["NetSendTo"].ToStr().Trim();
                        notifyPopupAddress.Name = CDAO.DataReader["Name"].ToStr().Trim();
                        this.Add(notifyPopupAddress);
                    }
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
