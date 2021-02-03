
/*
 *  File Name : MsgBrdBetaBrite.cs
 *  Author : Pradeep
 *  @ PCC Technology Group LLC
 *  Created Date : 11/25/2010
 */
namespace CooperAtkins.NotificationServer.NotifyEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.IO.Ports;
    using CooperAtkins.SocketManager;
    using CooperAtkins.Generic;



    public class MsgBrdBetaBrite : IMessageBoard
    {
        private string _commSettings;
        private int _port;
        private MessageBoardType _brdType;
        private int _id;//Message Board ID
        private string _lastError;
        private bool _isEnabled;//inUse
        private string _ipAddress;
        private bool _isNetworkAttached;
        private bool _isGroup;
        private string _name;
        private string _sensorAlarmID;
        private bool? _isDynamicNotificationCleared;
        static Dictionary<string, string> sensorMessages = new Dictionary<string, string>();
        static bool _hasUtilityMessage = false;
        private string _sensorFactoryID;

        public bool SetServerTime { get; set; }
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public string Description
        {
            get
            {
                if (_isNetworkAttached) return _ipAddress + ":" + _port.ToString();
                else
                    return _port.ToString();
            }
        }
        public string COMMSettings
        {
            get { return _commSettings; }
            set { _commSettings = value; }
        }
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }
        public MessageBoardType BoardType
        {
            get { return _brdType; }
            set { _brdType = value; }

        }
        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }
        public string LastError
        {
            get { return _lastError; }
            set { _lastError = value; }
        }
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }
        public string IPAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; }
        }
        public bool IsNetworkAttached
        {
            get { return _isNetworkAttached; }
            set { _isNetworkAttached = value; }
        }
        public bool IsGroup
        {
            get { return _isGroup; }
            set { _isGroup = value; }

        }
        public string SensorAlarmID
        {
            get { return _sensorAlarmID; }
            set { _sensorAlarmID = value; }
        }
        public bool? IsDynamicNotificationCleared
        {
            get { return _isDynamicNotificationCleared; }
            set { _isDynamicNotificationCleared = value; }
        }
        public string SensorFactoryID
        {
            get { return _sensorFactoryID; }
            set { _sensorFactoryID = value; }
        }

        string validFileLabels = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!#$%&()*+,-./:;<=?@[]\\|{}`_";

        public void DisplayMessage(string msg, int priority = 5)
        {
            LogBook.Write(this.SensorFactoryID + " " + this.SensorAlarmID + " " + "Executing DisplayMessage Method");
            try
            {

                //create instance for beta bite message board
                BetaBrite bb = new BetaBrite();

                //to process an array of files ex:"ABCD"  A, B, C, D are the file labels
                string fileLabelsToProcess = string.Empty;
                //we are initializing file label to 66, as we are using 65 to display time and date
                int fileLabel = 66;
                //byte array instance to send data to network
                byte[] byteArray;


                if (SetServerTime)
                {
                    //clear all the messages in the message board
                    WriteBytes(bb.ClearMemory());
                    //wait for a second for the message board to reset
                    Thread.Sleep(100);

                    LogBook.Write(this.SensorFactoryID + " " + this.SensorAlarmID + " " + "Call Display date time method");
                    //else display date and time
                    DisplayDateTime(bb);
                    return;
                }

                //IsDynamicNotificationCleared will be null, if the request comes form utility tool
                if (IsDynamicNotificationCleared == null)
                {
                    LogBook.Write("Message from utility");
                    _hasUtilityMessage = true;
                    sensorMessages.Clear();
                }//notification from the service, IsDynamicNotificationCleared will be false
                else if (IsDynamicNotificationCleared != null)
                {
                    //if the message board has already a message from the utility tool, 
                    //clear that message and set hasUtilityMessage to false
                    if (_hasUtilityMessage)
                    {
                        LogBook.Write("Clearing any messages from utility");
                        sensorMessages.Clear();
                    }
                    _hasUtilityMessage = false;
                }

                //check if the sensor id value already exists in the list
                //for testing message board we are sending IsDynamicNotificationCleared as null
                if (sensorMessages.ContainsKey(SensorAlarmID + "M" + _id + "M".ToString()) && IsDynamicNotificationCleared != null)
                {

                    string message = string.Empty;
                    message = "Check whether the dynamic notification cleared: " + SensorAlarmID + "message:" + msg;

                    //if dynamic notification is not cleared 
                    if (!Convert.ToBoolean(IsDynamicNotificationCleared))
                    {
                        //if the current message is same as the previous message, ignore the message
                        if (msg == sensorMessages[SensorAlarmID + "M" + _id + "M".ToString()])
                        {
                            message += "/r/nDynamic notification was not cleared, ignore sensor information : " + SensorAlarmID + "message:" + msg;
                            return;
                        }
                        else
                        {   //if the current message is not same as the previous message, update the message in the dictionary
                            sensorMessages[SensorAlarmID + "M" + _id + "M".ToString()] = msg;
                        }
                    }
                    else
                    {
                        //remove the sensor information from dictionary
                        message += "/r/nDynamic notification was cleared, Clearing sensor Information for sensorID: " + SensorAlarmID + "message:" + msg;
                        if (sensorMessages.ContainsKey(SensorAlarmID + "M" + _id + "M".ToString()))
                            sensorMessages.Remove(SensorAlarmID + "M" + _id + "M".ToString());
                    }
                    LogBook.Write(this.SensorFactoryID + " " + this.SensorAlarmID + " " + message);
                }
                //new message received from the service
                else if (IsDynamicNotificationCleared == false || IsDynamicNotificationCleared == null)
                {
                    LogBook.Write(this.SensorFactoryID + " " + "Add sensor information to dictionary sensorID: " + SensorAlarmID + "message:" + msg);
                    if (msg == string.Empty)
                        return;

                    //add new message to dictionary
                    sensorMessages.Add(SensorAlarmID + "M" + _id + "M".ToString(), msg);
                    LogBook.Write(this.SensorFactoryID + " " + "Sensor information added to dictionary sensorID: " + SensorAlarmID + "message:" + msg);
                }
                else if (!sensorMessages.ContainsKey(SensorAlarmID + "M" + _id + "M".ToString()) && IsDynamicNotificationCleared == true)
                {
                    /*it will happen only when the key was removed intentionally and when it was unable to connect the message board.*/
                    throw new Exception("Unable to connect");  
                }
                else
                    return;

                foreach (KeyValuePair<string, string> pair in sensorMessages)
                {
                    LogBook.Write("Sensor's to be processed : " + pair.Key);
                }


                //clear all the messages in the message board
                WriteBytes(bb.ClearMemory());
                //wait for a second for the message board to reset
                Thread.Sleep(100);

                //loop through messages in the sensorMessages dictionary
                foreach (KeyValuePair<string, string> pair in sensorMessages)
                {
                    if (pair.Key.Contains("M" + _id + "M"))
                    {
                        //convert int value to corresponding char
                        char charfileLabel = (char)fileLabel;
                        //increment the file label for the next message
                        fileLabel += 1;
                        //validate file label
                        charfileLabel = ValidateFileLabel(charfileLabel, fileLabelsToProcess);
                        //add the new file label to fileLabelsToProcess string
                        fileLabelsToProcess += charfileLabel.ToString();
                        //specify the type of file to be used
                        bb.UseMemoryText(charfileLabel, 256);
                        LogBook.Write(this.SensorFactoryID + " " + "Creating file: " + charfileLabel.ToString() + " Sensor ID : " + pair.Key);
                        //allocate memory
                        byteArray = bb.AllocateMemory();

                        WriteBytes(byteArray);

                        Thread.Sleep(100);

                        LogBook.Write(this.SensorFactoryID + " " + "Allocated memory at file : " + charfileLabel.ToString() + "for Sensor ID : " + pair.Key);
                        //set the message for the text file
                        byteArray = bb.SetText(charfileLabel, "<color=red><font=sevenbold>" + pair.Value, AlphaSignProtocol.Transition.Rotate, AlphaSignProtocol.Special.None);

                        WriteBytes(byteArray);

                        Thread.Sleep(100);

                        LogBook.Write(this.SensorFactoryID + " " + "Set text at file : " + charfileLabel.ToString() + "for Sensor ID : " + pair.Key);
                    }
                }
                //if there are file labels to process
                if (fileLabelsToProcess != string.Empty)
                {
                    LogBook.Write(this.SensorFactoryID + " " + this.SensorAlarmID + " " + "Send run sequence command to message board");
                    //run the messages on the board
                    byteArray = bb.SetRunSequence(fileLabelsToProcess);
                    WriteBytes(byteArray);

                    Thread.Sleep(100);

                    LogBook.Write(this.SensorFactoryID + " " + this.SensorAlarmID + " " + "Sent run sequence command to message board for files : " + fileLabelsToProcess);

                }
                else
                {
                    LogBook.Write(this.SensorFactoryID + " " + this.SensorAlarmID + " " + "Call Display date time method");
                    //else display date and time
                    DisplayDateTime(bb);
                }
            }
            catch (Exception ex)
            {
                LogBook.Write(ex, this.SensorFactoryID + " " + this.SensorAlarmID + " " + "NotifyEngine-MsgBrdBetaBrite");

                if ((bool)IsDynamicNotificationCleared == false)
                {
                    /*remove the message from dictionary to push the data next time when the same message and same board came in*/
                    if (sensorMessages.ContainsKey(SensorAlarmID + "M" + _id + "M".ToString()))
                        sensorMessages.Remove(SensorAlarmID + "M" + _id + "M".ToString());
                }

                throw ex;
            }

        }

        public void WriteBytes(byte[] data)
        {
            //create instance for the TCP/IP and Serial port classes
            NetworkClient client = new NetworkClient();//for TCP
            IOPort comPort = new IOPort();//for COMM port
            //get the comm settings for the connection
            string[] settingsArray = _commSettings.Split(',');
            IOPort.ComSettings commSettings = new IOPort.ComSettings();
            commSettings.BaudRate = settingsArray[0].ToInt();
            commSettings.ParityBit = Parity.Even;
            commSettings.DataBits = settingsArray[2].ToInt16();
            commSettings.StopBit = StopBits.One;
            try
            {
                //if the device is attached to network
                if (_isNetworkAttached)
                {
                    //send data using TCP/IP
                    client.TcpClient(_ipAddress, _port, data);
                }
                else
                {
                    //send data using Serial port
                    LogBook.Write(this.SensorFactoryID + " " + this.SensorAlarmID + " " + "Sending data using UDP Port: " + _port.ToString());
                    comPort.Handshake("COM" + _port.ToString(), commSettings, DataRecievedFromCom);
                    comPort.SerialPortOutput(data);
                }
            }
            catch (Exception ex)
            {
                LogBook.Write(ex, this.SensorFactoryID + " " + this.SensorAlarmID + " " + "NotifyEngine-MessageBoard");
                throw ex;
            }
        }

        private void DataRecievedFromCom(string data)
        {
            LogBook.Write("Data received from COM: " + data);
        }

        public void DisplayDateTime(BetaBrite bb)
        {
            try
            {
                LogBook.Write("Enter method display datetime");
                //"MM/DD/YY","DD/MM/YY","MM-DD-YY","DD-MM-YY","MM.DD.YY","DD.MM.YY","MM DD YY","DD MM YY","MMM.DD, YYYY","MMM.DD,YYYY"
                //"DDD"
                byte[] byteArray;
                //wait for 5secs for message board to reset
                //Thread.Sleep(5000);
                LogBook.Write("Setting date time for the message board");
                //set message board date time to current date and time
                WriteBytes(bb.SetDateAndTime(DateTime.Now));
                //specify the type of file to be used
                bb.UseMemoryText('A', 256);
                LogBook.Write("Creating file: A to display date and time");
                //allocate memory 
                byteArray = bb.AllocateMemory();
                WriteBytes(byteArray);
                LogBook.Write("Allocated memory at file : A");
                //write the message to the file
                byteArray = bb.SetText('A', "<color=green><calldate=DDD> <calltime>", AlphaSignProtocol.Transition.Hold, AlphaSignProtocol.Special.None);
                WriteBytes(byteArray);
                LogBook.Write("Set time and date  formats and calling the time and date functions");
                //run the file on the message board
                byteArray = bb.SetRunSequence("A");
                WriteBytes(byteArray);
                LogBook.Write("Sent run sequence command to message board");

                LogBook.Write("Exiting method display datetime");
            }
            catch (Exception ex)
            {
                LogBook.Write(ex, "NotifyEngine-MessageBoard");
                throw ex;
            }
        }

        private char ValidateFileLabel(char fileLable, string fileLabelsToProcess)
        {
            char validFilelabel = ' ';
            //check if the current file label "fileLable" does not exists in the list of valid file labels
            if (!validFileLabels.Contains(fileLable))
            {
                //loop through all the valid file chars
                foreach (char c in validFileLabels)
                {
                    //if the character is already being used , loop for the next char
                    if (!fileLabelsToProcess.Contains(c))
                    {
                        validFilelabel = c;
                    }
                }

            }
            else
            {
                //else return the current file lable
                validFilelabel = fileLable;
            }
            return validFilelabel;
        }




    }
}
