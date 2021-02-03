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
    /// Configuration section for the sensor alarm
    /// Configurable elements: Modules , Composers, notificationEndpoint, notificationTypes
    /// </summary>
    public class AlarmModuleConfiguration
    {
        private static AlarmModuleConfiguration _alarmConfiguration = null;

        private AlarmModuleConfigurationSection _alarmConfigurationSection = null;
        private AlarmModuleConfiguration()
        {
            _alarmConfigurationSection = (AlarmModuleConfigurationSection)System.Configuration.ConfigurationManager.GetSection("alarmConfiguration");
        }
        public AlarmModuleConfigurationSection Configuration
        {
            get
            {
                return _alarmConfigurationSection;
            }
        }
        public static AlarmModuleConfiguration Instance
        {
            get
            {
                if (_alarmConfiguration == null)
                {
                    _alarmConfiguration = new AlarmModuleConfiguration();
                }
                return _alarmConfiguration;
            }
        }
    }

}
