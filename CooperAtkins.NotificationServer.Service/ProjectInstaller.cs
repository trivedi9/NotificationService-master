using System.ComponentModel;
using System.Configuration;
using System.ServiceProcess;
using System;
namespace CooperAtkins.NotificationServer.Service
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
     
            //ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller();
            //this.Installers.Add(serviceProcessInstaller);
            
        }

        //public override void Install(System.Collections.IDictionary stateSaver)
        //{
            
        //    base.Install(stateSaver);
            
        //    string targetDirectory = Context.Parameters["targetdir"];
        //    string storeID = Context.Parameters["EDITA1"];
        //    string ScriptPath = Context.Parameters["EDITA2"];
        //    string NetSendFromName = Context.Parameters["EDITA3"];
        //    string SqlConnString = Context.Parameters["EDITA4"];
        //    string NotifyConfigType = Context.Parameters["BUTTON2"];
        //    string serverIP = Context.Parameters["EDITC1"];
        //    string port = Context.Parameters["EDITC2"];

        //    string exePath = targetDirectory;
        //    Configuration config = ConfigurationManager.OpenExeConfiguration(exePath);

        //    ConfigurationSection section = config.GetSection("alarmConfig");

        //    string xml = section.SectionInformation.GetRawXml();
        //    xml.Remove(xml.IndexOf("remoteEndPoint"));


        //    ////Step 1
        //    config.AppSettings.Settings["StoreID"].Value = storeID;
        //    config.AppSettings.Settings["ScriptPath"].Value = ScriptPath;
        //    config.AppSettings.Settings["NetSendFromName"].Value = NetSendFromName;
        //    config.ConnectionStrings.ConnectionStrings["SqlConnectionString"].ConnectionString = SqlConnString;
        //    if (NotifyConfigType.ToLower() == "r")
        //    {
        //        //CooperAtkins.Interface.   alarmConfigurationSection = config.GetSection("alarmConfiguration");

        //        //alarmConfigurationSection.SectionInformation.
                
        //    }


        //    //Step 2
        //    //config.ConnectionStrings.ConnectionStrings["MCCI_DeltaDatabase"].ConnectionString = deltaDatabaseConnString;
        //    //config.ConnectionStrings.ConnectionStrings["MCCI_TransactionDatabase"].ConnectionString = transactionDatabaseConnString;
        //    //config.ConnectionStrings.ConnectionStrings["MCCI_MultivueDatabase"].ConnectionString = multivueDatabaseConnString;
        //    //config.AppSettings.Settings["ReportServerURL"].Value = reportServerURL;
        //    ////Step 3
        //    //config.AppSettings.Settings["TransactionFile_Path"].Value = transactionFilePath;
        //    //config.AppSettings.Settings["ArchiveFolder_Path"].Value = archiveFolderPath;
        //    //config.AppSettings.Settings["ErrorFolder_Path"].Value = errorFolderPath;
        //    //config.AppSettings.Settings["Suspended_Flag"].Value = suspendedFlag;
        //    //config.Save();

        //}

    }

}
