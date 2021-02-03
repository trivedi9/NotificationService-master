using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Net.Sockets;
using System.Xml;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using CooperAtkins.NotificationEngine.Utility.Notify;


namespace CooperAtkins.NotificationEngine.Utility
{
    class Program
    {
        static string _server = string.Empty;
        static int _port = 0;
        static StringBuilder logBuilder = new StringBuilder();
        static object localNEObj = null;

        static bool CommandLineExec(string[] args)
        {
            //Utility.exe -c xxxxxxxx:xxxx -m "<sadadadsad>"
            //Utility.exe -m "<sadadadsad>"

            bool flag = true;


            //Console.WriteLine("Args: " + args.Length.ToString());

            if (args.Length == 4 && args[0].ToLower() == "-c" && args[2].Trim() == "-m")
            {
                Console.WriteLine("Remote Execution: ");
                string[] ipinfo = args[1].Trim().Split(':');

                _server = ipinfo[0].Trim();
                _port = Convert.ToInt32(ipinfo[1].Trim());

                Send(args[3]);
            }
            else if (args.Length == 2 && args[0].ToLower() == "-m")
            {
                Console.WriteLine("LocalExecution: ");
                LoadLocalNotifyEngine();
                LocalExecution(args[1]);
            }
            else
                flag = false;

            return flag;
        }
        static void Main(string[] args)
        {

            if (CommandLineExec(args))
                return;

            string userInput;

            Console.Title = "Cooper Atkins Notification Utility";

            WriteLine("Notification Engine Utility Program\n");
            WriteLine("(C) Cooper Atkins 2015\n");

            bool exitFlag = false;
            while (true)
            {

                Console.Write("\nNotification>");
                string[] commandArray = new string[1];
                if (args.Length>0) 
                {
                    commandArray = args;
                    WriteLine("\r\nNotification>" + args[0], false);
                    args=null;
                }
                else
                {
                    userInput = Console.ReadLine();
                    commandArray = userInput.Split(' ');
                    WriteLine("\r\nNotification>" + userInput, false);
              
                }

                
                IniHelp iniHelp = new IniHelp();
                
                string settingsXml = string.Empty;
                string inputYN = "";
                switch (commandArray[0].ToLower())
                {
                    case "help":
                        DisplayHelp(commandArray[0].ToLower());
                        break;
                    case "removeprocessall":
                        WriteLine("Are you sure you want to stop all the notification process?(Y/N):");
                        inputYN = Console.ReadLine();
                        WriteLine(inputYN, false);

                        if (inputYN.ToUpper() != "Y")
                            break;

                        foreach (Process pro in Process.GetProcessesByName("CooperAtkins.NotificationClient.Service"))
                        {
                            try
                            {
                                pro.Kill();
                            }
                            catch { }
                        }
                        break;
                    case "removeprocess":

                        //if (commandArray.Length <= 2) {

                        //  WriteLine("Invalid arguments. Syntax: process [-r] [-s][-a] <processid>\n -r  remove \n -s single process or -a all processes");
                        //}

                        //if (commandArray[1].ToLower() == "-r")
                        WriteLine("Are you sure you want to stop notification process?(Y/N):");
                        inputYN = Console.ReadLine();
                        WriteLine(inputYN, false);

                        if (inputYN.ToUpper() != "Y")
                            break;

                        int result;
                        if (!int.TryParse(commandArray[1], out result))
                        {
                            WriteLine("Invalid argument. Enter valid process id.");
                            break;
                        }
                        int pid = Convert.ToInt32(result);
                        foreach (Process pro in Process.GetProcessesByName("CooperAtkins.NotificationClient.Service"))
                        {
                            if (pro.Id == pid)
                            {
                                try
                                {
                                    pro.Kill();
                                }
                                catch { }
                            }
                        }
                        break;

                    case "ping":
                        Send("command ping");
                        break;
                    case "exit":
                    case "quit":
                        exitFlag = true;
                        break;
                    case "testivr":
                        PhoneNotify notify = new PhoneNotify();                        
                         string phoneNumber = "";

                        if (commandArray.Length == 2)
                        {
                            if (commandArray[1].Length != 10)
                            {
                                WriteLine("Please provide valid phone number.\n");
                                DisplayHelp(commandArray[0].ToLower());
                                break;
                            }
                            phoneNumber = commandArray[1].Trim();

                        }
                        else
                        {
                            WriteLine("Invalid notification command.\n");
                            DisplayHelp(commandArray[0].ToLower());
                        }
                        
                  

                            AdvancedNotifyRequest anr = new AdvancedNotifyRequest();
                            PhoneNotify pn = new PhoneNotify();
                            
                            anr.CallerIDName = "TempTrak Alert System";
                            anr.CallerIDNumber = "888-533-6900"; //TT registration #
                            anr.PhoneNumberToDial = phoneNumber;
                            //anr.TextToSay = "This is a test call from TempTrak for,, <say-as type=\"number:digits\">" + phoneNumber + "</say-as>" + ",,,Press the star key to acknowlege this call,, Thank You,,,This is a test call from TempTrak for,, <say-as type=\"number:digits\">" + phoneNumber + "</say-as>" + ",,,Press the star key to acknowlege this call,, Thank You,,,This is a test call from TempTrak for,, <say-as type=\"number:digits\">" + phoneNumber + "</say-as>" + ",,,Press the star key to acknowlege this call,, Thank You,,,";
                            //anr.TextToSay = /*"~\\ActOnFeature(false)~*/ "~\\SetVar(Attempt|1)~ ~\\ActOnDigitPress(false)~   ~\\Label(Menu)~ ~\\AssignDTMF(*|Ack)~ ~\\ActOnDigitPress(true)~ ~\\Beep()~ ~\\PlaySilence(0.1)~" + sensorInfo + " ~\\WaitForDTMF(1)~" + "Press the star key to acknowledge receipt of this alert.  Press 1 to repeat this message. ~\\PlaySilence(0.1)~   ~\\WaitForDTMF(10)~ ~\\IncreaseVariable(Attempt|1)~ ~\\GotoIf(Attempt|1|Menu)~ ~\\GotoIf(Attempt|2|Menu)~ ~\\GotoIf(Attempt|3|AttemptEnd)~ ~\\Label(AttemptEnd)~ Good Bye ~\\EndCall()~ ~\\Label(Ack)~ ~\\PlaySilence(0.1)~ Thank you for acknowledging receipt of this alert. ~\\PlaySilence(0.1)~ Log into TempTrak to take corrective action and officially clear alert. ~\\PlaySilence(0.1)~ Good Bye. ~\\EndCall()~";
                            string promptMessage = @"This is the Temp Track voice notification system  ~\\PlaySilence(0.2)~ There is an alarm with a sensor named "
                    + "Research Specimen Freezer 696"+ " ~\\PlaySilence(0.2)~ The Last Reading was " + "-5 degrees C"
                   + " ~\\PlaySilence(0.2)~ Sensor has been operating out of range for  ~\\PlaySilence(0.1)~ " + "30 minutes";
                            
                            anr.TextToSay = /*"~\\ActOnFeature(false)~*/ "~\\SetVar(Attempt|1)~ ~\\ActOnDigitPress(false)~   ~\\Label(Menu)~ ~\\AssignDTMF(*|Ack)~ ~\\ActOnDigitPress(true)~ ~\\Beep()~ ~\\PlaySilence(0.1)~" +promptMessage+ " ~\\WaitForDTMF(1)~" + "Press the star key to acknowledge receipt of this alert.  Press 1 to repeat this message. ~\\PlaySilence(0.1)~   ~\\WaitForDTMF(10)~ ~\\IncreaseVariable(Attempt|1)~ ~\\GotoIf(Attempt|1|Menu)~ ~\\GotoIf(Attempt|2|Menu)~ ~\\GotoIf(Attempt|3|AttemptEnd)~ ~\\Label(AttemptEnd)~ Good Bye ~\\EndCall()~ ~\\Label(Ack)~ ~\\PlaySilence(0.1)~ Thank you for acknowledging receipt of this alert. ~\\PlaySilence(0.1)~ Log into TempTrak to take corrective action and officially clear alert. ~\\PlaySilence(0.1)~ Good Bye. ~\\EndCall()~";
                            anr.VoiceID = 6;
                            anr.UTCScheduledDateTime = DateTime.UtcNow;
                            anr.LicenseKey = "b9eec503-0308-46f0-b5f2-01d3ae1182ea"; //stored in config file for now
                            anr.TryCount = 1;
                            anr.NextTryInSeconds = 0;
                            anr.TTSvolume = 5;

                            NotifyReturn nr = pn.NotifyPhoneAdvanced(anr);

                            Console.WriteLine("Call Server Response Text: " + nr.ResponseText);
                       //     Console.WriteLine("Call Server Response Code: " + nr.ResponseCode);
                            Console.WriteLine("Call QueueID: " + nr.QueueID);
                            Console.WriteLine("After Call is Complete: type 'getivrstatus "+nr.QueueID+"' to see call results");

                           for (int f=0; f<40; f++)
                           {
                               System.Threading.Thread.Sleep(2000);
                               if (f==0) 
                               {
                                   Console.WriteLine("");
                               } 
                               else 
                               {
                                   Console.Write("- ");
                               }
                           }
                                                                     
          
                        break;
                    case "getivrstatus":
                       string commandlineInput = commandArray[1].ToString();
                        long TransactionID = Convert.ToInt64(commandlineInput);

                        PhoneNotify notify2 = new PhoneNotify();
                        NotifyReturn notifyResult2 = notify2.GetQueueIDStatus(TransactionID);
                        Console.WriteLine(notifyResult2.ResponseText);

                        Console.WriteLine("Ans Machine Detection: " + notifyResult2.MachineDetection);
                        Console.WriteLine("Demo: " + notifyResult2.Demo);
                        
                        Console.WriteLine("Digits Pressed: " +notifyResult2.DigitsPressed);
                            Console.WriteLine("Call Duration: "+notifyResult2.Duration);
                            Console.WriteLine("Call Start Time: " + notifyResult2.StartTime);
                            Console.WriteLine("Call End Time: " + notifyResult2.EndTime);
                            Console.WriteLine("Call QueueID: " + notifyResult2.QueueID);
                            Console.WriteLine("Try Count: " + notifyResult2.TryCount);
                            Console.WriteLine("Text To Say: " + notifyResult2.TextToSay);
                            Console.WriteLine("Call Answered: " + notifyResult2.CallAnswered);
                            Console.WriteLine("Call Complete: " + notifyResult2.CallComplete);

                        if ((notifyResult2.MachineDetection=="MACHINE") || (notifyResult2.DigitsPressed=="")){
                            string[] commandArray2 = new string[] {"testivr"};
                            
                            Main(commandArray2);
                        }
                        break;

                    case "report":
                        FileStream logFileStream = File.Create(AppDomain.CurrentDomain.BaseDirectory + "Log.txt");
                        logFileStream.Write(Encoding.ASCII.GetBytes(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "\r\n"), 0, (DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "\r\n").Length);
                        logFileStream.Write(Encoding.ASCII.GetBytes(logBuilder.ToString().Replace("\\n", " ")), 0, logBuilder.ToString().Length);
                        logFileStream.Flush();
                        logFileStream.Close();
                        logFileStream.Dispose();
                        logFileStream = null;
                        Process process1 = new Process();
                        process1.StartInfo = new ProcessStartInfo(AppDomain.CurrentDomain.BaseDirectory + "Log.txt");
                        process1.Start();
                        break;
                    case "connect":
                        if (commandArray.Length > 2)
                        {
                            bool flag = ValidateIpPort(commandArray[1].Trim(), commandArray[2].Trim());
                            if (!flag)
                            {
                                WriteLine("Please provide valid IP Address and port.\n");
                                break;
                            }
                            _server = commandArray[1].Trim();
                            _port = Convert.ToInt32(commandArray[2].Trim());
                            Send("command ping");
                            localNEObj = null;
                        }
                        else
                        {
                            if (localNEObj == null)
                            {
                                //string notificationInstallationPath = Environment.GetEnvironmentVariable("NotifyServerPath", EnvironmentVariableTarget.Machine); ;

                                LoadLocalNotifyEngine();

                            }
                            else
                            {
                                WriteLine("Invalid notification command.\n");
                                DisplayHelp(commandArray[0].ToLower());
                            }
                        }
                        break;
                    case "clear":
                    case "cls":
                        Console.Clear();
                        break;
                    case "open":
                        Process process = new Process();
                        process.StartInfo = new ProcessStartInfo(AppDomain.CurrentDomain.BaseDirectory + "Commands.ini");
                        process.Start();
                        break;
                    default:
                        string[] keys = null;

                        if (commandArray[0].ToLower() != "utilitymessageformats")
                        {
                            keys = iniHelp.IniReadKeys(commandArray[0].ToLower());
                        }

                        if (keys != null)
                        {
                            StringBuilder sb = new StringBuilder();

                            if (commandArray.Length == 1)
                            {
                                string[] format = GetMessageFormat(commandArray[0].ToLower());
                                if (!string.IsNullOrEmpty(format[0]) && !string.IsNullOrEmpty(format[1]))
                                {
                                    string[] arrayVars = format[1].Split(',');
                                    string message = format[0];
                                    for (int i = 0; i < arrayVars.Length; i++)
                                    {
                                        Console.Write("Enter " + arrayVars[i].Trim() + ":");
                                        message = message.Replace("{" + i.ToString() + "}", Console.ReadLine());
                                    }
                                    if (commandArray[0].ToLower() == "remotepopup" || commandArray[0].ToLower() == "serverpopup")
                                    {
                                        string temp = "00000000";
                                        int strLength = temp.Length - message.Length.ToString().Length;
                                        message = temp.Substring(0, strLength) + message.Length + message;
                                        sb = new StringBuilder(message);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("\nPlease enter message to send (Press ctrl+z to end the message):\n");
                                    string line = string.Empty;
                                    do
                                    {
                                        line = Console.ReadLine();
                                        sb.Append(line);
                                    } while (line != null);
                                }
                            }
                            else
                            {
                                sb = new StringBuilder(string.Join(" ", commandArray, 1, commandArray.Length - 1));
                            }

                            settingsXml = WriteKeyValue(commandArray[0].ToLower(), sb.ToString());
                            if (settingsXml != null)
                            {
                                if (localNEObj == null)
                                    Send(settingsXml);
                                else
                                {
                                    LocalExecution(settingsXml);
                                }
                            }
                        }
                        else
                            WriteLine("Invalid notification command. \nPlease use open command to check available commands.\nSyntax: open\n\n");



                        break;
                }
                if (exitFlag)
                    break;
            }

        }

        private static void LoadLocalNotifyEngine()
        {

           // Assembly assembly = Assembly.LoadFrom("CooperAtkins.NotificationServer.NotifyEngine.dll");
            Assembly assembly = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\CooperAtkins.NotificationServer.NotifyEngine.dll");


            // Get the type to use.
            Type protocolWrapper = assembly.GetType("CooperAtkins.NotificationServer.NotifyEngine.NotificationReceiver");
            // Create an instance.
            localNEObj = Activator.CreateInstance(protocolWrapper);
        }

        /// <summary>
        /// method to validate an IP address
        /// using regular expressions. The pattern
        /// being used will validate an ip address
        /// with the range of 1.0.0.0 to 255.255.255.255
        /// </summary>
        /// <param name="addr">Address to validate</param>
        /// <returns></returns>
        public static bool ValidateIpPort(string addr, string port)
        {

            bool validIP = false;
            bool validPort = false;
            IPAddress tempIP;
            UInt16 tempPort = 0;

            if (IPAddress.TryParse(addr, out tempIP))
                validIP = true;
            else
                validIP = false;

            if (UInt16.TryParse(port.ToString(), out tempPort))
                validPort = true;
            else
                validPort = false;

            //return the results
            if (!validIP || !validPort)
                return false;
            else
                return true;
        }
        /// <summary>
        /// method to display help information 
        /// </summary>
        /// <param name="section"></param>
        static void DisplayHelp(string section)
        {
            switch (section.ToLower())
            {
                case "email":
                    WriteLine("email <message>\n");
                    break;
                case "connect":
                    WriteLine("connect <ipaddress> <port>\n");
                    break;
                default:
                    WriteLine("testivr - make a test call. \n");
                    WriteLine("Syntax: testivr <10 digit phone number>\n\n");
                    WriteLine("getivrstatus - check status on a call that was made. \n");
                    WriteLine("Syntax: connect <transaction code from testivr output>>\n\n");

                    WriteLine("connect - to connect to remote machine. \n");
                    WriteLine("Syntax: connect <ipaddress> <port>\n\n");
                    WriteLine("ping - to check whether the remote machine is listening or not. \n");
                    WriteLine("Syntax: ping <ipaddress> <port>\n\n");
                    WriteLine("open - to open the commands file, and edit the configuration.\n");
                    WriteLine("Syntax: open\n\n");
                    WriteLine("report - to show the log file.\n");
                    WriteLine("Syntax: report\n\n");
                    WriteLine("quit/exit - to exit the utility.\n");
                    WriteLine("Syntax: quit\n\n");
                    break;
            }

        }
        /// <summary>
        /// method to write the key value pairs to the console and form xml string to pass to the notification engine
        /// </summary>
        /// <param name="section"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        static string WriteKeyValue(string section, string message)
        {
            IniHelp iniHelp = new IniHelp();
            StringBuilder sb = new StringBuilder();
            string[] keys = iniHelp.IniReadKeys(section);

            if (keys == null)
                return null;

            sb = AddHeader(sb, section, message);
            sb.Append("<notificationSettings>");
            foreach (string s in keys)
            {
                sb.Append("<" + s + ">" + "<![CDATA[" + iniHelp.IniReadValue(section, s).Trim() + "]]></" + s + ">");
                WriteLine(s + ":" + iniHelp.IniReadValue(section, s) + "\n");

            }
            sb.Append("</notificationSettings></notification>");

            return sb.ToString();
        }
        /// <summary>
        /// method to add xml notification data and notification type to the xml string 
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="section"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        static StringBuilder AddHeader(StringBuilder sb, string section, string message)
        {
            sb.Append("<notification ack='true'>");
            sb.Append("<notificationData><![CDATA[" + message + "]]></notificationData>");
            sb.Append("<notificationType><![CDATA[" + section + "]]></notificationType>");
            return sb;

        }
        /// <summary>
        /// method to display the output response to the console
        /// </summary>
        /// <param name="xml"></param>
        static void DisplayOutput(string xml)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);

                foreach (XmlNode xmlNode in xmlDoc.DocumentElement.ChildNodes)
                {
                    switch (xmlNode.Name)
                    {
                        case "IsSucceeded":
                            WriteLine("Succeeded :" + (xmlNode.InnerText.Trim() == "1" || xmlNode.InnerText.Trim().ToLower() == "true" ? "yes" : "no") + "\n");
                            break;
                        case "ResponseContent":
                            WriteLine("Response :" + xmlNode.InnerText + "\n");
                            break;
                        case "IsError":
                            WriteLine("Error :" + (xmlNode.InnerText.Trim() == "1" || xmlNode.InnerText.Trim().ToLower() == "true" ? "yes" : "no") + "\n");
                            break;
                    }

                }
            }
            catch
            {

                WriteLine(xml);
                WriteLine("");
            }
        }
        /// <summary>
        /// method to send the data to notify engine using TDC/IP
        /// </summary>
        /// <param name="data"></param>
        static void Send(string data)
        {
            if (_server == string.Empty || _port == 0)
            {
                WriteLine("\nWARNING! You must specify the server and port to connect and execute notification component. \n");
                DisplayHelp("connect");
                return;
            }

            WriteLine("\nExecuting (Remotehost - " + _server + ":" + _port + ")....\n");


            Console.ForegroundColor = ConsoleColor.White;

            TcpClient client = null;
            NetworkStream stream = null;
            try
            {
                client = new TcpClient(_server, _port);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] buffer = System.Text.Encoding.ASCII.GetBytes(data);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(buffer, 0, buffer.Length);

                // Receive the TcpServer.response.
                // Buffer to store the response bytes.
                buffer = new Byte[1024];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(buffer, 0, buffer.Length);
                responseData = System.Text.Encoding.ASCII.GetString(buffer, 0, bytes);



                WriteLine("\n-------------------------------------\n");
                WriteLine("Result:");
                WriteLine("\n-------------------------------------\n");
                DisplayOutput(responseData);
                WriteLine("\n-------------------------------------\n");

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                WriteLine("\n");
                WriteLine("\nERROR : " + ex.Message);
                WriteLine("\n\n");
            }

            finally
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                // Close everything.
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();

            }
        }
        /// <summary>
        /// method to display and log messages
        /// </summary>
        /// <param name="message"></param>
        /// <param name="isDisplay"></param>
        static void WriteLine(string message, bool isDisplay = true)
        {
            logBuilder.AppendLine(message);
            if (isDisplay)
                Console.Write(message);
        }
        static void LocalExecution(string data)
        {
            Console.WriteLine("\nExecuting (Localhost) ....\n");
            Console.ForegroundColor = ConsoleColor.White;

            try
            {

                object response = localNEObj.GetType().InvokeMember("Execute", BindingFlags.InvokeMethod, null, localNEObj, new object[] { data });


                WriteLine("\n-------------------------------------\n");
                WriteLine("Result:");
                WriteLine("\n-------------------------------------\n");
                DisplayOutput(response.ToString());
                WriteLine("\n-------------------------------------\n");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                WriteLine("______________________________\n");
                WriteLine("\nERROR : " + ex.Message);
                WriteLine("\n______________________________\n");
            }

            Console.ForegroundColor = ConsoleColor.Gray;


        }

        static string[] GetMessageFormat(string section)
        {
            IniHelp iniHelp = new IniHelp();
            string[] keys = iniHelp.IniReadKeys("UtilityMessageFormats");
            string[] msgFormat = new string[2];
            msgFormat[0] = null;
            msgFormat[1] = null;

            foreach (string s in keys)
            {
                if (s.ToLower() == section.ToLower() + "_format")
                {

                    msgFormat[0] = iniHelp.IniReadValue("UtilityMessageFormats", s);
                }
                if (s.ToLower() == section.ToLower() + "_varnames")
                {
                    msgFormat[1] = iniHelp.IniReadValue("UtilityMessageFormats", s);
                }
            }
            return msgFormat;
        }
    }
}

