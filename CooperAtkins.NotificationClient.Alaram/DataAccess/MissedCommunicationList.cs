/*
 *  File Name : MissedCommunicationList.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 12/24/2010
 */

namespace CooperAtkins.NotificationClient.Alarm.DataAccess
{
    using System;
    using CooperAtkins.Generic;
    using EnterpriseModel.Net;


    public class MissedCommunicationList : DomainListBase<MissedCommunicationList, MissedComm>
    {
        
        protected override MissedCommunicationList LoadList(BaseCriteria criteria)
        {
            try
            {
                //Initialize the CSqlDbCommand for execute the stored procedure
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_GETMISSEDCOMMUNICATION);
                //Execute command 
                CDAO.ExecReader(cmd);

                //Create new object to assign retrieved values.
                while (CDAO.DataReader.Read())
                {
                    MissedComm missedComm = new MissedComm();
                    missedComm.GroupName = CDAO.DataReader["GroupName"].ToStr();
                    missedComm.PuckName = CDAO.DataReader["PuckName"].ToStr();
                    missedComm.SensorType = CDAO.DataReader["SensorType"].ToStr();
                    missedComm.FactoryID = CDAO.DataReader["FactoryID"].ToStr();
                    missedComm.UTID = CDAO.DataReader["UTID"].ToStr();
                    missedComm.Probe = CDAO.DataReader["Probe"].ToInt();
                    missedComm.Interval = CDAO.DataReader["Interval"].ToInt();
                    missedComm.LogIntervalMins = CDAO.DataReader["LogIntervalMins"].ToInt();
                    missedComm.LastContact = CDAO.DataReader["LastContact"].ToDateTime();
                    this.Add(missedComm);
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
