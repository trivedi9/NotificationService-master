/*
 *  File Name : ComposerElement.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/22/2010
 *  
 */

namespace CooperAtkins.Interface.Alarm
{
    using System.Configuration;

    /// <summary>
    /// Composer configuration element within a configuration file.
    /// To configure the notification endpoint and composer object type.
    /// </summary>
    public class ComposerElement : ConfigurationElement
    {
        /// <summary>
        /// Set/Get name of the composer element. 
        /// It must be the same as name of notification engine component configuration name.
        /// </summary>
        [ConfigurationProperty("name", IsRequired=true)]
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
        /// Set/Get composer type full name. 
        /// </summary>
        [ConfigurationProperty("type", IsRequired=true)]
        public string ModuleType
        {
            get
            { return (string)this["type"]; }
            set
            { this["type"] = value; }
        }
  
        /// <summary>
        /// Get notification endpoint element
        /// </summary>
        [ConfigurationProperty("notificationEndpoint")]
        public NotificationEndPointElement EndPoint
        {
            get
            {
                return this["notificationEndpoint"] as NotificationEndPointElement;
            }
        }

        /// <summary>
        /// Set/Get InvokeAlways. If the property value is true, 
        /// notification client will invoke the object always whether it was configured in the database or not
        /// </summary>
        [ConfigurationProperty("invokeAlways")]
        public bool? InvokeAlways
        {
            get
            {
                if (this["invokeAlways"] == null)
                    return null;
                else
                    return (bool)this["invokeAlways"];
            }
            set
            {
                this["invokeAlways"] = value;
            }
        }



    }

}
