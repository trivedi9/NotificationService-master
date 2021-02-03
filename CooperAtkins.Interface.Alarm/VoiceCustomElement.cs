/*
 *  File Name : VoiceCustomElement.cs
 *  Author : J Hardy
 *           @ Cooper Atkins
 *  Created Date : 7/18/14
 *  
 */

namespace CooperAtkins.Interface.Alarm
{
    using System.Configuration;

    /// <summary>
    /// Composer configuration element within a configuration file.
    /// To configure the notification endpoint and composer object type.
    /// </summary>
    public class VoiceCustomElement : ConfigurationElement
    {
        /// <summary>
        /// Set/Get name of the vociecustom element 
        /// It must be the same as name of notification engine component configuration name.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true)]
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

        /// <summary>
        /// Set/Get value 
        /// </summary>
        [ConfigurationProperty("value", IsRequired = true)]
        public string Value
        {
            get
            { return (string)this["value"]; }
            set
            { this["value"] = value; }
        }
        





    }

}
