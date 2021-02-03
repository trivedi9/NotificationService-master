
/*
 *  File Name : FormatString.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/23/2010
 *  Description: To get the format string based on the format id
 */

namespace CooperAtkins.NotificationClient.Generic.DataAccess
{
    using EnterpriseModel.Net;
    using CooperAtkins.Generic;

    public class FormatString : DomainCommandEx<FormatString>
    {
        public int FormatID { get; set; }
        public int LanguageID { get; set; }
        public string SensorType { get; set; }
        public string ComposeString { get; set; }

        /// <summary>
        /// Get Format string
        /// </summary>
        /// <returns></returns>
        protected override FormatString ExecuteCommand()
        {
            try
            {
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_GETFORMATSTRING);
                cmd.AddWithValue("FormatID", FormatID);
                cmd.AddWithValue("LanguageID", LanguageID);
                cmd.AddWithValue("SensorType", SensorType);
                CDAO.ExecReader(cmd);
                if (CDAO.DataReader.Read())
                {
                    ComposeString = CDAO.DataReader["FormatString"].ToStr();
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
