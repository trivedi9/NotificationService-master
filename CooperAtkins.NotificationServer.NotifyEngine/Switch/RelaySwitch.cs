/*
 *  File Name : RelaySwitch.cs
 *  Author : Pradeep
 *  @ PCC Technology Group LLC
 *  Created Date : 11/30/2010
  */
namespace CooperAtkins.NotificationServer.NotifyEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using CooperAtkins.SocketManager;
    using CooperAtkins.Generic;


    public class RelaySwitch
    {
        private class SwitchInfo
        {
            public string SwitchUniqueId { get; set; }
            public int Bitmask { get; set; }
            public string SensorAlarmId { get; set; }

        }

        public string CurrentState { get; set; }
        public string NewState { get; set; }

        private int _ioAddress;
        private string _switchName;
        private bool _isEnabled;
        private bool _isNetworkAttached;
        private string _comPort;
        private int _ipPort;
        private IOPort.ComSettings _comSettings;
        private string _ipAddress;
        private string _LastError;
        private string _comBuf;
        private int _lastValue;
        private short _switchBitMask;
        public bool? _isDynamicNotificationCleared;
        private string _sensorAlarmId;
        private string _factoryId;
        public string SwitchUniqueId { get; set; }

        public int IoAddress
        {
            get { return _ioAddress; }
            set { _ioAddress = value; }
        }
        public string SwitchName
        {
            get { return _switchName; }
            set { _switchName = value; }
        }
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }
        public bool IsNetworkAttached
        {
            get { return _isNetworkAttached; }
            set { _isNetworkAttached = value; }
        }
        public string ComPort
        {
            get { return _comPort; }
            set { _comPort = value; }
        }
        public int IpPort
        {
            get { return _ipPort; }
            set { _ipPort = value; }
        }

        public IOPort.ComSettings ComSettings
        {
            get { return _comSettings; }
            set { _comSettings = value; }
        }
        public string IpAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; }
        }
        public string LastError
        {
            get { return _LastError; }
            set { _LastError = value; }
        }
        public string ComBuf
        {
            get { return _comBuf; }
            set { _comBuf = value; }
        }
        public int LastValue
        {
            get { return _lastValue; }
            set { _lastValue = value; }
        }
        public string SensorAlarmID
        {
            get { return _sensorAlarmId; }
            set { _sensorAlarmId = value; }

        }
        public string FactoryID
        {
            get { return _factoryId; }
            set { _factoryId = value; }
        }

        public string Description
        {
            get
            {
                if (_isNetworkAttached)
                    return _ipAddress + " : " + _ipPort.ToString();
                else
                {
                    if (_ioAddress > 0)
                        return "LPT 0x" + String.Format("{0:X}", _ioAddress);
                    else
                        return "COM " + _ipPort.ToString();
                }
            }

        }

        public int Port
        {
            get
            {
                if (_isNetworkAttached)
                    return _ipPort;
                else
                {
                    if (_ioAddress > 0)
                        return _ioAddress;
                    else
                        return 0;
                }
            }
        }
        public short SwitchBitMask
        {
            get { return _switchBitMask; }
            set { _switchBitMask = value; }

        }
        public bool? IsDynamicNotificationCleared
        {
            get { return _isDynamicNotificationCleared; }
            set { _isDynamicNotificationCleared = value; }
        }

        public string ResponseContent { get; set; }
        private static List<RelaySwitch> SwitchInfoList { get; set; }

        public RelaySwitch()
        {
            if (SwitchInfoList == null)
                SwitchInfoList = new List<RelaySwitch>();
        }



        public void SendData()
        {
            SetCurrentState(true);
            SetNewState();


            LogBook.Write(this.FactoryID + " " + this.SensorAlarmID + " " + "Sending data to switch, switch bit mask value: " + SwitchBitMask + " data value :" + _lastValue.ToString());
            //if the configuration setting has the "Reset Notification Condition if Sensor Returns To Normal Status:" as checked 
            //then IsDynamicNotificationCleared is set to true
            if (IsDynamicNotificationCleared != null && IsDynamicNotificationCleared == true)
            {
                LogBook.Write(this.FactoryID + " " + this.SensorAlarmID + " " + "Dynamic notifications have to be cleared, previous switch value: " + SwitchBitMask + " data value :" + _lastValue.ToString());

                _switchBitMask = 0;
                SetNewState();

                ResponseContent += "switch: [" + this.SwitchName + "] " + (this.IpAddress.ToStr().Trim() != string.Empty ? "IP:" : " ") + this.IpAddress + ", Current state " + CurrentState + ", New State:" + NewState + ".\r\n";

                LogBook.Write(this.FactoryID + " " + this.SensorAlarmID + " " + "Resetting switch value: current relay state:" + CurrentState + ", New State :" + NewState);
            }


            //construct the relay command
            string sBuf = "R" + String.Format("{0:x2}", _lastValue) + ((char)13).ToString();
            try
            {
                //create instances of TCP/IP and Serial port
                NetworkClient client = new NetworkClient();//for TCP
                IOPort comPort = new IOPort();//for COMM port
                //if the device is attached to network
                if (_isNetworkAttached)
                {
                    LogBook.Write(this.FactoryID + " " + this.SensorAlarmID + " " + "Sending data using TCP IPAddress :" + _ipAddress.ToString() + "Port: " + _ipPort.ToString());
                    //send data using TCP Client
                    client.TcpClient(_ipAddress, _ipPort, Encoding.ASCII.GetBytes(sBuf));
                    LogBook.Write(this.FactoryID + " " + this.SensorAlarmID + " " + "Data sent using TCP IPAddress :" + _ipAddress.ToString() + "Port: " + _ipPort.ToString());
                }
                else
                {

                    LogBook.Write(this.FactoryID + " " + this.SensorAlarmID + " " + "Sending data using UDP Port: " + _comPort.ToString());
                    //ping the com port
                    comPort.Handshake(_comPort.ToString(), _comSettings, DataRecieveFromCom);
                    LogBook.Write(this.FactoryID + " " + this.SensorAlarmID + " " + "Handshake complete at port: " + _comPort.ToString());
                    LogBook.Write(this.FactoryID + " " + this.SensorAlarmID + " " + "Sending data to port: " + _comPort.ToString());
                    //send data using serial port
                    comPort.SerialPortOutput(Encoding.ASCII.GetBytes(sBuf));
                    LogBook.Write(this.FactoryID + " " + this.SensorAlarmID + " " + "Data sent to port: " + _comPort.ToString());
                }
            }
            catch (Exception ex)
            {
                LogBook.Write(ex.Message);
                throw ex;
            }
        }

        internal void ClearData(string sensorAlarmID)
        {
            /*get all relay switches (current and previous) for this specific alarm object*/
            var switchList = from info in SwitchInfoList
                             where info.SensorAlarmID == sensorAlarmID
                             select info;

            if (switchList.Count() > 0)
            {
                foreach (RelaySwitch relaySwitch in switchList.ToList<RelaySwitch>())
                {
                    //relaySwitch.SwitchBitMask = 0;
                    SetRelaySwitchInfo(relaySwitch);
                    SendData();
                }
            }
        }



        private void SetNewState()
        {
            /*check whether the object already exists in the list or not*/
            var switchList = from info in SwitchInfoList
                             where info.SensorAlarmID == _sensorAlarmId && info.SwitchUniqueId == (_isNetworkAttached ? _ipAddress : _comPort)
                             //where info.SwitchUniqueId == (_isNetworkAttached ? _ipAddress : _comPort)
                             select info;




            /*if it exists, only one object will be returned for this specific sensor alarm and overwrite the existing value*/
            if (switchList.Count() > 0)
            {
                switchList.ToList<RelaySwitch>()[0]._lastValue = _lastValue;
                switchList.ToList<RelaySwitch>()[0]._switchBitMask = _switchBitMask;
                if ((bool)_isDynamicNotificationCleared)
                    switchList.ToList<RelaySwitch>()[0]._switchBitMask = 0;
            }
            else
            {
                this.SwitchUniqueId = (_isNetworkAttached ? _ipAddress : _comPort);
                SwitchInfoList.Add(this);
            }

            SetCurrentState(false);
        }

        public void SetCurrentState(bool forCurrent)
        {
            int bitmask = 0;
            string state = null;
            var switchBitMaskList = from info in SwitchInfoList
                                    where info.SwitchUniqueId == (_isNetworkAttached ? _ipAddress : _comPort)
                                    select info;

            //var dupSwitchList = from info in SwitchInfoList
            //                    where info.SwitchUniqueId == (_isNetworkAttached ? _ipAddress : _comPort)
            //                    select info;

            //foreach (RelaySwitch relaySwitch in dupSwitchList.ToList<RelaySwitch>())
            //{
            //    /*if the same bitmask is already in on mode then don't add that value for another sensor again*/
            //    if (relaySwitch.SensorAlarmID != _sensorAlarmId && relaySwitch.SwitchBitMask == _switchBitMask)
            //    {
            //        _switchBitMask = 0;
            //        break;
            //    }
            //}


            string bitmaskValues = string.Empty;

            foreach (RelaySwitch info in switchBitMaskList.ToList<RelaySwitch>())
            {
                /*if the same bitmask is already in on mode then don't add that value for another sensor again*/
                //if (info.SensorAlarmID != _sensorAlarmId && info.SwitchBitMask == _switchBitMask)

                if (bitmaskValues.Contains("," + info.SwitchBitMask.ToStr() + ","))
                {
                    continue;
                }

                //if (info.SwitchBitMask == _switchBitMask)
                //{
                //    bitmask += info._switchBitMask;
                //    bitmaskValues += "," + info.SwitchBitMask.ToStr() + ",";
                //    continue;
                //}
                //else
                //{
                bitmask += info._switchBitMask;
                bitmaskValues += "," + info.SwitchBitMask.ToStr() + ",";
                //}
            }

            for (int i = 0; i <= 7; i++)
            {
                state = state + ((bitmask & (Convert.ToInt32(Math.Pow(2, i)))) != 0 ? (i + 1).ToString() : "-");
            }

            if (forCurrent)
                CurrentState = state;
            else
            {
                NewState = state;
                _lastValue = bitmask;
            }
        }


        /// <summary>
        /// callback method being called after pinging the serial port
        /// </summary>
        /// <param name="data"></param>
        private void DataRecieveFromCom(string data)
        {
            //set the last error to the data received from serial port 
            _LastError = data;
        }
        /// <summary>
        /// method to calculate the switch mask value
        /// </summary>
        /// <param name="switchMask"></param>
        /// <returns></returns>
        public string CalculateSwitchMask(int switchMask)
        {
            string strSwitchMask = string.Empty;
            for (int i = 0; i <= 7; i++)
            {
                strSwitchMask += ((SwitchBitMask & Convert.ToInt32(Math.Pow(2, i))) != 0) ? (i + 1).ToString() : "-";
            }
            return strSwitchMask;
        }


        private void SetRelaySwitchInfo(RelaySwitch relaySwitch)
        {
            try
            {
                this.IoAddress = relaySwitch.IoAddress;
                this.IsNetworkAttached = relaySwitch.IsNetworkAttached;
                this.IpAddress = relaySwitch.IpAddress;
                this.IpPort = relaySwitch.IpPort;
                this.IoAddress = relaySwitch.IoAddress;
                this.ComPort = relaySwitch.ComPort;
                if (relaySwitch.ComSettings != null)
                {
                    this.ComSettings = relaySwitch.ComSettings;
                    this.ComSettings.BaudRate = relaySwitch.ComSettings.BaudRate;
                    this.ComSettings.DataBits = relaySwitch.ComSettings.DataBits;
                    this.ComSettings.ParityBit = relaySwitch.ComSettings.ParityBit;
                    this.ComSettings.StopBit = relaySwitch.ComSettings.StopBit;
                }
                this.SensorAlarmID = relaySwitch.SensorAlarmID;
                this.IsEnabled = relaySwitch.IsEnabled;
                this.SwitchName = relaySwitch.SwitchName;
                this.LastValue = relaySwitch.LastValue;
                this.SwitchBitMask = relaySwitch.SwitchBitMask;
                this.IsDynamicNotificationCleared = true;
                this.FactoryID = relaySwitch.FactoryID;
            }
            catch (Exception ex)
            {
                LogBook.Write("  *** Error in SetRelaySwitchInfo method : " + ex.Message);
            }


        }

    }
}
