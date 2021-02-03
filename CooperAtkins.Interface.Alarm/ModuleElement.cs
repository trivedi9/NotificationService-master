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

    /// <summary>
    ///  Module configuration element within a configuration file.
    /// </summary>
    public class ModuleElement : ConfigurationElement
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

        [ConfigurationProperty("type")]
        public string ModuleType
        {
            get
            { return (string)this["type"]; }
            set
            { this["type"] = value; }
        }
    }
}
