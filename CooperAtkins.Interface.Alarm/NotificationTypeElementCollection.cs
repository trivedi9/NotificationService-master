namespace CooperAtkins.Interface.Alarm
{
    using System.Configuration;

    /// <summary>
    /// 
    /// </summary>
    public class NotificationTypeElementCollection
     : ConfigurationElementCollection
    {

        public NotificationTypeElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as NotificationTypeElement;
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
            return new NotificationTypeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NotificationTypeElement)element).Name;
        }
    }
}
