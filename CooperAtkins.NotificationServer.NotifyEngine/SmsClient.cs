using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using CooperAtkins.Generic;

namespace CooperAtkins.NotificationServer.NotifyEngine
{
    //# AT!band=00 - to change the frequency band to Automatic
    //# AT!band=04 - to change the frequency band to 2G 850/1900 
    //00 - Automatic
    //05 - 2G All
    //08 - 3G All 
    //at!band=00 and hit Enter --> To set the card to Auto frequency band
    //at!band=02  and hit Enter -->  To set the card to 3G 850/1900 frequency band
    //at!band=04  and hit Enter -->  To set the card to 2G 850/1900 frequency band
    //at!band=05  and hit Enter -->  To set the card to 2G All frequency band
    //at!band=08  and hit Enter -->  To set the card to 3G All frequency band 


    internal class SmsClient
    {
        public Interface.NotifyCom.INotifyObject notifyObject;
        int start = 1;
        SMSConfig config = new SMSConfig();
        SerialPort sp = null;
        public SmsClient()
        {

        }
        /// <summary>
        /// method to set the configuration for the sms notification
        /// </summary>
        /// <param name="notifyObject"></param>
        /// <returns></returns>
        private SerialPort SetConfiguration(Interface.NotifyCom.INotifyObject notifyObject)
        {
            try
            {

                config.Pin1 = notifyObject.NotifierSettings["PIN1"].ToString();
                config.Pin2 = notifyObject.NotifierSettings["PIN2"].ToString();
                config.ComPort = notifyObject.NotifierSettings["COMPort"].ToString(); //"COM1";
                config.ComSettings = notifyObject.NotifierSettings["COMSettings"].ToString(); //"115200,N,8,1";
                config.FrequencyBand = notifyObject.NotifierSettings["FrqBand"].ToString();
                config.ServiceCenterNumber = notifyObject.NotifierSettings["ServiceCenterNumber"].ToString();
                
                //if (config.IsPropertyChanged)
                //{
                    sp = SetPortConfig(notifyObject);
                    if (!sp.IsOpen)
                        sp.Open();
                    if (config.Pin1.Trim() != string.Empty)
                        sp.Write("AT+CPIN=" + config.Pin1.Trim() + Convert.ToString((char)13));

                    if (config.Pin2.Trim() != string.Empty)
                        sp.Write("AT+CPIN2=" + config.Pin2.Trim() + Convert.ToString((char)13));

                    if (config.ServiceCenterNumber.Trim() != string.Empty)
                        sp.Write("AT+CSCA=" + config.ServiceCenterNumber.Trim() + Convert.ToString((char)13));

                    if (config.FrequencyBand != string.Empty)
                        sp.Write("AT!BAND=" + config.FrequencyBand.Trim() + Convert.ToString((char)13));

                    config.IsPropertyChanged = false;
                //}



            }
            catch (Exception ex)
            {
                //LogBook.Write(ex, "Block 1 Error!!", ErrorSeverity.Normal);
                throw ex;

            }
            return sp;


        }
        /// <summary>
        /// method to send sms
        /// </summary>
        /// <param name="notifyObject"></param>
        /// <returns></returns>
        internal Interface.NotifyCom.NotifyComResponse Send(Interface.NotifyCom.INotifyObject notifyObject)
        {
            Interface.NotifyCom.NotifyComResponse response = new Interface.NotifyCom.NotifyComResponse();
            try
            {
                LogBook.Write("Executing Send Method");
                LogBook.Write("COM Port: " + notifyObject.NotifierSettings["COMPort"].ToString());
                LogBook.Write("Baud Rate: " + notifyObject.NotifierSettings["COMSettings"].ToString().Split(',')[0]);
                LogBook.Write("Data Bits: " + notifyObject.NotifierSettings["COMSettings"].ToString().Split(',')[2]);



                SetConfiguration(notifyObject);
                if (!sp.IsOpen)
                    sp.Open();
               
                //sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);

                sp.Write("AT" + Convert.ToString((char)13));

                Thread.Sleep(100);
                string msg = "AT+CMGS=\"" + notifyObject.NotifierSettings["ToPhoneNumber"].ToString() + "\"" + Convert.ToString(Environment.NewLine);
                //sp.Write("AT+CMGS=\"" + notifyObject.NotifierSettings["ToPhoneNumber"].ToString() + "\"" + Convert.ToString(Environment.NewLine));
                sp.Write(msg);

                //LogBook.Write("This is block 1: (Com Port: " + notifyObject.NotifierSettings["COMPort"].ToString() + ")");
                //LogBook.Write("This is block 1: (Number: " + notifyObject.NotifierSettings["ToPhoneNumber"].ToString() + ")");

                //AT+CMGF=1 	To format SMS as a TEXT message
                //AT+CSCA="+xxxxx"   	Set your SMS center's number. 
                //If the SIM detect pin indicates "absent", the response to AT+CPIN? is "+CME ERROR: 10"
                //(SIM not inserted). 

                Thread.Sleep(1000);
                
                sp.Write(notifyObject.NotificationData + Convert.ToString((char)26) + Environment.NewLine);
                
                Thread.Sleep(4000);
                // This is where we need to read from the serial port

                string returnCode = sp.ReadExisting();
                if (returnCode.Contains("ERROR") || returnCode.Contains("NO CARRIER") || !returnCode.Contains("+CMGS:"))
                {
                    response.IsError = true;
                    response.IsSucceeded = false;
                    response.ResponseContent = "SMS to [" + notifyObject.NotifierSettings["ToName"].ToStr() + "] " + notifyObject.NotifierSettings["ToPhoneNumber"].ToStr() + ", Failed";
                    notifyObject.NotifierSettings["AttemptCount"] = notifyObject.NotifierSettings["AttemptCount"].ToInt16() + 1;
                    LogBook.Write(String.Format("SMS to [{0}] {1}  Failed ReturnCode ={2}", notifyObject.NotifierSettings["ToName"].ToStr(), notifyObject.NotifierSettings["ToPhoneNumber"].ToStr(), returnCode), "CooperAtkins.NotificationServer.NotifyEngine.SmsClient");
                }
                else
                {
                    response.IsSucceeded = true;
                    response.IsError = false;
                    response.ResponseContent = "SMS to [" + notifyObject.NotifierSettings["ToName"].ToStr() + "] " + notifyObject.NotifierSettings["ToPhoneNumber"].ToStr() + ", sent successfully";
                    //LogBook.Write("SMS sent to: " + notifyObject.NotifierSettings["ToPhoneNumber"].ToStr());
                    LogBook.Write(String.Format("SMS to [{0}] {1}  sent successfully ReturnCode ={2}", notifyObject.NotifierSettings["ToName"].ToStr(), notifyObject.NotifierSettings["ToPhoneNumber"].ToStr(), returnCode));
                }
            }
            catch (Exception ex)
            {
                response.IsSucceeded = false;
                response.IsError = true;
                response.ResponseContent = "SMS to [" + notifyObject.NotifierSettings["ToName"].ToStr() + "] " + notifyObject.NotifierSettings["ToPhoneNumber"].ToStr() + ", Failed";
                notifyObject.NotifierSettings["AttemptCount"] = notifyObject.NotifierSettings["AttemptCount"].ToInt16() + 1;
                LogBook.Write(ex, "CooperAtkins.NotificationServer.NotifyEngine.SmsClient");
            }
            try
            {
                if (sp.IsOpen)
                {
                    sp.Close();
                }

            }
            catch (Exception ex)
            {
                LogBook.Write(String.Format("Error closing serial Port {0}", ex.Message));
            }
            return response;


        }
        /// <summary>
        /// method to set com settings for serial port
        /// </summary>
        /// <param name="notifyObject"></param>
        /// <returns></returns>
        private static SerialPort SetPortConfig(Interface.NotifyCom.INotifyObject notifyObject)
        {
            SerialPort sp = new SerialPort("COM" + notifyObject.NotifierSettings["COMPort"]);
            sp.BaudRate = Convert.ToInt32(notifyObject.NotifierSettings["COMSettings"].ToString().Split(',')[0]);
            sp.DataBits = Convert.ToInt32(notifyObject.NotifierSettings["COMSettings"].ToString().Split(',')[2]);
            sp.Parity = Parity.None;
            sp.StopBits = StopBits.One;
            sp.Handshake = Handshake.RequestToSend;
            //sp.DtrEnable = true;
            //sp.RtsEnable = true;
            return sp;
        }

        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (start == 1)
            {
                ((SerialPort)sender).Write(notifyObject.NotificationData + Convert.ToString((char)26));
            }
            start++;
        }
        /// <summary>
        /// method to close the serial port
        /// </summary>
        public void Close()
        {
            sp.Close();
            sp.Dispose();
        }
    }

    public class SMSConfig
    {
        string _pin1;
        string _pin2;
        string _serviceCenterNumber;
        string _frequencyBand;
        string _comPort;
        string _comSettings;
        bool _isPropertyChanged = false;

        public string Pin1
        {
            get { return _pin1; }
            set
            {
                if (value != _pin1)
                {
                    _pin1 = value;
                    _isPropertyChanged = true;
                }
            }
        }
        public string Pin2
        {
            get { return _pin2; }
            set
            {
                if (value != _pin2)
                {
                    _pin2 = value;
                    _isPropertyChanged = true;
                }
            }
        }
        public string ServiceCenterNumber
        {
            get { return _serviceCenterNumber; }
            set
            {
                if (value != _serviceCenterNumber)
                {
                    _serviceCenterNumber = value;
                    _isPropertyChanged = true;
                }

            }
        }
        public string FrequencyBand
        {
            get { return _frequencyBand; }
            set
            {
                if (value != _frequencyBand)
                {
                    _frequencyBand = value;
                    _isPropertyChanged = true;
                }

            }
        }
        public string ComSettings
        {
            get { return _comSettings; }
            set
            {
                if (value != _comSettings)
                {
                    _comSettings = value;
                    _isPropertyChanged = true;
                }
            }
        }
        public string ComPort
        {
            get { return _comPort; }
            set
            {
                if (value != _comPort)
                {
                    _comPort = value;
                    _isPropertyChanged = true;
                }
            }
        }
        public bool IsPropertyChanged
        {
            get { return _isPropertyChanged; }
            set { _isPropertyChanged = value; }
        }



    }
}
