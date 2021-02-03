
/*
 *  File Name : EscalationInfo.cs
 *  Author : Vasu Ravuri 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/29/2010
 *  Description: To hold escalation information.
 */

namespace CooperAtkins.NotificationClient.EscalationModule.DataAccess
{
    using EnterpriseModel.Net;
    using CooperAtkins.Interface.Alarm;

    internal class EscalationInfo : DomainEntity
    {
        public int EscalationID { get; set; }
        public string ProfileName { get; set; }
        public int NotifyProfileID { get; set; }
        public int WaitSecs { get; set; }
        public short EscalationLevel { get; set; }
        public bool IsFailSafe { get; set; }
        public bool StopEscOnUserAck { get; set; }
        public bool StopEscOnSesnorNormalState { get; set; }
        public Severity Severity { get; set; }
        public short SwitchBitmask { get; set; }
        public string PagerPrompt { get; set; }
    }
}
