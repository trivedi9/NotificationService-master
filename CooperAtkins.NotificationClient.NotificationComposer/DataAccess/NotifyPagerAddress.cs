/*
 *  File Name : NotifyEmailAddress.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/23/2010
 */
namespace CooperAtkins.NotificationClient.NotificationComposer.DataAccess
{
    using EnterpriseModel.Net;

    internal class NotifyPagerAddress : DomainEntity
    {
        public string PagerName { get; set; }
        public string PhoneNumber { get; set; }
        public int PagerDelay { get; set; }
        public int DeliveryMethod { get; set; }
        public string PagerMessage { get; set; }
    }
}
