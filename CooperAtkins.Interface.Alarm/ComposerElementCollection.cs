/*
 *  File Name : AlarmConfiguration.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/22/2010
 *  
 */

namespace CooperAtkins.Interface.Alarm
{
    using System.Configuration;

    /// <summary>
    /// 
    /// </summary>
    public class ComposerElementCollection
      : ConfigurationElementCollection
    {

        public ComposerElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as ComposerElement;
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
            return new ComposerElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ComposerElement)element).Name;
        }
    }
}
