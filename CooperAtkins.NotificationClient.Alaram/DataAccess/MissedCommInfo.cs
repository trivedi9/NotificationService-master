/*
 *  File Name : DBCommands.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 12/28/2010
 */


namespace CooperAtkins.NotificationClient.Alarm.DataAccess
{
    using EnterpriseModel.Net;
    using System.Configuration;
    using CooperAtkins.Generic;

    public class MissedCommInfo : DomainCommandEx<MissedCommInfo>
    {
        public int StoreID { get; set; }
        public int NotificationProfileID { get; set; }
        public short SwitchBitmask { get; set; }
        public string PagerPrompt { get; set; }

        protected override MissedCommInfo ExecuteCommand()
        {
            try
            {
                //Initialize the CSqlDbCommand for execute the stored procedure
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_GETMISSEDCOMMSETTINGS);


                int storeID = ConfigurationManager.AppSettings["StoreID"].ToInt();

                cmd.AddWithValue("StoreID", storeID);

                //Execute command 
                CDAO.ExecReader(cmd);

                //Create new object to assign retrieved values.
                if (CDAO.DataReader.Read())
                {
                    this.StoreID = CDAO.DataReader["StoreID"].ToInt();
                    this.NotificationProfileID = CDAO.DataReader["NotificationProfileID"].ToInt();
                    this.SwitchBitmask = CDAO.DataReader["SwitchBitmask"].ToInt16();
                    this.PagerPrompt = CDAO.DataReader["PagerPrompt"].ToStr();
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

            //return filled object.
            return this;
        }
    }
}
