/*
 *  File Name : GenStoreInfo.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/23/2010
 *  Change Log: added fields for Mobile SMS Dec 29 2010 By Pradeep.I
 */

namespace CooperAtkins.NotificationClient.Generic.DataAccess
{
    using System;
    using CooperAtkins.Generic;
    using EnterpriseModel.Net;
    using System.Configuration;

    public class GenStoreInfo : DomainCommandEx<GenStoreInfo>
    {


        public static GenStoreInfo _GenStoreInfo = null;
        public static GenStoreInfo GetInstance()
        {
            if (_GenStoreInfo == null)
                _GenStoreInfo = new GenStoreInfo().Execute();

            return _GenStoreInfo;
        }

        public void UpdateGenStore()
        {
            UpdateGenStoreValues();
        }

        public int StoreID { get; set; }
        public string StoreName { get; set; }
        public string StorePhoneNumber { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpSendMethod { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpAuthUserName { get; set; }
        public string SmtpAuthPassword { get; set; }
        public string SmtpAuthDomain { get; set; }
        public int SmtpFlags { get; set; }
        public string SNPPServer { get; set; }
        public int SNPPPort { get; set; }
        public Int16 PagerComPort { get; set; }
        public string ComPortInitStr { get; set; }
        //for Mobile SMS added on dec 29 2010
        public string PIN1 { get; set; }
        public string PIN2 { get; set; }
        public string ToPhoneNumber { get; set; }
        public string MobileCOMPort { get; set; }
        public string MobileCOMSettings { get; set; }
        public string SMSProviderServiceCenter { get; set; }
        public string FrequencyBand { get; set; }
        //for Email authentication type added on dec 29 2010
        public string SMTPAuthMethod { get; set; }
        public string CDYNE_ACCOUNT { get; set; }

        protected override GenStoreInfo ExecuteCommand()
        {
            UpdateGenStoreValues();
            //return object.
            return _GenStoreInfo;
        }

        public void UpdateGenStoreValues()
        {
            GenStoreInfo genStoreInfo = null;

            try
            {
                //Initialize the CSqlDbCommand for execute the stored procedure
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_GENSTORES);

                int storeID = ConfigurationManager.AppSettings["StoreID"].ToInt();

                cmd.AddWithValue("StoreID", storeID);

                //Execute command 
                CDAO.ExecReader(cmd);


                //Create new object to assign retrieved values.
                if (CDAO.DataReader.Read())
                {
                    genStoreInfo = new GenStoreInfo
                    {
                        //JH - 10/22/15
                        //setting the StoreID value was causing issue on lines 54-59 of NotificationEligibility
                        //StoreID = CDAO.DataReader["StoreID"].ToInt(),
                        FromAddress = CDAO.DataReader["EmailFromAddress"].ToStr(),
                        FromName = CDAO.DataReader["EmailFromName"].ToStr(),
                        SmtpSendMethod = CDAO.DataReader["SMTPSendMethod"].ToInt(),
                        SmtpAuthDomain = CDAO.DataReader["SMTPAuthDomain"].ToStr(),
                        SmtpAuthPassword = CDAO.DataReader["SMTPAuthPass"].ToStr(),
                        SmtpAuthUserName = CDAO.DataReader["SMTPAuthUser"].ToStr(),
                        SmtpFlags = CDAO.DataReader["SMTPFlags"].ToInt(),
                        SmtpPort = CDAO.DataReader["SMTPport"].ToInt(),
                        SmtpServer = CDAO.DataReader["SMTPserver"].ToStr(),
                        SNPPPort = CDAO.DataReader["SNPPPort"].ToInt(),
                        SNPPServer = CDAO.DataReader["SNPPServer"].ToStr(),
                        PagerComPort = CDAO.DataReader["PagerCOMport"].ToInt16(),
                        ComPortInitStr = CDAO.DataReader["COMportInitString"].ToStr(),
                        SMTPAuthMethod = CDAO.DataReader["SMTPAuthMethod"].ToStr(),
                        //for Mobile SMS
                        PIN1 = CDAO.DataReader["SMSPIN1"].ToStr(),
                        PIN2 = CDAO.DataReader["SMSPIN2"].ToStr(),
                        MobileCOMPort = CDAO.DataReader["SMSCOMPort"].ToStr(),
                        MobileCOMSettings = CDAO.DataReader["SMSSettings"].ToStr(),
                        SMSProviderServiceCenter = CDAO.DataReader["SMSServiceContactNo"].ToStr(),
                        StoreName = CDAO.DataReader["StoreName"].ToStr(),
                        StorePhoneNumber = CDAO.DataReader["Telephone"].ToStr(),
                        FrequencyBand = CDAO.DataReader.HasColumn("FrequencyBand") ? CDAO.DataReader["FrequencyBand"] != null ? CDAO.DataReader["FrequencyBand"].ToStr() : string.Empty : string.Empty,
                        CDYNE_ACCOUNT = CDAO.DataReader.HasColumn("IVRSvcAcctID") ? CDAO.DataReader["IVRSvcAcctID"] != null ? CDAO.DataReader["IVRSvcAcctID"].ToStr() : string.Empty : string.Empty

                    };
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                CDAO.CloseDataReader();
                CDAO.Dispose();
            }
            _GenStoreInfo = genStoreInfo;

        }
    }
}
