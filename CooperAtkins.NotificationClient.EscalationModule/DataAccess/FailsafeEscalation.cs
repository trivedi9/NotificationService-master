/*
 *  File Name : FailsafeEscalation.cs
 *  Author : Vasu Ravuri 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/30/2010
 *  Description: 	To check whether the sensor comes to the normal range or not.
 */

namespace CooperAtkins.NotificationClient.EscalationModule.DataAccess
{
    using System;
    using EnterpriseModel.Net;
    using CooperAtkins.Generic;

    internal class FailsafeEscalation : DomainCommandEx<FailsafeEscalation>
    {
        public int StoreID { get; set; }
        public int AlarmID { get; set; }
        public string UTID { get; set; }
        public int Probe { get; set; }
        public Int16 AlarmType { get; set; }

        public bool SensorIsInRange { get; set; }

        protected override FailsafeEscalation ExecuteCommand()
        {
            /* Initialize the command object*/
            CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_GETFAILSAFEINFO, System.Data.CommandType.StoredProcedure);

            cmd.AddWithValue("StoreID", StoreID);
            cmd.AddWithValue("AlarmID", AlarmID);
            cmd.AddWithValue("UTID", UTID);
            cmd.AddWithValue("AlarmType", AlarmType);
            cmd.AddWithValue("Probe", Probe);

            object count = CDAO.ExecScalar(cmd);

            /*If the procedure returns count more than zero, consider it as the sensor comes to the normal range.*/
            SensorIsInRange = count.ToInt() > 0;

            return this;
        }
    }
}
