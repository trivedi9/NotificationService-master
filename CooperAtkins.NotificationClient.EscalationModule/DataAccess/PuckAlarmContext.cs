/*
 *  File Name : PuckAlarmContext.cs
 *  Author : Pradeep.I
 *  @ PCC Technology Group LLC
 *  Created Date : 11/29/2010
 */

namespace CooperAtkins.NotificationClient.EscalationModule.DataAccess
{
    using System;
    using EnterpriseModel.Net;


    public class PuckAlarmContext : DomainContext<PuckAlarm, Criteria>
    {
        /// <summary>
        /// Method to insert alarm information.
        /// </summary>
        /// <param name="entityObject"></param>
        /// <returns></returns>
        protected override PuckAlarm Insert(PuckAlarm entityObject)
        {
            /* Initializing the command object. */
            CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_PUCKALARM);
            cmd.Parameters.AddWithValue("action", "C");

            /* Declaring AlarmID  as output parameter.*/
            CSqlDbParamter alarmIdParam = new CSqlDbParamter("AlarmID");
            alarmIdParam.Direction = System.Data.ParameterDirection.InputOutput;
            alarmIdParam.DbType = System.Data.DbType.Int32;
            alarmIdParam.Value = entityObject.AlarmID;

            cmd.Parameters.Add(alarmIdParam);

            /*Bind parameters to command object.*/
            BindParameters(cmd, entityObject);

            /* Executing the procedure.*/
            CDAO.ExecCommand(cmd);

            entityObject.AlarmID = Convert.ToInt32(CDAO.Parameters["AlarmID"].Value);
            return entityObject;

        }

        protected override PuckAlarm Load(Criteria criteria)
        {
            /* method was not implemented. */
            throw new NotImplementedException();
        }

        /// <summary>
        /// Method to delete alarm information.
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        protected override int Remove(Criteria criteria)
        {
            CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_PUCKALARM);
            cmd.Parameters.AddWithValue("action", "R");
            cmd.Parameters.AddWithValue("AlarmID", criteria.ID);
            int i = CDAO.ExecCommand(cmd);
            return i;
        }

        /// <summary>
        /// Method to update alarm information.
        /// </summary>
        /// <param name="entityObject"></param>
        /// <returns></returns>
        protected override PuckAlarm Update(PuckAlarm entityObject)
        {
            CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_PUCKALARM);
            cmd.Parameters.AddWithValue("action", "U");
            cmd.Parameters.AddWithValue("AlarmID", entityObject.AlarmID);
            cmd.Parameters.AddWithValue("AlarmActive", entityObject.AlarmActive);
            CDAO.ExecScalar(cmd);
            return entityObject;
        }

        /// <summary>
        /// Method to bind parameter values to entity object
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="entity"></param>
        private void BindParameters(CSqlDbCommand cmd, PuckAlarm entity)
        {
            cmd.Parameters.AddWithValue("UTID", entity.UTID);
            cmd.Parameters.AddWithValue("Probe", entity.Probe);
            cmd.Parameters.AddWithValue("ProbeName", entity.ProbeName);
            cmd.Parameters.AddWithValue("AlarmType", entity.AlarmType);
            cmd.Parameters.AddWithValue("AlarmTime", entity.AlarmTime);
            cmd.Parameters.AddWithValue("AlarmData", entity.AlarmData);
            cmd.Parameters.AddWithValue("AlarmActive", entity.AlarmActive);
            cmd.Parameters.AddWithValue("MinTemp", entity.MinTemp);
            cmd.Parameters.AddWithValue("MaxTemp", entity.MaxTemp);
            cmd.Parameters.AddWithValue("AlarmProfileRecID", entity.AlarmProfileRecID);
            cmd.Parameters.AddWithValue("NotificationID", entity.NotificationID);
        }
    }
}
