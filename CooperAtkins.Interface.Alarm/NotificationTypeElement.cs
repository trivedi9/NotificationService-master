/*
 *  File Name : NotificationTypeElement.cs
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

    /// <summary>
    /// NotificationTypeElement within a configuration file.
    /// </summary>
    public class NotificationTypeElement : ConfigurationElement
    {
        [ConfigurationProperty("name")]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("value")]
        public string Value
        {
            get
            { return (string)this["value"]; }
            set
            {
                this["value"] = value;
            }
        }

        [ConfigurationProperty("isDynamicType")]
        public string IsDynamicType
        {
            get
            { return (string)this["isDynamicType"]; }
            set
            { this["isDynamicType"] = value; }
        }
    }

}
