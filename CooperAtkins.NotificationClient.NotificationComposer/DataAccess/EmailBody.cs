
/*
 *  File Name : EmailBody.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/25/2010
 */
namespace CooperAtkins.NotificationClient.NotificationComposer.DataAccess
{
    using System;
    using EnterpriseModel.Net;
    using CooperAtkins.Generic;
    using CooperAtkins.Interface.Alarm;

    public class EmailBody : DomainCommandEx<EmailBody>
    {
        public Int16 EmailFormat { get; set; }
        public string UTID { get; set; }
        public int Probe { get; set; }
        public DateTime? LogTime { get; set; }
        public decimal TzOffset { get; set; }
        public decimal? SensorReading { get; set; }
        public DateTime? AlarmStartTime { get; set; }
        public int AlarmProfileRecID { get; set; }
        public int ThresholdMins { get; set; }
        public int CondThresholdMins { get; set; }
        public decimal CondMinValue { get; set; }
        public decimal CondMaxValue { get; set; }
        public Int16 IncludeHistory { get; set; }
        public int ValuesUOM { get; set; }
        public string HTMLlink { get; set; }
        public Severity Severity { get; set; }
        public string Body { get; set; }


        /// <summary>
        /// Gets Email Body
        /// </summary>
        /// <returns></returns>
        protected override EmailBody ExecuteCommand()
        {
            try
            {
                //Initialize the CSqlDbCommand for execute the stored procedure
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.SP_TTNOTIFY_EMAILBODY);

                cmd.AddWithValue("emailFormat", EmailFormat);
                cmd.AddWithValue("UTID", UTID);
                cmd.AddWithValue("Probe", Probe);
                //cmd.AddWithValue("logTime", LogTime);
                if (LogTime.HasValue == true)
                {
                    cmd.AddWithValue("logTime", ((DateTime)LogTime).ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else
                {
                    cmd.AddWithValue("logTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                cmd.AddWithValue("tzOffset", TzOffset);
                cmd.AddWithValue("SensorReading", SensorReading);
                if (AlarmStartTime.HasValue == true)
                {
                    cmd.AddWithValue("AlarmStartTime", ((DateTime)AlarmStartTime).ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else
                {
                    cmd.AddWithValue("AlarmStartTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }

                cmd.AddWithValue("AlarmProfileRecID", AlarmProfileRecID);
                cmd.AddWithValue("ThresholdMins", ThresholdMins);
                cmd.AddWithValue("CondThresholdMins", CondThresholdMins);
                cmd.AddWithValue("CondMinValue", CondMinValue);
                cmd.AddWithValue("CondMaxValue", CondMaxValue);
                cmd.AddWithValue("IncludeHistory", IncludeHistory);
                cmd.AddWithValue("ValuesUOM", ValuesUOM);
                cmd.AddWithValue("HTMLlink", HTMLlink);
                cmd.AddWithValue("Severity", (int)Severity);

                //Execute command 
                CDAO.ExecReader(cmd);

                //Create new object to assign retrieved values.
                while (CDAO.DataReader.Read())
                {
                    this.Body = this.Body + CDAO.DataReader[0].ToStr();
                    CDAO.DataReader.NextResult();
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
