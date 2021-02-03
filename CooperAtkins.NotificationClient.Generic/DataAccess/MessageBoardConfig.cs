/*
 *  File Name : MsgBrdBetaBrite.cs
 *  Author : Pradeep
 *  @ PCC Technology Group LLC
 *  Created Date : 11/25/2010
 */

namespace CooperAtkins.NotificationClient.Generic.DataAccess
{
    /// <summary>
    /// Class to store the message board configuration
    /// </summary>
    public class MessageBoardConfig 
    {
        public int NotifyID { get; set; }
        public string MessageBoardName { get; set; }
        public bool IsNetworkConnected { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public string COMSettings { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsGroup { get; set; }
        public int BoardType { get; set; }
    }
}
