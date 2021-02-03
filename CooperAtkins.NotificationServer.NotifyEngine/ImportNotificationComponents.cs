/*
 *  File Name : ImportModule.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/19/2010
 *  
 */

namespace CooperAtkins.NotificationServer.NotifyEngine
{

    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.IO;
    using System.Reflection;
    using CooperAtkins.Interface.NotifyCom;
    using CooperAtkins.Generic;

    /// <summary>
    /// Import Notification Components
    /// </summary>
    public class ImportNotificationComponents
    {
        [ImportMany(typeof(INotifyCom))]
        private IEnumerable<INotifyCom> _notifyComs;

        public void Import()
        {
            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();
            //Adds all the parts found in all assemblies in current directory
            catalog.Catalogs.Add(
             new SafeDirectoryCatalog(
              Path.GetDirectoryName(
               Assembly.GetExecutingAssembly().Location)));

            //Create the CompositionContainer with the parts in the catalog
            CompositionContainer container = new CompositionContainer(catalog);

            //Fill the imports of this object
            container.ComposeParts(this);
        }

        /// <summary>
        /// Invoke notification components
        /// </summary>
        /// <param name="alarmObject"></param>
        /// <returns></returns>
        public NotifyComResponse Run(INotifyObject notifyObject)
        {
            string comFullName = WhoAmI(notifyObject.NotificationType);
            NotifyComResponse response = null;
            foreach (INotifyCom nCom in _notifyComs)
            {
                if (nCom.GetType().FullName == comFullName.Trim())
                {
                    response = nCom.Invoke(notifyObject);
                    break;
                }
            }
            return response;
        }
        public void UnLoad()
        {
            foreach (INotifyCom nCom in _notifyComs)
            {
                try
                {
                    nCom.UnLoad();
                }
                catch { }
            }
        }
        private string WhoAmI(string name)
        {
            string comFullName = "";

            if (Interface.NotifyCom.NotifyConfiguration.Instance.Configuration == null)
                throw new Exception("Enable notificationConfiguration section in app.config.");

            foreach (ComponentElement element in Interface.NotifyCom.NotifyConfiguration.Instance.Configuration.Components)
            {
                if (element.Name.ToLower() == name.ToLower())
                {
                    comFullName = element.Type;
                    break;
                }
            }
            return comFullName;
        }

    }
}

