
/*
 *  File Name : NotifyMobileList.cs
 *  Author : Pradeep.I
 *  @ PCC Technology Group LLC
 *  Created Date : 12/29/2010
 */
namespace CooperAtkins.NotificationClient.NotificationComposer.DataAccess
{
    using EnterpriseModel.Net;

    internal class NotifyMobiles : DomainEntity
    {
        public int NotifyID { get; set; }
        public string MobileNumber { get; set; }
        public string Name { get; set; }
    }

}
