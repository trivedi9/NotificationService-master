/*
 *  File Name : NotificationTypes.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/24/2010
 */

namespace CooperAtkins.NotificationClient.NotificationComposer
{
    using System.Collections.Generic;
    using CooperAtkins.Interface.Alarm;
    using CooperAtkins.Generic;

    public class NotificationTypes
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public bool IsDynamicType { get; set; }

        /// <summary>
        /// Gets all Notification Types from the configuration file.
        /// </summary>
        /// <returns></returns>
        public List<NotificationTypes> GetNotificationTypes()
        {
            List<NotificationTypes> list = new List<NotificationTypes>();

            AlarmModuleConfigurationSection configSection = AlarmModuleConfiguration.Instance.Configuration;

            /* getting name, value and dynamic type flag from configuration file.  */
            foreach (NotificationTypeElement element in configSection.NotificationType)
            {
                list.Add(new NotificationTypes
                {
                    Name = element.Name,
                    Value = element.Value.ToInt(),
                    IsDynamicType = element.IsDynamicType.ToBoolean()
                });
            }

            return list;

        }
    }
}
