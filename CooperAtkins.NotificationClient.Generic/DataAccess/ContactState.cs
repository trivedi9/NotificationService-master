
/*
 *  File Name : ContactState.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/23/2010
 */

namespace CooperAtkins.NotificationClient.Generic.DataAccess
{
    using EnterpriseModel.Net;
    using CooperAtkins.Generic;

    public class ContactState : DomainCommandEx<ContactState>
    {
        public string UTID { get; set; }
        public int Probe { get; set; }
        public decimal contactState { get; set; }
        public int LanguageID { get; set; }
        public string contactStateString { get; set; }

        /// <summary>
        /// Get Contact state string value
        /// </summary>
        /// <returns></returns>
        protected override ContactState ExecuteCommand()
        {

            try
            {
                CSqlDbCommand cmd = new CSqlDbCommand("SP_TTGETCONTACTSENSORTEXT");
                cmd.AddWithValue("UTID", UTID);
                cmd.AddWithValue("Probe", Probe);
                cmd.AddWithValue("State", contactState);
                cmd.AddWithValue("LanguageID", LanguageID);

                CDAO.ExecReader(cmd);
                if (CDAO.DataReader.Read())
                {
                    contactStateString = CDAO.DataReader["StateText"].ToStr();
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
