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
    /// class to define each attribute of the components section of the notification configuration
    /// </summary>
    public class ComponentElement : ConfigurationElement
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
        public string Type
        {
            get
            { return (string)this["type"]; }
            set
            { this["type"] = value; }
        }
    }
}
