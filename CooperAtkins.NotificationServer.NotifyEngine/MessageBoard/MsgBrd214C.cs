
/*
 *  File Name : MsgBrd214C.cs
 *  Author : Pradeep
 *  @ PCC Technology Group LLC
 *  Created Date : 11/27/2010
 */
namespace CooperAtkins.NotificationServer.NotifyEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO.Ports;
    using CooperAtkins.SocketManager;
    using CooperAtkins.Generic;

    public class MsgBrd214C : IMessageBoard
    {
        private string _commSettings;
        private int _port;
        private MessageBoardType _brdType;
        private int _bytesIn;
        private int _bytesOut;
        private int _id;//Message Board ID
        private string _lastError;
        private bool _isEnabled;//inUse
        private string _ipAddress;
        private bool _isNetworkAttached;
        private bool _isGroup;
        private string _name;
        private string _sensorAlarmID;
        private string _sensorFactoryID;


        private Dictionary<string, int> _msgFileNumber = new Dictionary<string, int>();//dMsgFnum
        private Dictionary<string, string> _msg = new Dictionary<string, string>();//dMsg
        private Dictionary<string, DateTime> _msgTime = new Dictionary<string, DateTime>();//dMsgTime

        private string[] _aFileNumbers;//to store the sID, i.e message board ID
        private string[] _aFileStrings;//to store the chunks of strings of size 120 bits each
        private string[] _stringAddress;//to store the address for chunks of strings; aStringNumbers

        private bool _isDateTimeSet;

        public bool SetServerTime { get; set; }

        public string Description
        {
            get 
            {
                if (_isNetworkAttached)
                    return _ipAddress + ":" + _port.ToString();
                else
                    return _port.ToString();
            }
        }

        public string COMMSettings
        {
            get
            {
                return _commSettings;
            }
            set
            {
                _commSettings = value;
            }
        }

        public MessageBoardType BoardType
        {
            get
            {
                return _brdType;
            }
            set
            {
                _brdType = value;
            }
        }

        public int BytesIn
        {
            get
            {
                return _bytesIn;
            }
            set
            {
                _bytesIn = value;
            }
        }

        public int BytesOut
        {
            get
            {
                return _bytesOut;
            }
            set
            {
                _bytesOut = value;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public int ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public string LastError
        {
            get
            {
                return _lastError;
            }
            set
            {
                _lastError = value;
            }
        }

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
            }
        }

        public int Port
        {
            get
            {
                return _port;
            }
            set
            {
                _port = value;
            }
        }

        public string IPAddress
        {
            get
            {
                return _ipAddress;
            }
            set
            {
                _ipAddress = value;
            }
        }

        public bool IsNetworkAttached
        {
            get
            {
                return _isNetworkAttached;
            }
            set
            {
                _isNetworkAttached = value;
            }
        }

        public bool IsGroup
        {
            get
            {
                return _isGroup;
            }
            set
            {
                _isGroup = value;
            }
        }

        public string SensorAlarmID
        {
             get
             {
                 return _sensorAlarmID;
             }
            set
            {
                _sensorAlarmID = value;
            }
        }

        public string SensorFactoryID
        {
            get
            {
                return _sensorFactoryID;
            }
            set
            {
                _sensorFactoryID = value;
            }
        }

        public void Receive(string buf)
        {
            throw new NotImplementedException();
        }

        public void DisplayMessage(int ID, string msg, int priority = 5)
        {
            //used to build hexa decimal string
            string hexValue = string.Empty;
            //used to store ascii string to be sent to message board
            string data = string.Empty;

            LogBook.Write("Enter display message method");

            //if priority is less than 1 and greater than 9 set priority as 5
            if (priority < 1 || priority > 9) priority = 5;

            //initialize filenumber to zero, used to access the string address
            int fileNumber = 0;
            int index = 0;
            //if message length not less than zero, proceed
            if (msg.Length != 0)
            {
                //if the msgFileNumber dictionary already contains file number for that message board ID
                if (_msgFileNumber.ContainsKey(SensorAlarmID))
                {
                    //fetch the file number from the dictionary using the msgBrdID
                    fileNumber = _msgFileNumber[SensorAlarmID];
                }

                if (fileNumber < 2)
                {
                    //store the message in message dictionary
                    _msg[SensorAlarmID] = msg;
                    //store the time stamp for the message
                    _msgTime[SensorAlarmID] = DateTime.Now;
                    //initialze the message file number
                    _msgFileNumber[ID.ToString()] = 0;

                    
                    //get the index to store message board id in aFileNumber Array
                    while(index < _aFileNumbers.Length)
                    {
                        if (_aFileNumbers[index] != string.Empty)
                            index++;
                        else
                            break;
                    }
                    //store the message board id aFileNumber
                    _aFileNumbers[index] = ID.ToString();
                    //store the index in _msgFileNumber dictionary
                    _msgFileNumber[SensorAlarmID] = index;
                }

                hexValue = "00 FF FF 00 0B FF 01 3" + (index / 10).ToInt().ToString() + " 3" + (index % 10).ToInt().ToString() + " 02 EF B0 EF A2";
                data = Utility.HexToASCII(hexValue);
                hexValue = "FF FF 00";
                data = data + Utility.HexToASCII(hexValue);
            }

            //log file number and message id
            LogBook.Write("Setting File #" + index + " to '" + msg + "'");

            //send message to message boad
            SendToBoard(data);

            ShowActiveFileNumbers();

        }

        private void SendToBoard(string messageData)
        {
            LogBook.Write("Enter send to board method");
            //to communicate using TCP/IP
            NetworkClient client = new NetworkClient();
            //to communicate using serial port
            IOPort ioPort = new IOPort();

            if (_isNetworkAttached)
            {
                //send message to message board using TCP/IP 
                //client.TcpClient(_ipAddress, _port, messageData);
            }
            else
            {
                string[] settingsArray = _commSettings.Split(',');
                IOPort.ComSettings commSettings = new IOPort.ComSettings();
                commSettings.BaudRate = settingsArray[0].ToInt();
                commSettings.ParityBit = Parity.None;
                commSettings.DataBits = settingsArray[3].ToInt16();
                commSettings.StopBit = StopBits.One;

                ioPort.Handshake(_port.ToString(), commSettings, DataRecievedFromCom);
                ioPort.SerialPortOutput(messageData);

                //send message to message board using serial port
                //ioPort.SerialPort(_port.ToString(),commSettings, messageData,null);
            }
            LogBook.Write("Message sent to message board.");
            LogBook.Write("Exit send to board method");

        }
        private void DataRecievedFromCom(string data)
        {
            LogBook.Write("Data recieved from COM: " + data);
        }
        private void ShowActiveFileNumbers()
        {
            string hexValue = string.Empty;
            string data = string.Empty;
            string fileNumbers = string.Empty;
            //get all the active file numbers
            for (int i = 0; i < _aFileNumbers.Length; i++)
            {
                if (_aFileNumbers[i] != string.Empty)
                {
                    fileNumbers = fileNumbers + " 3" + (i / 10).ToInt().ToString() + " 3" + (i % 10).ToInt().ToString() + " ";
                }
            }

            //if the fileNumbers string is empty, display date time
            if (fileNumbers.Length < 0)
            {
                DisplayDateTime();

                fileNumbers = "30 31";
            }

            //Set board to show correct file #s
            hexValue = "00 FF FF 00 0B FF 02 00 00 30 30 30 30 30 30 30 30 " + fileNumbers + " FF 00";
            data = Utility.HexToASCII(hexValue.Replace(" ",""));
            //send data to board
            SendToBoard(data);

        }

        public void ClearMessage(string sensorAlarmID)
        {
            LogBook.Write("Enter clear message method");
            //clear the file number for the specific message board
            _aFileNumbers[_msgFileNumber[sensorAlarmID].ToInt()] = string.Empty;
            //clear the file number
            _msgFileNumber[sensorAlarmID] = 0;
            //clear the message 
            _msg[sensorAlarmID] = string.Empty;
            //clear the time stamp
            _msgTime[sensorAlarmID] = DateTime.MinValue;

            LogBook.Write("Cleared message.");

            ShowActiveFileNumbers();
        }

        public void ClearAllMessages()
        {
            LogBook.Write("Enter method Clear all messages");
            LogBook.Write("Clear all messages command sent to board");
            //clear all message in the message dictionary
            _msg.Clear();
            //clear all message file numbers
            _msgFileNumber.Clear();
            //clear all message time stamps
            _msgTime.Clear();

            //clear all file number associate with message board id's
            int i = 0;
            while (i < _aFileNumbers.Length)
            {
                _aFileNumbers[i] = "";
            }
            //show active file numbers
            ShowActiveFileNumbers();

        }

        public void SetDateTime(bool bForce)
        {
            string hexString = string.Empty, data = string.Empty;

            if (bForce && !_isDateTimeSet)
            {
                hexString = "00 FF FF 00 0B FF 08 " + Utility.Digits2(DateTime.Now.DayOfWeek.ToInt().ToString()) + " 01 ";
                hexString = hexString + String.Format("{0:X}", DateTime.Now.Year.ToString("00")) + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00");
                hexString = hexString + DateTime.Now.Hour.ToString("00") + DateTime.Now.Minute.ToString("00") + DateTime.Now.Second.ToString("00");
                hexString = hexString + " FF 00";

                data = Utility.HexToASCII(hexString.Replace(" ",""));
    
                SendToBoard(data);
        
                _isDateTimeSet = true;
                LogBook.Write("SetDateTime: Current Date/Time sent to message board");
            }
        }

        private void DisplayDateTime()
        {
            string hexValue = string.Empty;
            string data = string.Empty;

            LogBook.Write("Enter method display date time");

            SetDateTime(true);

            //Set file#1 to display time (Bright Green)
            hexValue = "00 FF FF 00 0B FF 01 30 31 02 EF B7 EF A2 EF 80 FF FF 00";
            data = Utility.HexToASCII(hexValue.Replace(" ",""));
            SendToBoard(data);
        }
        
        public void Closed()
        {
            throw new NotImplementedException();
        }

        public void Connected()
        {
            throw new NotImplementedException();
        }


        public void DisplayMessage(string msg, int priority = 5)
        {
            throw new NotImplementedException();
        }


        public bool? IsDynamicNotificationCleared
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
