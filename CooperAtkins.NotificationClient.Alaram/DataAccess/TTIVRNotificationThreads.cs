
/*
 *  File Name : IvrAlarmList.cs
 *  Author : Vasu Ravuri 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/24/2010
 *  Description: To get IVR alarm list.
 */
namespace CooperAtkins.NotificationClient.Alarm.DataAccess
{
    using System;
    using CooperAtkins.Interface.Alarm;
    using EnterpriseModel.Net;
    using CooperAtkins.Generic;
    using CooperAtkins.NotificationClient.Generic.DataAccess;

    public class TTIVRNotificationThreads : DomainListBase<TTIVRNotificationThreads, AlarmObject>
    {
        /// <summary>
        /// Gets all active IVR alarms.
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        protected override TTIVRNotificationThreads LoadList(BaseCriteria criteria)
        {
            try
            {
                Criteria listCriteria = (Criteria)criteria;

                //Initialize the CSqlDbCommand for execute the stored procedure
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_GETIVRNOTIFICATIONTHREADLIST, System.Data.CommandType.StoredProcedure);

                //Execute reader 
                CDAO.ExecReader(cmd);

                /*fill the object and add to list.*/
                while (CDAO.DataReader.Read())
                {

                    IvrAlarm alarm = new IvrAlarm();

                    alarm.ThreadID = CDAO.DataReader["ThreadID"].ToInt();
                 

                    this.Add(alarm);
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

