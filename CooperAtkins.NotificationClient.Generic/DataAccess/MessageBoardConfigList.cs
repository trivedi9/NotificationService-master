/*
 *  File Name : MsgBrdBetaBrite.cs
 *  Author : Pradeep
 *  @ PCC Technology Group LLC
 *  Created Date : 11/25/2010
 */
namespace CooperAtkins.NotificationClient.Generic.DataAccess
{
    using EnterpriseModel.Net;
    using CooperAtkins.Generic;

    public class MessageBoardConfigList : DomainListBase<MessageBoardConfigList, MessageBoardConfig>
    {
        /// <summary>
        /// to fetch the message board configuration list
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        protected override MessageBoardConfigList LoadList(BaseCriteria criteria)
        {
            //CDAO.Connection = EnterpriseModel.Net.
            try
            {
                Criteria listCriteria = (Criteria)criteria;
                //Initialize the CSqlDbCommand for execute the stored procedure
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_GETNOTIFYMSGBOARDIDLIST);
                if (listCriteria.ID == null)
                    cmd.AddWithValue("MsgBoardNotifyID", System.DBNull.Value);
                else
                    cmd.AddWithValue("MsgBoardNotifyID", listCriteria.ID);

                //Execute reader 
                CDAO.ExecReader(cmd);
                while (CDAO.DataReader.Read())
                {
                    MessageBoardConfig messageBoardConfig = new MessageBoardConfig()
                    {
                        NotifyID = CDAO.DataReader["NotifyID"].ToInt(),
                        MessageBoardName = CDAO.DataReader["Name"].ToStr(),
                        IsNetworkConnected = CDAO.DataReader["NetworkConnection"].ToBoolean(),
                        IpAddress = CDAO.DataReader["IPAddress"].ToStr(),
                        Port = CDAO.DataReader["Port"].ToInt(),
                        COMSettings = CDAO.DataReader["Settings"].ToStr(),
                        IsEnabled = CDAO.DataReader["isEnabled"].ToBoolean(),
                        //IsGroup = CDAO.DataReader["IsGroup"].ToBoolean(),
                        BoardType = CDAO.DataReader["BoardType"].ToInt()
                    };

                    this.Add(messageBoardConfig);
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
