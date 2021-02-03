/*
 *  File Name : SensorTypeInfo.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/23/2010
 */

namespace CooperAtkins.NotificationClient.Generic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using EnterpriseModel.Net;

    public class SensorTypeInfo : DomainEntity
    {
        public int ID { get; set; }
        public double Offset1 { get; set; }
        public double Offset2 { get; set; }
        public double Scale1 { get; set; }
        public double Scale2 { get; set; }
        public string UOM { get; set; }
        public int nDecimals { get; set; }
        public bool isTemp { get; set; }
        public string Description { get; set; }
        public string SensorType { get; set; }
        public string SubType { get; set; }
    }
}
