/*
 *  File Name : NotifyClientEnd.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 12/20/2010
 *  
 */

namespace CooperAtkins.NotificationClient.NotificationComposer
{

    using System;
    using System.IO;
    using System.Reflection;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using CooperAtkins.Generic;
    using CooperAtkins.Interface.NotifyCom;

    internal class NotifyClientEnd
    {
        [ImportMany(typeof(INotificationChannelClient))]
        private IEnumerable<INotificationChannelClient> _notificationChannels = null;
        private Dictionary<string, INotificationChannelClient> _notificationChannelsWithName = new Dictionary<string, INotificationChannelClient>();


        public void Import()
        {
            try{

            LogBook.Write("NotificationClientEnd - Import Path: "+Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            if (_notificationChannels != null) return;
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

            foreach (INotificationChannelClient channel in _notificationChannels)
            {
                _notificationChannelsWithName.Add(channel.GetType().FullName, channel);
            }
            }
            catch (Exception Ex)
            {
                LogBook.Write(Ex, "Notification ClientEnd Exception");
            }
        }

        public INotificationChannelClient GetClient(string type, string endpointAddress)
        {
            if (type == null)
                throw new ArgumentNullException();
            INotificationChannelClient client = _notificationChannelsWithName[type];
            client.EndPointAddress = endpointAddress;
            return client;
        }
    }

}
