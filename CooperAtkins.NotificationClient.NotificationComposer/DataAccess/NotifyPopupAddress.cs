/*
 *  File Name : NotificationPopupAddressList.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 12/13/2010
 */
namespace CooperAtkins.NotificationClient.NotificationComposer.DataAccess
{
    using EnterpriseModel.Net;

    public class NotifyPopupAddress : DomainEntity
    {
        public int NotifyID { get; set; }
        public string NetSendTo { get; set; }
        public string Name { get; set; }
    }
}
