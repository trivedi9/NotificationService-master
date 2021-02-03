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


    public class NotifyConfiguration {
        private static NotifyConfiguration _notifyConfiguration = null;

        private NotifyConfigurationSection  _notifyConfigurationSection= null;
        private NotifyConfiguration() {
            _notifyConfigurationSection = (NotifyConfigurationSection)System.Configuration.ConfigurationManager.GetSection("notificationConfiguration");
        }
        public NotifyConfigurationSection Configuration {
            get {
                return _notifyConfigurationSection;
            }
        }
        public static NotifyConfiguration Instance {
            get {
                if (_notifyConfiguration == null)
                {
                    _notifyConfiguration = new NotifyConfiguration();
                }
                return _notifyConfiguration;
            }
        }
    }
    
    

    
}
