/*
* File : AlarmStatus.cs
* Author : Vasu
* Created Date :04/20/2011
* File Version : 1 .0
*/
namespace CooperAtkins.NotificationClient.Generic.DataAccess
{
    using System;
    using System.Collections.Generic;
    using EnterpriseModel.Net;
    using CooperAtkins.Generic;

    /// <summary>
    /// To know the current sensor status (value and time)
    /// </summary>
    public class SensorCurrentStatus : DomainCommandEx<SensorCurrentStatus>
    {

        public string UTID { get; set; }
        public int Probe { get; set; }        

        public decimal ? CurrentValue { get; set; }
        public DateTime CurrentTime { get; set; }


        protected override SensorCurrentStatus ExecuteCommand()
        {
            try
            {
                //Initialize the CSqlDbCommand for execute the stored procedure
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_GETCURRENTVALUE);

                //Bind  property values as input parameters for procedure
                cmd.AddWithValue("UTID", UTID);
                cmd.AddWithValue("Probe", Probe);


                //Execute command 
                CDAO.ExecReader(cmd);

                //Bind output values to IvrAlarmStatus object

                if (CDAO.DataReader.Read())
                {
                    CurrentTime = TypeCommonExtensions.IfNull(CDAO.DataReader["CurrentTime"], DateTime.UtcNow).ToDateTime();
                    CurrentValue = CDAO.DataReader["CurrentValue"].ToDecimal();
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