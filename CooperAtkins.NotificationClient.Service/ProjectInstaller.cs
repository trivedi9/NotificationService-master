using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Win32;
using System.ServiceProcess;
using CooperAtkins.Generic;

namespace CooperAtkins.NotificationClient.Service
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            
            try
            {
                
                base.Install(stateSaver);
                //stop the old service
                
                StopService("Intelli-Ware Notification Service", 500);
               
                
                
                ///targetdir="[TARGETDIR]\"
                string targetDirectory = Context.Parameters["targetdir"];
                

                string exePath = string.Format("{0}CooperAtkins.NotificationClient.Service.exe", targetDirectory);
                

                Configuration config = ConfigurationManager.OpenExeConfiguration(exePath);

                //////Step 1
                //Driver={SQL Native Client};Database=IntelliWare;UID=sa;PWD=sasa;SERVER=(local);MultipleActiveResultSets=true;                
                string connectionString = GetRegValue("DSN", CooperAtkins.NotificationClient.Generic.Common.TEMP_TRAK_REG_KEY).ToString();
                

                connectionString = connectionString.Remove(0, connectionString.IndexOf(';') + 1);

                //connectionString = connectionString.Substring(0, connectionString.Length - "MultipleActiveResultSets=true;".Length);
                connectionString = connectionString.Replace("MultipleActiveResultSets=true;","");

                config.ConnectionStrings.ConnectionStrings["SqlConnectionString"].ConnectionString = connectionString;
                

                //string rootDirectory = Assembly.GetExecutingAssembly().Location.Remove(Assembly.GetExecutingAssembly().Location.LastIndexOf("\\"));
                string rootDirectory = targetDirectory.Replace(@"\\", @"\");
                

                config.AppSettings.Settings["ScriptPath"].Value = rootDirectory + "\\UE_Alert.vbs";
                config.Save();
                
            }
            catch(Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                
            }
        }
        
        /// <summary>
        /// Return Registry Key value based on key name
        /// </summary>
        /// <param name="regKeyName"></param>
        /// <returns></returns>
        public static object GetRegValue(string regKeyName, string registryPath)
        {
            object regKeyValue = null;
            RegistryKey regkey;
            regkey = Registry.LocalMachine.OpenSubKey(registryPath);
            try
            {
                if (regkey != null)
                    regKeyValue = regkey.GetValue(regKeyName);
            }
            catch (Exception ex) {  /*Write Log*/    }
            return regKeyValue;
        }

        public static void SetRegValue(string registryPath, string regKeyName, object regKeyValue, RegistryValueKind registryValueKind)
        {
            RegistryKey regkey;
            regkey = Registry.LocalMachine.OpenSubKey(registryPath, true);
            try
            {
                if (regkey != null)
                    regkey.SetValue(regKeyName, regKeyValue, registryValueKind);
            }
            catch (Exception ex) {  /*Write Log*/    }
        }

        public static void StopService(string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
                if (service.Status == ServiceControllerStatus.Running)
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                }
                
            }
            catch(Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }



    }
}
