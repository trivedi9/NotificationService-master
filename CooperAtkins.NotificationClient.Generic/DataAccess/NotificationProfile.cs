/*
 *  File Name : NotificationProfile.cs
 *  Author : Pradeep.I
 *  @ PCC Technology Group LLC
 *  Created Date : 11/26/2010
 *  Description: Get the NotifyID for the appropriate destination object
 */


namespace CooperAtkins.NotificationClient.Generic.DataAccess
{
    using EnterpriseModel.Net;
    using CooperAtkins.Generic;
    using System.Collections;
    using System.Collections.Generic;

    public class NotificationProfile : DomainCommandEx<NotificationProfile>
    {
        public int NotifyProfileID { get; set; }
        public int NotifyType { get; set; }
        public int EmailNotifyID { get; set; }
        public int PagerNotifyID { get; set; }
        public int MsgBoardNotifyID { get; set; }
        public int NetSendNotifyID { get; set; }
        public int SwitchNotifyID { get; set; }
        public int SMSNotifyID { get; set; }
        public string PagerPrompt { get; set; }//pager message
        public int SwitchBitMask { get; set; }
        public int IVR_UserID { get; set; }
        public bool IsAlertEnabled { get; set; }
        public int StoreID { get; set; }
        public string ProfileName { get; set; }
        /// <summary>
        /// to fetch the notify id ,based on the notify profile id
        /// </summary>
        /// <returns></returns>
        protected override NotificationProfile ExecuteCommand()
        {
            try
            {
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_GETACTIVENOTIFICATIONS);
                cmd.AddWithValue("ProfileID", NotifyProfileID);
                cmd.AddWithValue("StoreID", StoreID);
                CDAO.ExecReader(cmd);

                if (CDAO.DataReader.Read())
                {
                    NotifyType = CDAO.DataReader["NotifyType"].ToInt();
                    EmailNotifyID = CDAO.DataReader["EmailNotifyID"].ToInt();
                    PagerNotifyID = CDAO.DataReader["PagerNotifyID"].ToInt();
                    MsgBoardNotifyID = CDAO.DataReader["MsgBoardNotifyID"].ToInt();
                    NetSendNotifyID = CDAO.DataReader["NetSendNotifyID"].ToInt();
                    SwitchNotifyID = CDAO.DataReader["SwitchNotifyID"].ToInt();
                    PagerPrompt = CDAO.DataReader["PagerPrompt"].ToStr();
                    SwitchBitMask = CDAO.DataReader["SwitchBitMask"].ToInt();
                    IVR_UserID = CDAO.DataReader["IVR_UserID"].ToInt();
                    IsAlertEnabled = CDAO.DataReader["IsAlertEnabled"].ToBoolean();
                    ProfileName = CDAO.DataReader["ProfileName"].ToStr();
                    SMSNotifyID = CDAO.DataReader.HasColumn("SMSNotifyID") ? CDAO.DataReader["SMSNotifyID"] != null ? CDAO.DataReader["SMSNotifyID"].ToInt() : 0 : 0;
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
