/*
 *  File Name : MsgBrdBetaBrite.cs
 *  Author : Pradeep
 *  @ PCC Technology Group LLC
 *  Created Date : 11/28/2010
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

    public class MsgBrd215C : IMessageBoard
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
        private bool _bPlaySound = true;
        private string _sensorFactoryID;

        private Dictionary<string, string> _dLastMsg = new Dictionary<string, string>();
        private Dictionary<string, string> _dLastMsgRaw = new Dictionary<string, string>();
        private Dictionary<string, DateTime> _dLastMsgTime = new Dictionary<string, DateTime>();

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

        public void Receive(string buf)
        {
            throw new NotImplementedException();
        }

        public void DisplayMessage(int ID, string msg, int priority = 5)
        {
            string rawMsg = string.Empty;

            LogBook.Write("Enter display message method");

            //if priority is less than 1 and greater than 9 set priority as 5
            if (priority < 1 || priority > 9) priority = 5;
            //set raw message 
            switch (_brdType)
            {
                default:
                    rawMsg = ((char)127).ToString() + "*~ID" + ID + "~PR" + priority + "~CO3" + msg + "~ES" + ((char)3).ToString();
                    break;
            }
            _dLastMsg[ID.ToString()] = msg;
            _dLastMsgRaw[ID.ToString()] = rawMsg;
            _dLastMsgTime[ID.ToString()] = DateTime.Now;

            SendToBoard(rawMsg);

            if (_dLastMsg.Count == 1)
            {
                //Just added out first alert msg ... clear the time display
                RemoveDateTime();
                //AddBeep();
            }
            LogBook.Write("Exiting method display message");

        }

        private void SendToBoard(string rawMessage)
        {
            LogBook.Write("Enter send to board method");
            //to communicate using TCP/IP
            NetworkClient client = new NetworkClient();
            //to communicate using serial port
            IOPort ioPort = new IOPort();

            if (_isNetworkAttached)
            {
                //send message to message board using TCP/IP 
                //client.TcpClient(_ipAddress, _port, rawMessage);
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
                ioPort.SerialPortOutput(rawMessage);

                //send message to message board using serial port
                //ioPort.SerialPort(_port.ToString(), commSettings, rawMessage, null);
            }
            LogBook.Write("Message sent to message board.");
            LogBook.Write("Exit send to board method");
        }

        private void DataRecievedFromCom(string data)
        {
            LogBook.Write("Data recieved from COM: " + data);
        }
        private void RemoveDateTime()
        {
            LogBook.Write("Enter method remove date time");
            SendToBoard(((char)127).ToString() + "*~MCTTTTT" + ((char)3).ToString());
            LogBook.Write("Exit method remove date time");
        }

        private void DisplayDateTime(bool bForce)
        {
            LogBook.Write("Enter method display date time");
            //set date time for the message board
            SetDateTime(true);
            //format date time
            SendToBoard(((char)127).ToString() + "*~IDTTTTT~SEA~CE~TI3~EE~PA05~CLA" + ((char)3).ToString());
            // Codes:    SEA - Special effect "A" - Standard stationary message
            //           CE - Center message
            //           TI3 - Time insert #3 - "[h]h:mm [a|p]m"
            //           End Effect
            //           PA05 - Pause 05 seconds
            //           CLA - Clear screen

            LogBook.Write("Exit method display date time");

        }

        public void ClearMessage(string msgBoardID)
        {
            LogBook.Write("Enter method Clear message");
            string rawMessage = string.Empty;
            switch (_brdType)
            {
                default:
                    rawMessage = ((char)127).ToString() + "*~MC" + ((char)3).ToString();
                    break;
            }
            _dLastMsg[msgBoardID.ToString()] = "";
            _dLastMsgRaw[msgBoardID.ToString()] = "";
            _dLastMsgTime[msgBoardID.ToString()] = DateTime.MinValue;

            SendToBoard(rawMessage);

            if (_dLastMsg.Count < 1)
            {
                // All messages have been cleared...re-display date/time message
                RemoveBeep();
                DisplayDateTime(true);
            }
        }

        public void ClearAllMessages()
        {
            LogBook.Write("Enter method clear all messages");
            string rawMessage = string.Empty;
            switch (_brdType)
            {
                default:
                    rawMessage = ((char)127).ToStr() + "*~DE" + ((char)3).ToString();
                    break;
            }
            _dLastMsg.Clear();
            _dLastMsgRaw.Clear();
            _dLastMsgTime.Clear();


            SendToBoard(rawMessage);
            RemoveBeep();
            DisplayDateTime(true);

            LogBook.Write("Exit method clear all message");
        }

        public void SetDateTime(bool bForce = false)
        {
            LogBook.Write("Enter method set date time");
            string rawMessage = string.Empty;
            if (bForce && !_isDateTimeSet)
            {
                switch (_brdType)
                {
                    default:
                        rawMessage = ((char)127).ToString() + String.Format("{0:hhmmssMMddYYYY}", DateTime.Now) + ((char)3).ToString();
                        break;
                }
                SendToBoard(rawMessage);

            }
            LogBook.Write("Exit method set date time");
        }

        public void Closed()
        {
            throw new NotImplementedException();
        }

        public void Connected()
        {
            throw new NotImplementedException();
        }

        private void AddBeep(int vol = 8, int priority = 5)
        {
            LogBook.Write("Enter method Add Beep");
            if (_bPlaySound)
            {
                SendToBoard(((char)127).ToString() + "*~VL" + "Volume" + ((char)3).ToString());

                SendToBoard(((char)127).ToString() + "*~IDBEEPX~PR" + "Priority" + "~AU5" + ((char)3).ToString());
            }
            LogBook.Write("Exit method Add Beep");
        }

        private void RemoveBeep()
        {
            LogBook.Write("Enter method remove beep");
            if (_bPlaySound)
            {
                SendToBoard(((char)127).ToString() + "*~MCBEEPX" + ((char)3).ToString());
            }
            LogBook.Write("Exit method remove beep");

        }





        public string SensorAlarmID
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

        public void DisplayMessage(string msg, int priority = 5)
        {
            throw new NotImplementedException();
        }

        string IMessageBoard.Description
        {
            get { throw new NotImplementedException(); }
        }

        string IMessageBoard.COMMSettings
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

        MessageBoardType IMessageBoard.BoardType
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

       

        string IMessageBoard.Name
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

        int IMessageBoard.ID
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

        string IMessageBoard.LastError
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

        bool IMessageBoard.IsEnabled
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

        int IMessageBoard.Port
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

        string IMessageBoard.IPAddress
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

        bool IMessageBoard.IsNetworkAttached
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

        bool IMessageBoard.IsGroup
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

        string IMessageBoard.SensorAlarmID
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

        void IMessageBoard.DisplayMessage(string msg, int priority)
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
