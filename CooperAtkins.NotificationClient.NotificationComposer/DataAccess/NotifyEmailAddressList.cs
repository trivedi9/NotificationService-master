
/*
 *  File Name : NotifyEmailAddress.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/23/2010
 *  ChangeLog: add isSms field to know whether it is sms or email . Pradeep.I on 31/12/2010
 */

namespace CooperAtkins.NotificationClient.NotificationComposer.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using EnterpriseModel.Net;
    using CooperAtkins.Generic;

    public class NotifyEmailAddressList : DomainListBase<NotifyEmailAddressList, NotifyEmailAddress>
    {

        protected override NotifyEmailAddressList LoadList(BaseCriteria criteria)
        {
            try
            {
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_GETNOTIFYEMAILADDRESSLIST, System.Data.CommandType.StoredProcedure);
                cmd.AddWithValue("NotifyID", criteria.ID);

                //Execute command 
                CDAO.ExecReader(cmd);

                while (CDAO.DataReader.Read())
                {
                    /*NotifyEmailAddress notifyEmailAddress = new NotifyEmailAddress();
                    notifyEmailAddress.EmailAddress = CDAO.DataReader["EmailAddress"].ToStr();
                    notifyEmailAddress.AlphaPager = CDAO.DataReader["IsAlphaPager"].ToInt16();
                    notifyEmailAddress.Name = CDAO.DataReader["Name"].ToStr();
                    this.Add(notifyEmailAddress);*/

                    if (CDAO.DataReader["EmailAddress"].ToStr().Contains(";") == true)
                    {
                        string[] notifyEmailAddressEntries = CDAO.DataReader["EmailAddress"].ToStr().Split(';');
                        foreach (string notifyEmailAddressEntry in notifyEmailAddressEntries)
                        {
                            NotifyEmailAddress notifyEmailAddress = new NotifyEmailAddress();
                            notifyEmailAddress.EmailAddress = notifyEmailAddressEntry;
                            notifyEmailAddress.AlphaPager = CDAO.DataReader["IsAlphaPager"].ToInt16();
                            notifyEmailAddress.Name = CDAO.DataReader["Name"].ToStr();
                            this.Add(notifyEmailAddress);
                        }
                    }

                    else if (CDAO.DataReader["EmailAddress"].ToStr().Contains(",") == true)
                    {
                        string[] notifyEmailAddressEntries = CDAO.DataReader["EmailAddress"].ToStr().Split(',');
                        foreach (string notifyEmailAddressEntry in notifyEmailAddressEntries)
                        {
                            NotifyEmailAddress notifyEmailAddress = new NotifyEmailAddress();
                            notifyEmailAddress.EmailAddress = notifyEmailAddressEntry;
                            notifyEmailAddress.AlphaPager = CDAO.DataReader["IsAlphaPager"].ToInt16();
                            notifyEmailAddress.Name = CDAO.DataReader["Name"].ToStr();
                            this.Add(notifyEmailAddress);
                        }
                    }
                    else
                    {
                        NotifyEmailAddress notifyEmailAddress = new NotifyEmailAddress();
                        notifyEmailAddress.EmailAddress = CDAO.DataReader["EmailAddress"].ToStr();
                        notifyEmailAddress.AlphaPager = CDAO.DataReader["IsAlphaPager"].ToInt16();
                        notifyEmailAddress.Name = CDAO.DataReader["Name"].ToStr();
                        this.Add(notifyEmailAddress);
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
