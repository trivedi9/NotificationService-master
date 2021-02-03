/*
 *  File Name : NotificationEndPointElement.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/22/2010
 *  
 */

namespace CooperAtkins.Interface.Alarm
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Configuration;


    public class NotificationEndPointElement : ConfigurationElement
    {
        [ConfigurationProperty("remoteEndpoint")]
        public string EndpointAddress
        {
            get
            { return (string)this["remoteEndpoint"]; }
            set
            { this["remoteEndpoint"] = value; }
        }

        /// <summary>
        /// Protocol wrapper type
        /// </summary>
        [ConfigurationProperty("type")]
        public string WType
        {
            get
            { return (string)this["type"]; }
            set
            { this["type"] = value; }
        }
        /// <summary>
        /// Protocol wrapper type
        /// </summary>
        [DefaultSettingValue("120000")]
        [ConfigurationProperty("timeout")]
        public int Timeout
        {
            get
            { return (int)this["timeout"]; }
            set
            { this["timeout"] = value; }
        }

    }

}
