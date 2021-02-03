/*
 *  File Name : EscalationList.cs
 *  Author : Vasu Ravuri 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/29/2010
 *  Description: Returns escalation list for the current alarm object.
 */

namespace CooperAtkins.NotificationClient.EscalationModule.DataAccess
{
    using EnterpriseModel.Net;
    using CooperAtkins.Generic;

    internal class EscalationList : DomainListBase<EscalationList, EscalationInfo>
    {

        protected override EscalationList LoadList(BaseCriteria criteria)
        {
            try
            {
                /* Initialize the command object. */
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_GETESCALATIONPROFILEINFO, System.Data.CommandType.StoredProcedure);
                cmd.Parameters.AddWithValue("EscalationProfileID", criteria.ID);

                /* Execute the command. */
                CDAO.ExecReader(cmd);

                /* To know current escalation level*/
                short _level = 1;
             //   LogBook.Write("Looking up Escaltion List using Profile ID: " + criteria.ID);
                while (CDAO.DataReader.Read())
                {
                    EscalationInfo escalationInfo = new EscalationInfo()
                    {
                        EscalationID = CDAO.DataReader["RecID"].ToInt(),
                        NotifyProfileID = CDAO.DataReader["NotificationProfileID"].ToInt(),
                        ProfileName = CDAO.DataReader["ProfileName"].ToStr(),
                        StopEscOnUserAck = (CDAO.DataReader["Flags"].ToInt() & 1).ToBoolean(),
                        StopEscOnSesnorNormalState = (CDAO.DataReader["Flags"].ToInt() & 2).ToBoolean(),
                        SwitchBitmask = CDAO.DataReader["SwitchBitmask"].ToInt16(),
                        WaitSecs = CDAO.DataReader["WaitSecs"].ToInt(),
                        IsFailSafe = (CDAO.DataReader["Ordering"].ToInt() == 6), //6 indicates the failsafe escalation. other are normal escalations.
                        PagerPrompt = CDAO.DataReader["PagerPrompt"].ToStr(),
                        Severity = (CooperAtkins.Interface.Alarm.Severity)CDAO.DataReader["Severity"],
                        EscalationLevel = _level
                    };

                    if (escalationInfo.StopEscOnUserAck == false && escalationInfo.StopEscOnSesnorNormalState == false)
                    {
                        LogBook.Write("Default using 'Stop Escalation on user Ack/Clear', since both user & auto are false: " + escalationInfo.ProfileName);
                        escalationInfo.StopEscOnUserAck = true;
                    }

                    this.Add(escalationInfo);
                    _level++;
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
