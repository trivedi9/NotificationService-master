/*
 *  File Name : ModuleElementCollection.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/22/2010
 *  
 */


namespace CooperAtkins.Interface.Alarm
{
    using System.Configuration;

    /// <summary>
    /// Represent a Module configuration element containing a collection of child module elements
    /// </summary>
    public class ModuleElementCollection : ConfigurationElementCollection
    {

        public ModuleElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as ModuleElement;
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
            return new ModuleElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ModuleElement)element).Name;
        }
    }
}
