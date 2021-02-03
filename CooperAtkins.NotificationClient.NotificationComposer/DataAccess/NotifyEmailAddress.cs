
/*
 *  File Name : NotifyEmailAddress.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/23/2010
 *  */
namespace CooperAtkins.NotificationClient.NotificationComposer.DataAccess
{
    using System;
    using EnterpriseModel.Net;

    public class NotifyEmailAddress : DomainEntity
    {
        public int NotifyID { get; set; }
        public string EmailAddress { get; set; }
        public Int16 AlphaPager { get; set; }
        public string Name { get; set; }
        public bool isSms { get; set; }
    }
}
