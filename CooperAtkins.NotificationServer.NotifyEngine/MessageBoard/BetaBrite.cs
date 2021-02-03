using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Collections;

namespace CooperAtkins.NotificationServer.NotifyEngine
{
    public class BetaBrite
    {
        private AlphaSignProtocol.SetMemmoryCommands _memoryCommands = new AlphaSignProtocol.SetMemmoryCommands();
        private AlphaSignProtocol.WriteTextFileCommands _textFileCommands = new AlphaSignProtocol.WriteTextFileCommands();
        private bool _isDebug = System.Diagnostics.Debugger.IsAttached;
        
        //''' <summary>
        //''' Sets the address and communication port of the BetaBrite sign you wish to control
        //''' communication will not be opened until the first command is sent
        //''' </summary>
        public BetaBrite(int comPort = 1, string address = "00")
        {

            //_msgBrdBetaBrite.Port = comPort;
            //_msgBrdBetaBrite.IPAddress = address;
        }
        //''' <summary>
        //''' In debug mode, all commands are 'pretty printed' to Debug.Trace 
        //''' when they are sent to the sign
        //''' </summary>
        public bool DebugMode
        {
            get { return _isDebug; }
            set { _isDebug = value; }
        }

        public ArrayList MemoryItems
        {
            get
            {
                return _memoryCommands.MemoryItems;
            }
        }

        public int MemoryItemCount
        {
            get
            {
                return _memoryCommands.MemoryItems.Count;
            }
        }
               
        //''' <summary>
        //''' Queue a request to use memory for a text element in this file label
        //''' Call AllocateMemory to perform your queued allocations
        //''' </summary>
        public void UseMemoryText(char fileLabel, int sizeBytes)
        {
            _memoryCommands.AllocateTextFile(fileLabel, AlphaSignProtocol.Protection.Locked, sizeBytes);
        }
        /// <summary>
        /// Queue a request to use memory for a string element in this file label
        /// Call AllocateMemory to perform your queued allocations
        /// </summary>
        public void UseMemoryString(char fileLabel, int sizeBytes)
        {
            _memoryCommands.AllocateStringFile(fileLabel, sizeBytes);
        }
        /// <summary>
        /// Queue a request to use memory for a picture of a default (80x7) size in this file label
        /// Call AllocateMemory to perform your queued allocations
        /// </summary>
        public void UseMemoryPicture(char fileLabel)
        {
            _memoryCommands.AllocatePictureFile(fileLabel);
        }
        /// <summary>
        /// Queue a request to use memory for a picture of a specific size in this file label
        /// Call AllocateMemory to perform your queued allocations
        /// </summary>
        public void UseMemoryPicture(char fileLabel, int width, int height)
        {
            _memoryCommands.AllocatePictureFile(fileLabel, AlphaSignProtocol.Protection.Locked, width, height);
        }
        /// <summary>
        /// Allocates all queued memory requests in the sign's memory. This is always destructive!
        /// </summary>
        public byte[] AllocateMemory()
        {
            if ((int)_memoryCommands.Count == 0)
            {
                throw new Exception("No memory to allocate; use the 'UseMemory' commands to specify what type of memory you need first.");
            }
            return _memoryCommands.ToBytes();
            //-- clear for next call
            //_memoryCommands = new AlphaSignProtocol.SetMemmoryCommands();
        }
        /// <summary>
        /// Calculates the exact amount of memory storage required for
        /// a fully expanded message with control codes and/or international characters
        /// </summary>
        public int CalculateMessageLength(string message)
        {
            return AlphaSignProtocol.ExpandedMessageLength(message);
        }
        /// <summary>
        /// Sets the date and time on the sign to the current system date/time
        /// </summary>
        public byte[] SetDateAndTime()
        {
            return SetDateAndTime(DateTime.Now);
        }
        /// <summary>
        /// Sets the date and time on the sign to any arbitrary date/time
        /// </summary>
        public byte[] SetDateAndTime(DateTime dt)
        {
            AlphaSignProtocol.SetDateTimeCommand dc = new AlphaSignProtocol.SetDateTimeCommand(dt);
            //{<NUL><SOH>^00<STX>E 1342<EOT><NUL><SOH>^00<STX>E&6<EOT><NUL><SOH>^00<STX>E;121010<EOT>}
            if (_isDebug)
            {
                Debug.Write(dc.ToString());
            }
            return dc.ToBytes();
        }

       /// <summary>
        /// Sets a run sequence 1 to 128 file labels (note: text files only)
        /// eg, "DEBC" would display text files D, E, B, and C.
        /// </summary>
        public byte[] SetRunSequence(string fileLabels)
        {
            AlphaSignProtocol.SetRunSequenceCommand rc = new AlphaSignProtocol.SetRunSequenceCommand();
            foreach (char c in fileLabels)
            {
                rc.AddFile(c);
            }
            return rc.ToBytes();
        }
        //allocate for memory for displaying message
        public byte[] SetMemoryForDisplay(string message)
        {
            //-- allocate memory for the single message in the first File Label "A"
            AlphaSignProtocol.SetMemoryCommand mc = new AlphaSignProtocol.SetMemoryCommand('A', AlphaSignProtocol.FileType.TextFile, AlphaSignProtocol.Protection.Unlocked, message);
            //Console.WriteLine(mc.ToString());
            return mc.ToBytes();
        }
        /// <summary>
        /// Displays a single message on the sign and holds it there.
        /// This basic command not require allocating memory, but can only display one message in file label "A".
        /// HTML-style formatting codes can be used to specify various display options. 
        /// </summary>
        public byte[] Display(string message)
        {
           //-- fill the first file label "A" with our message
            AlphaSignProtocol.WriteTextFileCommand tc = new AlphaSignProtocol.WriteTextFileCommand('A', message, AlphaSignProtocol.Transition.Hold);
            return tc.ToBytes();
        }

        /// <summary>
        /// Sets a single text message in the specified file label.
        /// Once set, a particular file label can be displayed by setting the RunOrder sequence.
        /// HTML-style markup can be used to specify various display and visual options within the message.
        /// </summary>
        public byte[] SetText(char fileLabel, string message, AlphaSignProtocol.Transition t = AlphaSignProtocol.Transition.Auto, AlphaSignProtocol.Special sm = AlphaSignProtocol.Special.None)
        {
            return new AlphaSignProtocol.WriteTextFileCommand(fileLabel, message, t, sm).ToBytes();
        }
        /// <summary>
        /// Sets multiple text messages in the specified file label.
        /// </summary>
        public byte[] SetTextMultiple(char fileLabel)
        {
            if ((int)_textFileCommands.Count == 0)
            {
                throw new Exception("No text commands were specified; use SetTextMultiple to specify at least one text message first.");
            }
            return _textFileCommands.ToBytes();
            //-- clear after sending
            //_textFileCommands = new AlphaSignProtocol.WriteTextFileCommands();
        }
        /// <summary>
        /// Specifies a text message to be combined with other text messages.
        /// Once set, a particular file label can be displayed by setting the RunOrder sequence.
        /// HTML-style markup can be used to specify various display and visual options within the message.
        /// </summary>
        public void SetTextMultiple(string message = "", AlphaSignProtocol.Transition t = AlphaSignProtocol.Transition.Auto, AlphaSignProtocol.Special sm = AlphaSignProtocol.Special.None)
        {
            _textFileCommands.AddTextFile(message, t, sm);
        }
        /// <summary>
        /// Sets a string message in the sign's memory.
        /// Once set, strings can be displayed via the &lt;CallString=(filelabel)&gt; message markup command
        /// strings can be overwritten in memory without making the sign 'flash', but only support
        /// a subset of the full message markup commands.
        /// </summary>
        public byte[] SetString(char fileLabel, string message)
        {
            return new AlphaSignProtocol.WriteStringFileCommand(fileLabel, message).ToBytes();
        }

        /// <summary>
        /// Clear the sign's memory completely; this also causes it to go into the 
        /// default attract sequence (which is also a pretty good demo of everything
        /// you can do programmatically using this class!)
        /// </summary>
        public byte[] ClearMemory()
        {
            return new AlphaSignProtocol.ClearMemoryCommand().ToBytes();
        }
        /// <summary>
        /// Performs a non-destructive reset of the sign. Memory contents ARE retained.
        /// </summary>
        public byte[] Reset()
        {
            return new AlphaSignProtocol.ResetCommand().ToBytes();
        }
        public void WriteBytes(byte[] b)
        {

            
            TcpClient client = new TcpClient();
            client.Connect(new IPEndPoint(IPAddress.Parse("192.168.1.216"), 10001));

            if ((client.Connected))
            {
                NetworkStream stream = default(NetworkStream);
                stream = client.GetStream();
                stream.Write(b, 0, b.Length);
                stream.Flush();
                stream.Close();
            }
            

            //SerialPort port = new SerialPort("COM1");
            //try
            //{
                
            //    port.BaudRate = 9600;
            //    port.Parity = Parity.Even;
            //    port.DataBits = 7;
            //    port.StopBits = StopBits.One;
            //    port.Open();
            //    port.Write(b,0,b.Length);
            //}
            //catch (Exception ex)
            //{

            //}
            //finally
            //{
            //    port.Close();
            //   port.Dispose();
            //}
            

            
            
        }
        private void SendCommand(AlphaSignProtocol.BaseCommand c)
        {
            if (_isDebug)
            {
                Debug.WriteLine(c.ToString());
            }
            WriteBytes(c.ToBytes());
        }
    }
}
