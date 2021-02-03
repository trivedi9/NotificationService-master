using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Configuration;
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Reflection;

namespace CooperAtkins.NotificationClient.Service
{
    public partial class CustomForm : Form
    {
        public CustomForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            ///targetdir="[TARGETDIR]\"
            //string targetDirectory = Context.Parameters["targetdir"];
            
            //string exePath = string.Format("{0}CooperAtkins.NotificationClient.Service.exe", Application.ExecutablePath);

            
            Configuration config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);


            //////Step 1
            //Driver={SQL Native Client};Database=IntelliWare;UID=sa;PWD=sasa;SERVER=(local);MultipleActiveResultSets=true;
            //Driver={SQL Server Native Client 10.0};Database=Intelliware;UID=sa;PWD=sasa;SERVER=(local);MultipleActiveResultSets=true;
            string connectionString = GetRegValue("DSN", CooperAtkins.NotificationClient.Generic.Common.TEMP_TRAK_REG_KEY).ToString();
            if (rdbNo.Checked)
            {
                connectionString = connectionString.Remove(0, connectionString.IndexOf(';') + 1);

                //connectionString = connectionString.Substring(0, connectionString.Length - "MultipleActiveResultSets=true;".Length);
                connectionString = connectionString.Replace("MultipleActiveResultSets=true;", "");

                config.ConnectionStrings.ConnectionStrings["SqlConnectionString"].ConnectionString = connectionString;
                
                string rootDirectory = Assembly.GetExecutingAssembly().Location.Remove(Assembly.GetExecutingAssembly().Location.LastIndexOf("\\"));
                
                config.AppSettings.Settings["ScriptPath"].Value = rootDirectory + "\\UE_Alert.vbs";
                
                config.Save();
            }
            else
            {
                connectionString = connectionString.Remove(0, connectionString.IndexOf(';') + 1);

                //connectionString = connectionString.Substring(0, connectionString.Length - "MultipleActiveResultSets=true;".Length);
                connectionString = connectionString.Replace("MultipleActiveResultSets=true;", "");

                string[] connStringArray = connectionString.Split(';');

                connectionString = "Database=Intelliware_OG;" + connStringArray[1] + ";" + connStringArray[2] + ";" + connStringArray[3]+ ";";

                config.ConnectionStrings.ConnectionStrings["SqlConnectionString"].ConnectionString = connectionString;

                string rootDirectory = Assembly.GetExecutingAssembly().Location.Remove(Assembly.GetExecutingAssembly().Location.LastIndexOf("\\"));

                config.AppSettings.Settings["ScriptPath"].Value = rootDirectory + "\\UE_Alert.vbs";
                
                config.Save();
            }
            this.Close();
        }

        public object GetRegValue(string regKeyName, string registryPath)
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

        private void CustomForm_Load(object sender, EventArgs e)
        {
            rdbNo.Focus();
        }

    }
}
