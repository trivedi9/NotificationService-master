/*
 *  File Name : NotifyConfiguration.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/22/2010
 *  
 */

namespace CooperAtkins.Interface.NotifyCom
{
    using System.Configuration;

    /// <summary>
    /// Class to define each attribute of notification configuration section
    /// </summary>
    public class NotifyConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("components")]
        public NotifyComElementCollection Components
        {
            get
            {
                return this["components"] as NotifyComElementCollection;
            }
        }

        [ConfigurationProperty("type")]
        public string Type
        {
            get
            {
                return this["type"] as string;
            }
            set
            {
                this["type"] = value;
            }
        }

        [ConfigurationProperty("endpointAddress")]
        public string EndpointAddress
        {
            get
            {
                return this["endpointAddress"] as string;
            }
            set
            {
                this["endpointAddress"] = value;
            }
        }

    }
}
