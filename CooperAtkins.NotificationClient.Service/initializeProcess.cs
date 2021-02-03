/*
 *  File Name : initializeProcess.cs
 *  Author : Vasu Ravuri 
 *           @ PCC Technology Group LLC
 *  Created Date : 03/15/2010
 */

namespace CooperAtkins.NotificationClient.Service
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.ServiceProcess;
    using Microsoft.Win32;
    using CooperAtkins.NotificationClient.Generic.DataAccess;
    using CooperAtkins.Generic;
    using System.Windows.Forms;
    using System.Reflection;


    internal class initializeProcess
    {
        static NotificationClientService _clientService = null;
        static string DBName = null;
        static int oUID = 0;
        public static string firstDBName = null;
        static int firstOUID = 0;
        static string[] _args;
        public static bool isDBConnected = false;
        public static string DefaultConnectionString = null;

        internal static void InitProcess(string[] _args)
        {
            ConfigurationManager.RefreshSection("connectionStrings");

            bool flag = false;

            //Find whether the process is service or new instance of service initialize
            if (_args != null && _args.Length == 0)
            {
                string connectionString = "";
                flag = CheckIsMultiDB();
                if (flag)
                {
                    connectionString = ServiceMain();
                }
                else
                {
                    DefaultConnectionString = ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString;
                }
            }
        }




        /// <summary>
        /// Returns first connection string
        /// </summary>
        /// <returns></returns>
        private static string ServiceMain()
        {
            List<ServiceBase> servicebaseList = new List<ServiceBase>();
            ConfigurationManager.RefreshSection("connectionStrings");
            EnterpriseModel.Net.LibConfig.Instance.Config = new CooperAtkins.NotificationClient.Generic.DataAccess.EnterpriseModelConfig(ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString);
            OrganizationalUnitList orgUnitList = new OrganizationalUnitList();
            ServiceEntity svcEntity = new ServiceEntity();
            orgUnitList.Load(null);
            string firstConnectionString = "";

            for (int i = 0; i < orgUnitList.Count; i++)
            {
                if (orgUnitList[i].isActive)
                {
                    string connString = GetConnectionString(orgUnitList[i].DSN);
                    DBName = orgUnitList[i].DBName;
                    oUID = orgUnitList[i].OUID;
                    if (firstConnectionString == "")
                    {
                        DefaultConnectionString = connString;
                        firstConnectionString = connString;
                        firstDBName = orgUnitList[i].DBName;
                        firstOUID = orgUnitList[i].OUID;
                        continue;
                    }

                    Process p = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location, "\"" + connString + " \" " + DBName + " \" " + oUID.ToString() + "");
                    startInfo.CreateNoWindow = true;
                    p.StartInfo = startInfo;
                    p.Start();

                    svcEntity.OUID = orgUnitList[i].OUID;
                    svcEntity.PID = p.Id;
                    svcEntity.ExeName = p.ProcessName;
                    svcEntity.TimeStamp = DateTime.Now;

                    using (ServiceEntityContext context = new ServiceEntityContext())
                    {
                        context.Save(svcEntity);
                    }
                }
                else
                    continue;
            }

            return firstConnectionString;
        }
        /// <summary>
        /// In multi DB case read the connection string from the Organizational Units table
        /// and modify accordingly as per our requirement and return the string
        /// </summary>
        /// <param name="dsnConnString"></param>
        /// <returns></returns>
        private static string GetConnectionString(string dsnConnString)
        {
            //Driver={SQL Server Native Client 10.0};Database=Intelliware;UID=sa;PWD=sasa;SERVER=(local);MultipleActiveResultSets=true;
            dsnConnString = dsnConnString.Remove(0, dsnConnString.IndexOf(';') + 1);

            //dsnConnString = dsnConnString.Substring(0, dsnConnString.Length - "MultipleActiveResultSets=true;".Length);
            dsnConnString = dsnConnString.Replace("MultipleActiveResultSets=true;", "");

            return dsnConnString;
        }

        /// <summary>
        /// Initially when the service starts check whether we are running a muti db database or a single instance
        /// </summary>
        /// <returns></returns>
        private static bool CheckIsMultiDB()
        {

            EnterpriseModel.Net.LibConfig.Instance.Config = new CooperAtkins.NotificationClient.Generic.DataAccess.EnterpriseModelConfig(ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString);
            InstanceType instanceType = new InstanceType();
            instanceType = instanceType.Execute();
            return instanceType.IsMultiDB;
        }

        internal static void UpdateConfig()
        {
            string connectionString = GetConnectionString(GetRegValue("DSN", CooperAtkins.NotificationClient.Generic.Common.TEMP_TRAK_REG_KEY).ToString());

            string existingConnString = ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString;

            if (existingConnString.ToLower() != connectionString.ToLower())
            {
                string exePath = Assembly.GetExecutingAssembly().Location;

                Configuration config = ConfigurationManager.OpenExeConfiguration(exePath);

                config.ConnectionStrings.ConnectionStrings["SqlConnectionString"].ConnectionString = connectionString;

                config.Save();
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
    }
}
