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
    /// Represent a collection of notification objects defined in the configuration file, used to define notification components
    /// </summary>
    public class NotifyComElementCollection
      : ConfigurationElementCollection
    {

        public ComponentElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as ComponentElement;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ComponentElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ComponentElement)element).Name;
        }
    }
}
