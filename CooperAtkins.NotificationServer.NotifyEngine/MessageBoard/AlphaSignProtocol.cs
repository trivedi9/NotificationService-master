using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace CooperAtkins.NotificationServer.NotifyEngine
{
    public class AlphaSignProtocol
    {
        private enum Ascii : byte
        {
            NUL = 0x0,
            SOH = 0x1,
            STX = 0x2,
            EOT = 0x4,
            ESC = 0x1b,
            CR = 0xd
        }

        private enum SignType : byte
        {
            AllSigns = 0x5a,
            // "Z"
            BetaBrite = 0x5e
            // "^"
            //-- dozens more in protocol doc (!)
        }

        public enum CommandCode : byte
        {
            WriteTextFile = 65,
            // "A"
            ReadTextFile = 66,
            // "B"
            WriteSpecialFunction = 69,
            // "E"
            ReadSpecialFunction = 70,
            // "F"
            WriteStringFile = 71,
            // "G"
            ReadStringFile = 72,
            // "H"
            WritePictureFile = 73,
            // "I"
            ReadPictureFile = 74
            // "J"
        }

        private static string _address = "00";
        private const SignType _signType = SignType.BetaBrite;
        private const int _nullHeaderCount = 10;
        private const int _maxPictureWidth = 80;
        private const int _maxPictureHeight = 7;

        public AlphaSignProtocol(string address = "00")
        {
            _address = address;
        }
        // ''' <summary>
        //''' The identifier or “address” of the sign represented by two ASCII digits as a number between “00” and “FF” (0 to 255). 
        //''' Address "00" is reserved as a broadcast address. The wildcard character “?” (3FH) can be used to send messages to a 
        //''' range of addresses. For example, a Sign Address of “0?” will access signs with address between 01H and 0FH (1 and 15).
        //''' Defaults to "00"
        //''' </summary>
        public string Address
        {
            get
            {
                return _address;
            }
            set
            {
                if (!Regex.IsMatch(value, "^[0-9a-fA-F?]{2}$"))
                {
                    throw new ArgumentOutOfRangeException("Address", value, "Address must be two ASCII hex digits or the wildcard character '?'");
                }
                _address = value;
            }
        }

        #region BaseClass
        //''' <summary>
        //''' character betweeh 20h (32) and 7fH (127)
        //''' </summary>
        //''' <remarks>
        //'''  File Label 0 (30H) is used for a Priority TEXT file
        //'''  File Label 0 (30H) and ? (3FH) cannot be used as STRING file labels.
        //''' </remarks>
        public class BetaBriteFile
        {
            private byte _labelByte = 0x41;

            public BetaBriteFile()
            {

            }
            public BetaBriteFile(char c)
            {
                this.Label = c;
            }
            public BetaBriteFile(string s)
            {
                this.LabelString = s;
            }
            public int LabelInt
            {
                get
                {
                    return Convert.ToInt32(_labelByte);
                }
                set
                {
                    if (value > 94 || value < 0)
                    {
                        throw new ArgumentOutOfRangeException("LabelInt", "Label must be between 0 and 94");
                    }
                    _labelByte = Convert.ToByte(value + 20);
                }
            }
            public string LabelString
            {
                get
                {
                    return Convert.ToString(this.Label);
                }
                set
                {
                    if (value.Length > 1)
                    {
                        throw new ArgumentOutOfRangeException("LabelString", "Label must be a single ASCII character.");
                    }
                    this.Label = Convert.ToChar(value);

                }
            }
            public char Label
            {
                get
                {
                    return Convert.ToChar(_labelByte);
                }
                set
                {
                    if (!Regex.IsMatch(value.ToString(), "[\\x20-\\x7e]"))
                    {
                        throw new ArgumentOutOfRangeException("Label", "Label must be a single ASCII character between 'space' (20h) and 'half-space' (7eh)");
                    }
                    _labelByte = Encoding.ASCII.GetBytes(value.ToString())[0];
                }
            }
            public byte Byte
            {
                get { return _labelByte; }
                set
                {
                    if (value > 94 || value < 0)
                    {
                        throw new ArgumentOutOfRangeException("FileLabel", "FileLabel must be between 20h and 7eh");
                    }
                    _labelByte = value;
                }
            }


        }

        #endregion

        //''' <summary>
        //''' Base class for an Alpha Communications Protocol command
        //''' </summary>
        public abstract class BaseCommand
        {
            protected CommandCode commandCode;
            public bool isNested = false;

            public virtual new string ToString()
            {
                return BytesToString(this.ToBytes());
            }

            public virtual byte[] ToBytes()
            {
                if (commandCode == 0)
                {
                    throw new ArgumentNullException("CommandCode", "The CommandCode was not specified");
                }

                System.IO.MemoryStream ms = new System.IO.MemoryStream();

                if (!isNested)
                {
                    //-- A minimum of 5 <NUL>s must be transmitted;
                    //-- the sign uses these to establish the baud rate
                    for (int i = 0; i <= _nullHeaderCount - 1; i++)
                    {
                        //(byte)Enum.Parse(typeof(Volume), volume)
                        ms.WriteByte((byte)Enum.Parse(typeof(Ascii), Ascii.NUL.ToString()));
                    }

                    //-- the <SOH> is the "Start of Header" ASCII character
                    ms.WriteByte((byte)Enum.Parse(typeof(Ascii), Ascii.SOH.ToString()));

                    //-- sign type we're addressing with this command
                    ms.WriteByte((byte)_signType);

                    //-- sign address, if you have multiple signs
                    //-- this is set via dip switches or firmware
                    ms.Write(Encoding.ASCII.GetBytes(_address), 0, 2);
                }

                //-- The <STX> is the "Start of Text" ASCII character; it always precedes a command code
                //-- NOTE: When nesting packets, there must be at least a 100-millisecond delay after the <STX>.
                ms.WriteByte((byte)Enum.Parse(typeof(Ascii), Ascii.STX.ToString()));

                //-- one ASCII character defines the command
                ms.WriteByte((byte)commandCode);

                //-- this calls the method on the inherited child
                string DataField = FormDataField();

                //-- made up of ASCII characters which are dependent on the preceding Command Code
                ms.Write(Encoding.ASCII.GetBytes(DataField), 0, DataField.Length);

                //-- The <EOT> is the "End of Text" ASCII character
                ms.WriteByte((byte)Enum.Parse(typeof(Ascii), Ascii.EOT.ToString()));

                ms.Close();
                return ms.ToArray();
            }
            protected abstract string FormDataField();
        }
        private static string BytesToString(byte[] ba)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in ba)
            {
                if (Convert.ToInt32(b) < 32)
                {
                    if (Enum.IsDefined(typeof(Ascii), b))
                    {
                        sb.Append("<" + ((Ascii)b).ToString() + ">");
                        if (b == (byte)Enum.Parse(typeof(Ascii), Ascii.CR.ToString()))
                        {
                            sb.Append(Environment.NewLine);
                        }
                    }
                    else
                    {
                        sb.Append(string.Format("<{0:x2}>", b));
                    }
                }
                else
                {
                    sb.Append(Convert.ToChar(b));
                }
            }
            return sb.ToString();
        }

        // ''' <summary>
        //''' TextFile used in SetTextCommand and SetTextCommands
        //''' </summary>

        public enum LinePosition : byte
        {
            Middle = 0x20   //space 
            //-- betabrite is a single line sign
            //Top = 0x22      ' double quote
            //Bottom = 0x26   ' &
            //Fill = 0x30     ' 0
            //Left = 0x31     ' 1
            //Right = 0x32    ' 2
        }

        public enum Transition : byte
        {
            //''' <summary>
            //''' Message travels right to left
            //''' </summary>
            Rotate = 0x61, // a
            //''' <summary>
            //''' Message remains stationary
            //''' </summary>        
            Hold = 0x62,   //b
            //''' <summary>
            //''' Message remains stationary and flashes
            //''' </summary>
            Flash = 0x63,            //c        
            //''' <summary>
            //''' Previous message is pushed up by a new message
            //''' </summary>
            RollUp = 0x65,           //e
            //''' <summary>
            //''' Previous message is pushed down by a new message
            //''' </summary>
            RollDown = 0x66,         //f
            //''' <summary>
            //''' Previous message is pushed left by a new message
            //''' </summary>
            RollLeft = 0x67,         //g
            //''' <summary>
            //''' Previous message is pushed right by a new message
            //''' </summary>
            RollRight = 0x68,        //h
            //''' <summary>
            //''' New message is wiped over the previous message from bottom to top
            //''' </summary>
            WipeUp = 0x69,           //i
            //''' <summary>
            //''' New message is wiped over the previous message from top to bottom
            //''' </summary>
            WipeDown = 0x6A,         //j
            //''' <summary>
            //''' New message is wiped over the previous message from right to left
            //''' </summary>
            WipeLeft = 0x6B,         //k
            //''' <summary>
            //''' New message is wiped over the previous message from left to right
            //''' </summary>
            WipeRight = 0x6C,        //l
            //''' <summary>
            //''' New message line pushes the bottom line to the top line if 2-line sign
            //''' </summary>
            Scroll = 0x6D,           //m
            //''' <summary>
            //''' Various Modes are called upon to display the message automatically
            //''' </summary>
            Auto = 0x6F,           //n
            //''' <summary>
            //''' Previous message is pushed toward the center of the display by the new message
            //''' </summary>
            RollIn = 0x70,           //o
            //''' <summary>
            //''' Previous message is pushed outward from the center by the new message
            //''' </summary>
            RollOut = 0x71,          //p
            //''' <summary>
            //''' New message is wiped over the previous message in an inward motion
            //''' </summary>
            WipeIn = 0x72,           //q
            //''' <summary>
            //''' New message is wiped over the previous message in an outward motion
            //''' </summary>
            WipeOut = 0x73,          //s
            //''' <summary>
            //''' Message travels right to left. Characters are approximately one half their normal width
            //''' </summary>
            CompressedRotate = 0x74,  //t
            //''' <summary>
            //''' Special transition defined in the Special enumeration
            //''' </summary>
            Special = 0x6E          //n
        }

        public enum Special : byte
        {
            //''' <summary>
            //''' no special mode selected; used as a default only
            //''' </summary>
            None = 0x0,          //NUL
            //''' <summary>
            //''' Message will twinkle on the sign (all LEDs will flash rapidly)
            //''' </summary>
            Twinkle = 0x30,      //0
            //''' <summary>
            //''' New message will sparkle over the current message
            //''' </summary>
            Sparkle = 0x31,      //1
            //''' <summary>
            //''' Message will drift on to the display like falling snow
            //''' </summary>
            Snow = 0x32,         //2
            //''' <summary>
            //''' New message will interlock over the current message in alternating rows of dots from each end
            //''' </summary>
            Interlock = 0x33,    //3
            //''' <summary>
            //''' Alternating characters “switch” off the sign up and down. New message “switches” on in a similar manner
            //''' </summary>
            Switch = 0x34,       //4
            //''' <summary>
            //''' New message sprays across and onto the sign from right to left
            //''' </summary>
            Spray = 0x36,        //6
            //''' <summary>
            //''' small explosions blast the new message onto the sign
            //''' </summary>
            Starburst = 0x37,    //7
            //''' <summary>
            //''' welcome is written in script across the sign and changes multiple colors
            //''' </summary>
            Welcome = 0x38,      //8
            //''' <summary>
            //''' Slot machine symbols appear randomly across the sign
            //''' </summary>
            SlotMachine = 0x39,  //9
            //''' <summary>
            //''' Satellite dish broadcasts the words News Flash on the sign
            //''' </summary>
            NewsFlash = 0x41,    //A
            //''' <summary>
            //''' Animated trumpet blows multicolored notes across the sign
            //''' </summary>
            Trumpet = 0x42,      //B
            //''' <summary>
            //''' color changes from one color to another
            //''' </summary>
            CycleColors = 0x43,  //C
            //''' <summary>
            //''' Thank You is written in script across the sign and changes multiple colors
            //''' </summary>
            ThankYou = 0x53,     //S
            //''' <summary>
            //''' A cigarette image appears, is then extinguished and replaced with a no smoking symbol
            //''' </summary>
            NoSmoking = 0x55,    //U
            //''' <summary>
            //''' A car runs into a cocktail glass and is replaced with the text “Please don’t drink and drive”
            //''' </summary>
            DontDrink = 0x56,    //V
            //''' <summary>
            //''' fish swim across the sign, then are chased back across it by a shark
            //''' </summary>
            Fish = 0x57,         //W
            //''' <summary>
            //''' Large fireworks explode randomly across the sign
            //''' </summary>
            Fireworks = 0x58,    //X
            //''' <summary>
            //''' Party baloons scroll up the display
            //''' </summary>
            Balloon = 0x59,      //Y
            //''' <summary>
            //''' A bomb fuse burns down followed by a giant explosion
            //''' </summary>
            CherryBomb = 0x5A   //Z
        }

        public enum FileType : byte
        {
            //''' <summary>
            //''' File represents displayable text
            //''' </summary>
            TextFile = 0x41,         // A
            //''' <summary>
            //''' File represents a string variable
            //''' </summary>
            StringFile = 0x42,     //B 
            //''' <summary>
            //''' File represents a bitmap image
            //''' </summary>
            PictureFile = 0x44      //D
        }

        public enum Protection : byte
        {
            //''' <summary>
            //''' Can be changed via the infrared remote
            //''' </summary>
            Unlocked = 0x55,     // U
            //''' <summary>
            //''' Cannot be changed via the infrared remote
            //''' </summary>
            Locked = 0x4C       //L
        }

        public enum SpecialFunction : byte
        {
            TimeOfDay = 0x20,
            // space
            Memory = 0x24,
            // $
            DayOfWeek = 0x26,
            // &
            TimeFormat = 0x27,
            // double quote
            RunTime = 0x29,
            // )
            SoftReset = 0x2c,
            // comma
            RunSequence = 0x2e,
            // period
            RunDay = 0x32,
            //  2
            ClearSerialError = 0x34,
            // 4
            Address = 0x37,
            // 7
            Date = 0x3b
            // ;
        }

        public enum RunSequenceOrder
        {
            ByTime = 0x54,
            ByOrder = 0x53,
            ByTimeThenDelete = 0x44
        }

        public enum RunDay : byte
        {
            Daily = 0x30,
            Sun = 0x31,
            Mon = 0x32,
            Tue = 0x33,
            Wed = 0x34,
            Thu = 0x35,
            Fri = 0x36,
            Sat = 0x37,
            MonToFri = 0x38,
            SatSun = 0x39,
            Always = 0x41,
            Never = 0x42
        }

        private enum ControlChars : byte
        {
            Flash = 0x7,
            ExtChar = 0x8,
            CallDate = 0xB,
            NewLine = 0xD,
            NoHold = 0x9,
            CallString = 0x10,
            WideOff = 0x11,
            WideOn = 0x12,
            CallTime = 0x13,
            CallPic = 0x14,
            Speed1 = 0x15,
            Speed2 = 0x16,
            Speed3 = 0x17,
            Speed4 = 0x18,
            Speed5 = 0x19,
            Font = 0x1A,
            Color = 0x1C,
            CharAttrib = 0x1D,
            CharSpacing = 0x1E
        }
        private enum Color : byte
        {
            Red = 0x31,
            Green = 0x32,
            Amber = 0x33,
            DimRed = 0x34,
            DimGreen = 0x35,
            Brown = 0x36,
            Orange = 0x37,
            Yellow = 0x38,
            Rainbow1 = 0x39,
            Rainbow2 = 0x41,
            ColorMix = 0x42,
            Auto = 0x43
        }
        private enum Font : byte
        {
            Five = 0x31,
            FiveBold = 0x32,
            FiveWide = 0x3b,
            FiveWideBold = 0x3e,
            Seven = 0x33,
            SevenSerif = 0x35,
            SevenBold = 0x34,
            SevenBoldSerif = 0x36,
            SevenShadow = 0x37,
            SevenShadowSerif = 0x3a,
            SevenWide = 0x3c,
            SevenWideBold = 0x39,
            SevenWideBoldSerif = 0x38,
            SevenWideSerif = 0x3d
        }
        private enum ExtChar
        {
            EnterKey = 0x62,
            YKey = 0x63,
            UpArrow = 0x64,
            DownArrow = 0x65,
            LeftArrow = 0x66,
            RightArrow = 0x67,
            Pacman = 0x68,
            Sailboat = 0x69,
            Baseball = 0x6a,
            Telephone = 0x6b,
            Heart = 0x6c,
            Car = 0x6d,
            Handicap = 0x6e,
            Rhino = 0x6f,
            Mug = 0x70,
            Satellite = 0x71,
            Copyright = 0x72
        }
        private enum CharAttrib
        {
            Wide = 0x30,
            DoubleWide = 0x31
            //-- these don't appear to work on the BetaBrite
            //DoubleHigh = &H32
            //TrueDescenders = &H33
            //FixedWidth = &H34
            //Fancy = &H35
            //Shadow = &H1D
        }

        #region TextFileCommands
        //'' <summary>
        //''' TextFile used in SetTextCommand and SetTextCommands
        //''' </summary>
        private class TextFile
        {
            private LinePosition _linePosition = LinePosition.Middle;
            private string _message;
            public Transition transitionType = Transition.Auto;
            public Special specialMode = Special.None;

            public TextFile()
            {
            }

            public TextFile(string message, Transition t = Transition.Auto, Special s = Special.None)
            {
                this._message = message;
                this.transitionType = t;
                this.specialMode = s;
            }

            public string Message
            {
                get { return _message; }
                set
                {
                    value = ExpandMessage(value);
                    if (!Regex.IsMatch(value, "[\\x00-\\x7f]*"))
                    {
                        throw new ArgumentOutOfRangeException("Message", "Text messages can only contain ASCII characters in the range 00-7F.");
                    }
                    _message = value;
                }
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Convert.ToChar(Ascii.ESC));
                sb.Append(Convert.ToChar(_linePosition));
                sb.Append(Convert.ToChar(transitionType));
                if (this.transitionType == Transition.Special)
                {
                    if (specialMode == Special.None)
                    {
                        throw new ArgumentException("TransitionType was set to special, but SpecialMode was not specified (None).", "SpecialMode");
                    }
                    sb.Append(Convert.ToChar(specialMode));
                }
                //-- An ASCII Message cannot be displayed if the previous field (Special Specifier) is a Special
                //-- Graphic. To display text after a Special Graphic, another Mode Field must be used.
                sb.Append(_message);
                return sb.ToString();
            }

        }

        //''' <summary>
        //''' Write a single text command to a File
        //''' </summary>
        public class WriteTextFileCommand : BaseCommand
        {
            private TextFile _textFile;
            public BetaBriteFile _file = new BetaBriteFile();


            public WriteTextFileCommand()
            {
                this.commandCode = CommandCode.WriteTextFile;
            }

            public WriteTextFileCommand(char fileLabel, string message = "", Transition t = Transition.Auto, Special sm = Special.None)
            {
                this.commandCode = CommandCode.WriteTextFile;
                this._file.Label = fileLabel;
                _textFile = new TextFile()
                {
                    Message = message,
                    transitionType = t,
                    specialMode = sm
                };
            }

            protected override string FormDataField()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(_file.Label);
                sb.Append(_textFile.ToString());
                return sb.ToString();
            }

        }


        //''' <summary>
        //''' Writes multiple TEXT commands to a File
        //''' </summary>
        public class WriteTextFileCommands : BaseCommand
        {
            public BetaBriteFile file = new BetaBriteFile();
            private ArrayList _textFiles;

            public WriteTextFileCommands()
            {
                this.commandCode = CommandCode.WriteTextFile;
            }

            public WriteTextFileCommands(char fileLabel)
                : this()
            {
                this.file.Label = fileLabel;
            }

            public object Count
            {
                get { return _textFiles.Count; }
            }

            public void AddTextFile(string message, Transition t = Transition.Auto, Special sm = Special.None)
            {
                TextFile tf = new TextFile(message, t, sm);
                _textFiles.Add(tf);
            }

            protected override string FormDataField()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(this.file.Label);
                if (_textFiles.Count == 0)
                {
                    throw new ArgumentException("TextCommand command contains no TextFiles; use AddTextFile to add items first");
                }
                foreach (TextFile tf in _textFiles)
                {
                    sb.Append(tf.ToString());
                }
                return sb.ToString();
            }

        }

        #endregion


        #region StringFileCommands
        /// <summary>
        /// Writes data to string file
        /// </summary>
        public class WriteStringFileCommand : BaseCommand
        {
            public BetaBriteFile file = new BetaBriteFile();
            private string _fileData;

            public string FileData
            {
                get { return _fileData; }
                set
                {
                    if (!Regex.IsMatch(value, "[\\x20-\\x7f\\x09\\x0d\\x11-\\x13\\x15-\\x19\\x1a\\x1c\\1xe]*"))
                    {
                        throw new ArgumentOutOfRangeException("StringData", "Strings can only contain ASCII characters 20-7F or a subset of the formatting codes.");
                    }
                    _fileData = value;
                }
            }

            public WriteStringFileCommand(char fileLabel, string fileData = "")
            {
                base.commandCode = CommandCode.WriteStringFile;
                this._fileData = fileData;
                this.file.Label = fileLabel;
            }


            protected override string FormDataField()
            {
                return file.Label + _fileData;
            }
        }
        #endregion

        //''' <summary>
        //''' MemoryItem used in SetMemoryCommand and SetMemoryCommands
        //''' </summary>

        public class MemoryItem
        {
            public BetaBriteFile betaBriteFile = new BetaBriteFile();
            public FileType fileType = FileType.TextFile;
            public Protection protection = Protection.Unlocked;
            public int sizeBytes = 0;
            public DateTime startTime = DateTime.MinValue;
            public DateTime stopTime = DateTime.MaxValue;
            public int width = 0;
            public int height = 0;
            public char Label
            {
                get
                {
                    return betaBriteFile.Label;
                }
                set
                {
                    betaBriteFile.Label = value;
                }
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(betaBriteFile.Label);
                sb.Append(Convert.ToChar(fileType));
                //-- protection type
                if (fileType == FileType.StringFile)
                {
                    //-- string files MUST be locked
                    sb.Append(Convert.ToChar(Protection.Locked));
                }
                else
                {
                    sb.Append(Convert.ToChar(this.protection));
                }

                //-- 4-digit ASCII hex size
                switch (fileType)
                {
                    case FileType.StringFile:
                    case FileType.TextFile:
                        if (sizeBytes == 0)
                        {
                            throw new ArgumentOutOfRangeException("SizeBytes", "File size, in bytes, must be specified.");
                        }
                        sb.Append(String.Format("{0:x4}", sizeBytes));
                        break;
                    case FileType.PictureFile:

                        break;
                }
                //-- misc data per-type
                switch (fileType)
                {
                    case FileType.TextFile:
                        if (startTime == null || stopTime == null)
                        {
                            sb.Append(TimeToByteAscii(TimeConstant.Always));
                            sb.Append(TimeToByteAscii(TimeConstant.StartOfDay));
                        }
                        else
                        {
                            sb.Append(TimeToByteAscii((DateTime)startTime));
                            sb.Append(TimeToByteAscii((DateTime)stopTime));
                        }
                        break;
                    case FileType.StringFile:
                        //-- constant padding; not used
                        sb.Append("0000");
                        break;
                    case FileType.PictureFile:
                        //'Valid entries are “1000” = monochrome, “2000” = 3-color, “4000”= 8-color (RGB)
                        //'-- always 4000 for betabrite..
                        sb.Append("4000");
                        break;
                }

                return sb.ToString().ToUpper();
            }
        }

        // ''' <summary>
        //''' Stores a single item in the sign's memory
        //''' (via constructor only)
        //''' </summary>

        public class SetMemoryCommand : BaseCommand
        {
            private MemoryItem _memoryItem;

            public SetMemoryCommand()
            {
                this.commandCode = CommandCode.WriteSpecialFunction;
            }

            public SetMemoryCommand(char fileLabel, FileType ft, Protection p, int sizeBytes, DateTime startTime, DateTime stopTime)
                : this()
            {

                _memoryItem = new MemoryItem()
                {
                    Label = fileLabel,
                    fileType = ft,
                    protection = p,
                    sizeBytes = sizeBytes,
                    startTime = startTime,
                    stopTime = stopTime
                };
            }
            public SetMemoryCommand(char fileLabel, FileType ft, Protection p, int sizeBytes)
                : this(fileLabel, ft, p, sizeBytes, DateTime.MinValue, DateTime.MaxValue)
            {
            }
            public SetMemoryCommand(char fileLabel, FileType ft, Protection p, string message)
                : this(fileLabel, ft, p, ExpandedMessageLength(message), DateTime.MinValue, DateTime.MaxValue)
            {
            }

            public SetMemoryCommand(char fileLabel, FileType ft, Protection p, string message, DateTime startTime, DateTime stopTime)
                : this(fileLabel, ft, p, ExpandedMessageLength(message), startTime, stopTime)
            {
            }

            protected override string FormDataField()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Convert.ToChar(SpecialFunction.Memory));
                sb.Append(_memoryItem.ToString());
                return sb.ToString();
            }

        }

        //''' <summary>
        //''' Stores multiple items in the sign's memory
        //''' </summary>
        public class SetMemmoryCommands : BaseCommand
        {
            private ArrayList _memoryItems = new ArrayList();

            public SetMemmoryCommands()
            {
                base.commandCode = CommandCode.WriteSpecialFunction;
            }
            public ArrayList MemoryItems
            {
                get
                {
                    return _memoryItems;
                }
            }
            public void AllocateTextFile(char fileLabel, Protection p, int sizeBytes)
            {
                MemoryItem mi = new MemoryItem();
                mi.Label = fileLabel;
                mi.fileType = FileType.TextFile;
                mi.protection = p;
                mi.sizeBytes = sizeBytes;
                _memoryItems.Add(mi);
            }

            public object Count
            {
                get { return _memoryItems.Count; }
            }

            public void AllocateTextFile(char fileLabel, Protection p, string message)
            {
                AllocateTextFile(fileLabel, p, ExpandedMessageLength(message));
            }

            public void AllocateStringFile(char fileLabel, int sizeBytes)
            {
                MemoryItem mi = new MemoryItem();
                mi.Label = fileLabel;
                mi.fileType = FileType.StringFile;
                mi.protection = Protection.Locked;
                mi.sizeBytes = sizeBytes;
                _memoryItems.Add(mi);
            }

            public void AllocateStringFile(char fileLabel, string message)
            {
                AllocateStringFile(fileLabel, ExpandedMessageLength(message));
            }
            public void AllocatePictureFile(char fileLabel, Protection p, int width, int height)
            {
                MemoryItem mi = new MemoryItem();
                mi.Label = fileLabel;
                mi.fileType = FileType.PictureFile;
                mi.protection = p;
                mi.width = width;
                mi.height = height;
                _memoryItems.Add(mi);
            }
            public void AllocatePictureFile(char fileLabel)
            {
                AllocatePictureFile(fileLabel, Protection.Unlocked, 80, 7);
            }
            protected override string FormDataField()
            {
                if (_memoryItems.Count == 0)
                {
                    throw new ArgumentException("Memory command contains no items; use AddMemoryItem to add items first");
                }
                StringBuilder sb = new StringBuilder();
                sb.Append(Convert.ToChar(SpecialFunction.Memory));
                foreach (MemoryItem mi in _memoryItems)
                {
                    sb.Append(mi.ToString());
                }
                return sb.ToString();
            }

        }
        //''' <summary>
        //''' Sets the run (display) sequence for a series of 1-128 text files. If a file label is invalid or 
        //''' does not exist, the next one in the sequence will run.
        //''' </summary>
        public class SetRunSequenceCommand : BaseCommand
        {
            private ArrayList _files = new ArrayList();
            public RunSequenceOrder order = RunSequenceOrder.ByOrder;
            public Protection protection = Protection.Unlocked;

            public SetRunSequenceCommand()
            {
                base.commandCode = CommandCode.WriteSpecialFunction;
            }

            public SetRunSequenceCommand(RunSequenceOrder rso, Protection p)
                : this()
            {
                this.order = rso;
                this.protection = p;
            }

            public void AddFile(char fileLabel)
            {
                if (_files.Count > 128)
                {
                    throw new ArgumentOutOfRangeException("AddFile", "Run sequences cannot exceed 128 text files.");
                }
                _files.Add(new BetaBriteFile(fileLabel));
            }
            protected override string FormDataField()
            {
                if (_files.Count == 0)
                {
                    throw new ArgumentException("Run sequence contains no files; use AddFile to add files.");
                }
                StringBuilder sb = new StringBuilder();
                sb.Append(Convert.ToChar(SpecialFunction.RunSequence));
                sb.Append(Convert.ToChar(order));
                sb.Append(Convert.ToChar(protection));
                foreach (BetaBriteFile f in _files)
                {
                    sb.Append(f.Label);
                }
                return sb.ToString();
            }



        }

        //''' <summary>
        //''' Set the times to start and stop displaying a particular Text file
        //''' </summary>
        public class SetRunTimeCommand : BaseCommand
        {
            public DateTime start = DateTime.MinValue;
            public DateTime stop = DateTime.MaxValue;
            public BetaBriteFile file;

            public SetRunTimeCommand()
            {
                base.commandCode = CommandCode.WriteSpecialFunction;
            }

            public SetRunTimeCommand(char fileLabel, DateTime startTime, DateTime stopTime)
                : this()
            {
                this.file.Label = fileLabel;
                this.start = startTime;
                this.stop = stopTime;
            }

            protected override string FormDataField()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Convert.ToChar(SpecialFunction.RunTime));
                sb.Append(file.Label);
                sb.Append(TimeToByteAscii(start));
                sb.Append(TimeToByteAscii(stop));
                return sb.ToString();
            }
        }

        //''' <summary>
        //''' Set the days to start and stop displaying a particular Text file
        //''' </summary>
        public class SetRunDayCommand : BaseCommand
        {

            public BetaBriteFile file;
            public RunDay start = RunDay.Daily;
            public RunDay stop = RunDay.Always;

            public SetRunDayCommand()
            {
                base.commandCode = CommandCode.WriteSpecialFunction;
            }

            public SetRunDayCommand(char fileLabel, RunDay start, RunDay stop)
                : this()
            {
                this.file.Label = fileLabel;
                this.start = start;
                this.stop = stop;
            }

            protected override string FormDataField()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Convert.ToChar(SpecialFunction.RunDay));
                sb.Append(file.Label);
                sb.Append(Convert.ToChar(start));
                sb.Append(Convert.ToChar(stop));
                return sb.ToString();
            }
        }

        //''' <summary>
        //''' Clears all sign memory
        //''' </summary>
        public class ClearMemoryCommand : BaseCommand
        {

            public ClearMemoryCommand()
            {
                base.commandCode = CommandCode.WriteSpecialFunction;
            }
            protected override string FormDataField()
            {
                return Convert.ToChar(SpecialFunction.Memory).ToString();
            }
        }

        //''' <summary>
        //''' Performs a soft (non-destructive) reset of the sign; memory is retained
        //''' </summary>
        public class ResetCommand : BaseCommand
        {
            public ResetCommand()
            {
                base.commandCode = CommandCode.WriteSpecialFunction;
            }
            protected override string FormDataField()
            {
                return Convert.ToChar(SpecialFunction.SoftReset).ToString();
            }
        }

        #region Date/Time Commands
        public enum TimeFormat : byte
        {
            Standard = 0x53,
            Military = 0x4D
        }
        //''' <summary>
        //''' Set format of time displayed on the sign, either
        //''' standard or 24-hour (military)
        //''' </summary>
        public class SetTimeFormatCommand : BaseCommand
        {
            private TimeFormat _tf = TimeFormat.Standard;
            public SetTimeFormatCommand(TimeFormat tf)
            {
                base.commandCode = CommandCode.WriteSpecialFunction;
            }
            protected override string FormDataField()
            {
                return Convert.ToChar(SpecialFunction.TimeFormat).ToString() + Convert.ToChar(_tf).ToString();
            }
        }

        //''' <summary>
        //''' Set the date, time, and day of week on the display
        //''' </summary>
        internal class SetDateTimeCommand
        {
            private SetTimeCommand _timeCommand;
            private SetDayCommand _dayCommand;
            private SetDateCommand _dateCommand;
            private DateTime _dt;

            public DateTime Date
            {
                get
                {
                    return _dt;
                }
                set
                {
                    _dt = value;
                }
            }

            public SetDateTimeCommand(DateTime dt)
            {
                _dt = dt;
            }

            public override string ToString()
            {
                return BytesToString(this.ToBytes());
            }
            public byte[] ToBytes()
            {
                MemoryStream ms = new MemoryStream();

                _timeCommand = new SetTimeCommand(_dt);
                _dayCommand = new SetDayCommand(_dt);
                _dateCommand = new SetDateCommand(_dt);
                byte[] b = null;
                b = _timeCommand.ToBytes();
                ms.Write(b, 0, b.Length);
                b = _dayCommand.ToBytes();
                ms.Write(b, 0, b.Length);
                b = _dateCommand.ToBytes();
                ms.Write(b, 0, b.Length);
                return ms.ToArray();
            }
        }

        //''' <summary>
        //''' Sets the time on the display
        //''' </summary>
        private class SetTimeCommand : BaseCommand
        {
            private DateTime _dt;
            public SetTimeCommand()
            {
                base.commandCode = CommandCode.WriteSpecialFunction;
                _dt = DateTime.Now;
            }
            public SetTimeCommand(DateTime dt)
                : this()
            {
                _dt = dt;
            }
            protected override string FormDataField()
            {
                return Convert.ToChar(SpecialFunction.TimeOfDay).ToString() + _dt.ToString("HHmm").ToString();
            }
        }

        //''' <summary>
        //''' Sets the day of week on the display
        //''' </summary>
        private class SetDayCommand : BaseCommand
        {
            private DateTime _dt;
            public SetDayCommand()
            {
                this.commandCode = CommandCode.WriteSpecialFunction;
                _dt = DateTime.Now;
            }
            public SetDayCommand(DateTime dt)
                : this()
            {
                _dt = dt;
            }
            protected override string FormDataField()
            {
                return Convert.ToChar(SpecialFunction.DayOfWeek).ToString() + Convert.ToString(Convert.ToInt32(_dt.DayOfWeek) + 1);
            }
        }

        //''' <summary>
        //''' Sets the date on the display
        //''' </summary>
        private class SetDateCommand : BaseCommand
        {
            private DateTime _dt;
            public SetDateCommand()
            {
                base.commandCode = CommandCode.WriteSpecialFunction;
                _dt = DateTime.Now;
            }
            public SetDateCommand(DateTime dt)
                : this()
            {
                _dt = dt;
            }
            protected override string FormDataField()
            {
                return Convert.ToChar(SpecialFunction.Date).ToString() + _dt.ToString("MMddyy");
            }
        }

        #endregion

        #region Text Formatting


        //''' <summary>
        //''' Returns the number of bytes in a fully expanded message that contains
        //''' control code tags and/or international characters
        //''' </summary>
        public static int ExpandedMessageLength(string message)
        {
            return ExpandMessage(message).Length + 1;
        }
        //''' <summary>
        //''' Expands message control code tags and/or international characters, if present
        //''' </summary>
        private static string ExpandMessage(string message)
        {
            message = ExpandControlCodes(message);
            message = ExpandInternationalChars(message);
            return message;
        }
        //''' <summary>
        //''' returns a list of vaguely HTML-style commands and parameters, eg
        //''' &lt;font=fiveslim&gt;Hello &lt;color=red&gt;World
        //''' </summary>
        private static string ExpandControlCodes(string s)
        {

            //-- no pseudo-HTML found? nothing to expand; exit
            if (!Regex.IsMatch(s, "<[^>]+?>"))
            {
                return s;
            }

            string CommandCode = null;
            string Parameter = null;
            string Expansion = null;
            foreach (Match m in Regex.Matches(s, "<(?<Command>[^=>]+)=*(?<Parameter>[^=>]+)*.*?>"))
            {
                CommandCode = m.Groups["Command"].Value;
                Parameter = m.Groups["Parameter"].Value;
                Expansion = ExpandControlChar((ControlChars)MapCommandToEnum(CommandCode), Parameter);
                s = s.Replace(m.Value, Expansion);
            }

            return s;
        }
        //''' <summary>
        //''' high ascii chars must be expressed as double-byte chars in a specific BetaBrite format
        //''' </summary>
        private static string ExpandInternationalChars(string s)
        {

            //-- no high ascii found? nothing to expand; exit
            if (!Regex.IsMatch(s, "[\\x80-\\xFF]"))
                return s;

            string b = System.Text.Encoding.ASCII.GetString(System.Text.Encoding.ASCII.GetBytes(s));
            foreach (Match m in Regex.Matches(s, "[\\x80-\\xFF]"))
            {
                s = s.Replace(m.Value, ExpandInternationalChar(Convert.ToChar(m.Value)));
            }

            return s;
        }
        private static string ExpandInternationalChar(char c)
        {
            int asciimap;
            switch (c)
            {
                case 'Ç':
                    asciimap = 0x20;
                    break;
                case 'Ü':
                    asciimap = 0x21;
                    break;
                case 'é':
                    asciimap = 0x22;
                    break;
                case 'â':
                    asciimap = 0x23;
                    break;
                case 'ä':
                    asciimap = 0x24;
                    break;
                case 'à':
                    asciimap = 0x25;
                    break;
                case 'å':
                    asciimap = 0x26;
                    break;
                case 'ç':
                    asciimap = 0x27;
                    break;
                case 'ê':
                    asciimap = 0x28;
                    break;
                case 'ë':
                    asciimap = 0x29;
                    break;
                case 'è':
                    asciimap = 0x2A;
                    break;
                case 'Ï':
                    asciimap = 0x2B;
                    break;
                case 'Î':
                    asciimap = 0x2C;
                    break;
                case 'Ì':
                    asciimap = 0x2D;
                    break;
                case 'Ä':
                    asciimap = 0x2E;
                    break;
                case 'Å':
                    asciimap = 0x2F;
                    break;
                case 'É':
                    asciimap = 0x30;
                    break;
                case 'æ':
                    asciimap = 0x31;
                    break;
                case 'Æ':
                    asciimap = 0x32;
                    break;
                case 'ô':
                    asciimap = 0x33;
                    break;
                case 'ö':
                    asciimap = 0x34;
                    break;
                case 'ò':
                    asciimap = 0x35;
                    break;
                case 'Û':
                    asciimap = 0x36;
                    break;
                case 'ù':
                    asciimap = 0x37;
                    break;
                case 'Ÿ':
                    asciimap = 0x38;
                    break;
                case 'Ö':
                    asciimap = 0x39;
                    break;
                //case 'Ü':
                //    asciimap = 0x3A;
                //        break;
                case '¢':
                    asciimap = 0x3B;
                    break;
                case '£':
                    asciimap = 0x3C;
                    break;
                case '¥':
                    asciimap = 0x3D;
                    break;
                case 'ƒ':
                    asciimap = 0x3F;
                    break;
                case 'á':
                    asciimap = 0x40;
                    break;
                case 'í':
                    asciimap = 0x41;
                    break;
                case 'ó':
                    asciimap = 0x42;
                    break;
                case 'ú':
                    asciimap = 0x43;
                    break;
                case 'ñ':
                    asciimap = 0x44;
                    break;
                case 'Ñ':
                    asciimap = 0x45;
                    break;
                case '¿':
                    asciimap = 0x48;
                    break;
                case '°':
                    asciimap = 0x49;
                    break;
                case '¡':
                    asciimap = 0x4A;
                    break;
                case ' ':
                    asciimap = 0x4B;
                    break;
                case 'ø':
                    asciimap = 0x4D;
                    break;
                case 'Ø':
                    asciimap = 0x4C;
                    break;
                case 'c':
                    asciimap = 0x4E;
                    break;
                case 'C':
                    asciimap = 0x4F;
                    break;
                //case 'c':
                //    asciimap = 0x50;
                //        break;
                //case 'C':
                //    asciimap = 0x51;
                //        break;
                case 'd':
                    asciimap = 0x52;
                    break;
                case 'Ð':
                    asciimap = 0x53;
                    break;
                case 'Š':
                    asciimap = 0x54;
                    break;
                case 'Ž':
                    asciimap = 0x55;
                    break;
                case 'ž':
                    asciimap = 0x56;
                    break;
                case 'ß':
                    asciimap = 0x57;
                    break;
                case 'š':
                    asciimap = 0x58;
                    break;
                case 'Á':
                    asciimap = 0x5A;
                    break;
                case 'À':
                    asciimap = 0x5B;
                    break;
                case 'Ã':
                    asciimap = 0x5C;
                    break;
                case 'ã':
                    asciimap = 0x5D;
                    break;
                case 'Ê':
                    asciimap = 0x5E;
                    break;
                case 'Í':
                    asciimap = 0x5F;
                    break;
                case 'Õ':
                    asciimap = 0x60;
                    break;
                case 'õ':
                    asciimap = 0x61;
                    break;
                default:
                    //-- remove anything we can't map; it'll be illegal anyway
                    return "";
            }

            return Convert.ToChar(ControlChars.ExtChar).ToString() + Convert.ToChar(asciimap).ToString();
        }

        private static string ExpandCharAttrib(string param)
        {
            if (param.IndexOf(",") == -1)
            {
                throw new ArgumentException("The character attribute parameter must include a comma followed by a boolean value.");
            }

            //-- extract the boolean trailer
            string parameterBool = Regex.Match(param, "[^,]+$").Value;
            param = Regex.Match(param, "^[^,]+").Value;

            CharAttrib ca = default(CharAttrib);
            object o = ParseEnum(param, typeof(CharAttrib));
            if (o == null)
            {
                throw new ArgumentException("The character attribute '" + param + "' is not recognized.");
            }
            else
            {
                ca = (CharAttrib)o;
            }
            return Convert.ToChar(ca) + ExpandBool(parameterBool);
        }

        private static string ExpandColor(string param)
        {
            Color c = default(Color);
            object o = ParseEnum(param, typeof(Color));
            if (o == null)
            {
                throw new ArgumentException("The color '" + param + "' is not recognized.");
            }
            else
            {
                c = (Color)o;
            }
            return Convert.ToChar(c).ToString();
        }
        private static string ExpandFont(string param)
        {
            Font f = default(Font);
            object o = ParseEnum(param, typeof(Font));
            if (o == null)
            {
                throw new ArgumentException("The font '" + param + "' is not recognized.");
            }
            else
            {
                f = (Font)o;
            }
            return Convert.ToChar(f).ToString();
        }
        private static string ExpandBool(string param)
        {
            if (!Regex.IsMatch(param, "^on|^1|^true|^yes|^off|^0|^false|^no", RegexOptions.IgnoreCase))
            {
                throw new ArgumentException("The parameter '" + param + "' should be a boolean.");
            }

            if (Regex.IsMatch(param, "^on|^1|^true|^yes", RegexOptions.IgnoreCase))
            {
                return "1";
            }
            else
            {
                return "0";
            }
        }
        private static string ExpandDateAttrib(string param)
        {
            switch (param.ToUpper())
            {
                case "MM/DD/YY":
                    return "0";
                case "DD/MM/YY":
                    return "1";
                case "MM-DD-YY":
                    return "2";
                case "DD-MM-YY":
                    return "3";
                case "MM.DD.YY":
                    return "4";
                case "DD.MM.YY":
                    return "5";
                case "MM DD YY":
                    return "6";
                case "DD MM YY":
                    return "7";
                case "MMM.DD, YYYY":
                case "MMM.DD,YYYY":
                    return "8";
                case "DDD":
                    return "9";
                default:
                    throw new ArgumentException("The date format '" + param + "' is not recognized.");
            }
        }

        private static string ExpandExtendedChar(string param)
        {
            ExtChar ec = default(ExtChar);
            object o = ParseEnum(param, typeof(ExtChar));
            if (o == null)
            {
                throw new ArgumentException("The extended character '" + param + "' is not recognized.");
            }
            else
            {
                ec = (ExtChar)o;
            }
            return Convert.ToChar(ec).ToString();
        }
        private static char ExpandFileLabel(string param)
        {
            //-- let the filelabel do our validation for us
            BetaBriteFile fl = new BetaBriteFile(param);
            return fl.Label;
        }
        private static string ExpandControlChar(ControlChars c, string param)
        {

            string s = Convert.ToChar(c).ToString();

            switch (c)
            {
                case ControlChars.CallDate:
                    s += ExpandDateAttrib(param);
                    break;
                case ControlChars.CallPic:
                    s += ExpandFileLabel(param);
                    break;
                case ControlChars.CallString:
                    s += ExpandFileLabel(param);
                    break;
                case ControlChars.CallTime:
                    break;
                case ControlChars.CharAttrib:
                    s += ExpandCharAttrib(param);
                    break;
                case ControlChars.CharSpacing:
                    break;
                case ControlChars.Color:
                    s += ExpandColor(param);
                    break;
                case ControlChars.ExtChar:
                    s += ExpandExtendedChar(param);
                    break;
                case ControlChars.Flash:
                    s += ExpandBool(param);
                    break;
                case ControlChars.Font:
                    s += ExpandFont(param);
                    break;
                case ControlChars.NewLine:
                    break;
                case ControlChars.Speed1:
                    break;
                case ControlChars.Speed2:
                    break;
                case ControlChars.Speed3:
                    break;
                case ControlChars.Speed4:
                    break;
                case ControlChars.Speed5:
                    break;
                case ControlChars.WideOff:
                    break;
                case ControlChars.WideOn:
                    break;
                default:
                    break;
            }

            return s;
        }
        private static Enum MapCommandToEnum(string CommandCode)
        {
            object o = ParseEnum(CommandCode, typeof(ControlChars));
            if ((o != null))
            {
                return (ControlChars)o;
            }
            throw new ArgumentException("The command '" + CommandCode + "' is not recognized.");
        }
        private static object ParseEnum(string s, Type t)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }
            object o = null;
            try
            {
                o = System.Enum.Parse(t, s, true);
            }
            catch (System.ArgumentException ex)
            {
                //-- if the string representation provided doesn't match 
                //-- any known enum case, we'll get this exception
            }
            return o;
        }
        #endregion


        //''' <summary>
        //''' converts a time to hex code between 00-8F (10 minute intervals)
        //''' 00 equals 12:00-12:10am, 01 equals 12:10-12:20am, etc.
        //''' </summary>
        //''' <remarks>
        //''' Stop Time is ignored when Start Time is set to Always (FF)
        //''' </remarks>
        private static string TimeToByteAscii(DateTime dt)
        {
            if (dt == DateTime.MinValue)
            {
                return TimeToByteAscii(TimeConstant.StartOfDay);
            }
            if (dt == DateTime.MaxValue)
            {
                return TimeToByteAscii(TimeConstant.Always);
            }
            int i = Convert.ToInt32(Math.Floor(dt.TimeOfDay.TotalMinutes / 10));
            return string.Format("{0:x2}", i);
        }
        private static string TimeToByteAscii(TimeConstant t)
        {
            return string.Format("{0:x2}", Convert.ToByte(t));
        }
        public enum TimeConstant : byte
        {
            StartOfDay = 0x0,
            EndOfDay = 0x8f,
            AllDay = 0xfd,
            Never = 0xfe,
            Always = 0xff
        }
    }
}
