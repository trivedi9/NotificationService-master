/*
 *  File Name : AlarmModuleConfigurationSection.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/22/2010
 *  
 */
namespace CooperAtkins.Interface.Alarm
{
    using System.Configuration;

    public class AlarmModuleConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("modules")]
        public ModuleElementCollection Modules
        {
            get
            {
                return this["modules"] as ModuleElementCollection;
            }
        }
        [ConfigurationProperty("voiceCustomIzations")]
        public VoiceCustomElementCollection VoiceCustoms
        {
            get
            {
                return this["voiceCustomIzations"] as VoiceCustomElementCollection;
            }
        }

        [ConfigurationProperty("composers")]
        public ComposerElementCollection Composers
        {
            get
            {
                return this["composers"] as ComposerElementCollection;
            }
        }

        [ConfigurationProperty("notificationEndpoint")]
        public NotificationEndPointElement EndPoint
        {
            get
            {
                return this["notificationEndpoint"] as NotificationEndPointElement;
            }
        }

        [ConfigurationProperty("notificationTypes")]
        public NotificationTypeElementCollection NotificationType
        {
            get
            {
                return this["notificationTypes"] as NotificationTypeElementCollection;
            }
        }


    }
}
