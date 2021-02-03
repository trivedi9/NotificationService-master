/*
 *  File Name : INotificationProtocolServer.cs
 *  Author : Pradeep 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/26/2010
 *  
 *  Change Log:
 *  1. Added ParallePort Methods - By Rajesh, 12/1/2010
 *     ParallePortOutput,ParallePortReset
 *     (Paste the inpout32.dll in windows/system32 folder to execute above methods)
 *  
 */
namespace CooperAtkins.SocketManager
{
    using System;
    using System.Text;
    using System.IO.Ports;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Wrapper for Serial and Parallel Port
    /// </summary>
    public class IOPort
    {
        [DllImport("inpout32.dll", EntryPoint = "Out32")]
        private static extern void Output(int adress, int value);

        [DllImport("inpout32.dll", EntryPoint = "Inp32")]
        private static extern void Input(int adress);

        private SerialPort _comPort = null;

        public delegate void DataReceived(string data);

        public DataReceived _dataReceived;

        public class ComSettings
        {
            private int _baudRate;
            private Parity _parityBit;
            private int _dataBits;
            private StopBits _stopBit;

            public int BaudRate
            {
                get { return _baudRate; }
                set { _baudRate = value; }
            }

            public Parity ParityBit
            {
                get { return _parityBit; }
                set { _parityBit = value; }

            }

            public int DataBits
            {
                get { return _dataBits; }
                set { _dataBits = value; }

            }

            public StopBits StopBit
            {
                get { return _stopBit; }
                set { _stopBit = value; }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        /// <param name="settings"></param>
        /// <param name="dataReceived"></param>
        public void Handshake(string port, ComSettings settings, DataReceived dataReceived = null)
        {
            if (_comPort == null)
            {
                _comPort = new SerialPort(port);
                if (settings != null)
                {
                    _comPort.BaudRate = settings.BaudRate;//set baud rate
                    _comPort.Parity = settings.ParityBit;//set parity bit
                    _comPort.DataBits = settings.DataBits;//set data bits
                    _comPort.StopBits = settings.StopBit;//set stop bit
                }
                if (dataReceived != null)
                {
                    _dataReceived = dataReceived;
                    _comPort.DataReceived += new SerialDataReceivedEventHandler(_comPort_DataReceived);
                }

            }

        }
        void _comPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = "";

            byte[] buffer = new byte[1024];

            _comPort.Read(buffer, 0, buffer.Length);

            data = Encoding.ASCII.GetString(buffer);

            _dataReceived(data);
        }
        public void SerialPortOutput(string data)
        {
            if (_comPort == null)
                throw new Exception("Before write the data into serial port call handshake method");

            if (!_comPort.IsOpen)
                _comPort.Open();

            _comPort.Write(data);
        }
        public void SerialPortOutput(byte[] data, bool closePort)
        {
            if (_comPort == null)
                throw new Exception("Before write the data into serial port call handshake method");

            if (!_comPort.IsOpen)
                _comPort.Open();


            _comPort.Write(data, 0, data.Length);
            if (closePort)
                CloseComPort();
        }

        public void SerialPortOutput(byte[] data)
        {
            if (_comPort == null)
                throw new Exception("Before write the data into serial port call handshake method");

            if (!_comPort.IsOpen)
                _comPort.Open();


            _comPort.Write(data, 0, data.Length);
            CloseComPort();
        }
        public void CloseComPort()
        {
            if (_comPort != null)
            {
                if (_comPort.IsOpen)
                    _comPort.Close();
                _comPort.Dispose();
            }
        }

        public static void ParallePortOutput(int port, int data)
        {
            Output(port, data);
        }

        public static void ParallePortReset(int port)
        {
            Output(port, 0);
        }
    }
}
