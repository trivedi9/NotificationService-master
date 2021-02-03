/*
 *  File Name : SensorFormatString.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/23/2010
 *  Description: Get the format string ID
 */

namespace CooperAtkins.NotificationClient.Generic.DataAccess
{
    using EnterpriseModel.Net;
    using CooperAtkins.Generic;

    public class SensorFormatString : DomainCommandEx<SensorFormatString>
    {
        public string UTID { get; set; }
        public int Probe { get; set; }
        public string FormatType { get; set; }
        public int Action { get; set; }
        public int SensorFormatID { get; set; }

        /// <summary>
        /// Get Sensor Format ID 
        /// </summary>
        /// <returns></returns>
        protected override SensorFormatString ExecuteCommand()
        {
            try
            {
                CSqlDbCommand cmd = new CSqlDbCommand("USP_NS_GETSENSORFORMATSTRINGID");
                cmd.AddWithValue("UTID", UTID);
                cmd.AddWithValue("Probe", Probe);
                cmd.AddWithValue("FormatType", FormatType);
                cmd.AddWithValue("Action", Action);
                CDAO.ExecReader(cmd);
                if (CDAO.DataReader.Read())
                {
                    SensorFormatID = CDAO.DataReader["FormatID"].ToInt();
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
