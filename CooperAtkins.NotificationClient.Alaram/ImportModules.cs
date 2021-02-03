/*
 *  File Name : ImportModule.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/19/2010
 *  
 */
namespace CooperAtkins.NotificationClient.Alarm
{
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.IO;
    using System.Reflection;
    using CooperAtkins.Interface.Alarm;
    using CooperAtkins.Generic;

    /// <summary>
    /// Import Alarm Process Modules
    /// </summary>
    public class ImportModules
    {
        [ImportMany(typeof(IModule) )]
        private IEnumerable<IModule> _processModules;

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

            AlarmModuleConfigurationSection configSection = AlarmModuleConfiguration.Instance.Configuration;

            List<IModule> orderingModules = new List<IModule>();
            
                foreach(ModuleElement element in configSection.Modules){
                    foreach (IModule module in _processModules)
                    {
                        if (module.GetType().FullName == element.ModuleType)
                        {
                            orderingModules.Add(module);
                            break;
                    }
                }
            }

            _processModules = orderingModules;
            LogBook.Write("Found and added modules from Config File");
             
        }

        /// <summary>
        /// Invoke process module(s)
        /// if any one of the module retuns false then alarm process
        /// will not send the information to notification engine.
        /// </summary>
        /// <param name="alarmObject"></param>
        /// <returns></returns>
        public bool Run(AlarmObject alarmObject,INotificationClient client)
        {
            bool flag = true;

            foreach (IModule module in _processModules)
            {
                //module.GetType().GetCustomAttributes(false)
                if (!module.Invoke(alarmObject, client))
                {
                    flag = false;
                }
            }
            return flag;
        }
    }


   

}
