/*
 *  File Name : PagerHelper.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/29/2010
 */

namespace CooperAtkins.NotificationServer.NotifyEngine
{
    using System;
    using KTGUtil;
    using CooperAtkins.Interface.NotifyCom;
    using CooperAtkins.Generic;
    using CooperAtkins.SocketManager;
    using System.Text;
    using System.Threading;

    public class PagerHelper
    {

        private string _snppServer;
        private int _snppPort;
        private string _pagerSubject;
        private string _pagerBody;
        private string _pagerToAddress;
        private string _name;
        private string _lastError;
        string[] COMcmds = new string[7];
        int nCOMcmds = 0;
        int curCOMcmd = 0;
        IOPort comPort = null;
        public string Message { get; set; }

        int currentRespTimeout = 0;
        string modemResponse = string.Empty;
        bool currentCmdSent = false;
        DateTime aTDTsentTime;
        DateTime sendPageTime;
        string currentCmd = string.Empty;
        NotifyComResponse notifyComResponse = null;
        public bool ProcessCompleted { get; set; }

        int callAttempts = 0;

        const int BUSYWAITSECS = 10;
        const int MAXRETRIES = 10;

        public PagerHelper(INotifyObject notifyObject)
        {
            try
            {
                _snppServer = notifyObject.NotifierSettings["SNPPServer"].ToStr();
                _snppPort = notifyObject.NotifierSettings["SNPPPort"].ToInt();
                _pagerSubject = notifyObject.NotifierSettings["PagerSubject"].ToStr();
                _pagerToAddress = notifyObject.NotifierSettings["ToAddress"].ToStr();
                _name = notifyObject.NotifierSettings["Name"].ToStr();
                _pagerBody = notifyObject.NotificationData.ToStr();
            }
            catch (Exception ex)
            {
                /*Debug Object values for reference*/
                LogBook.Debug(notifyObject, this);

                /*Write exception log*/
                LogBook.Write("Error has occurred while retrieving values from notification settings", ex, "CooperAtkins.NotificationServer.NotifyEngine.PagerHelper");
            }
        }


        public PagerHelper(String SNPPServer, Int32 SNPPPort, String Subject, String Address, String Name, String Message)
        {

            _snppServer = SNPPServer;
            _snppPort = SNPPPort;
            _pagerSubject = Subject;
            _pagerToAddress = Address;
            _name = Name;
            _pagerBody = Message;

        }


        /// <summary>
        /// Send Mail
        /// </summary>
        /// <returns></returns>
        public NotifyComResponse Send(INotifyObject notifyObject)
        {
            notifyComResponse = new NotifyComResponse();
            try
            {
                _snppServer = notifyObject.NotifierSettings["SNPPServer"].ToStr();
                _snppPort = notifyObject.NotifierSettings["SNPPPort"].ToInt();
                _pagerSubject = notifyObject.NotifierSettings["PagerSubject"].ToStr();
                _pagerToAddress = notifyObject.NotifierSettings["ToAddress"].ToStr();
                _name = notifyObject.NotifierSettings["Name"].ToStr();
                _pagerBody = notifyObject.NotificationData.ToStr();



                /*Send notification to Pager using SNPP Server*/
                try
                {
                    if (notifyObject.NotifierSettings["DeliveryMethod"].ToInt() != 1)
                    {
                        string phone = notifyObject.NotifierSettings["ToAddress"].ToStr();
                        int pagerDelay = notifyObject.NotifierSettings["PagerDelay"].ToInt();
                        string pagerMessage = notifyObject.NotifierSettings["PagerMessage"].ToStr();
                        string comPortId = notifyObject.NotifierSettings["PagerComPort"].ToStr();

                        Int16 attemptCount = notifyObject.NotifierSettings["AttemptCount"].ToInt16();
                        attemptCount++;
                        notifyObject.NotifierSettings["AttemptCount"] = attemptCount;
                        notifyObject.NotifierSettings["LastSentTime"] = DateTime.Now;

                        Message = " Attempt: " + attemptCount.ToString();

                        sendPageTime = DateTime.Now;
                        /*
                         "2400,n,8,1"
                         1st baud rate
                        2nd parity
                        3rd data bits
                        4th stop bit*/


                        IOPort.ComSettings comSettings = new IOPort.ComSettings();
                        comSettings.BaudRate = 2400;
                        comSettings.ParityBit = System.IO.Ports.Parity.Even;
                        comSettings.DataBits = 8;
                        comSettings.StopBit = System.IO.Ports.StopBits.One;



                        int n = 0;
                        //COMcmds[n++] = "1~+~.1";
                        //COMcmds[n++] = ".3~+~.1";
                        //COMcmds[n++] = ".3~+~.1";

                        //COMcmds[n++] = ".5~ATH~3";          // Hang-up if connected
                        COMcmds[n++] = ".5~ATH~3";          // Hang-up if connected                        
                        COMcmds[n++] = ".5~ATZ~3";          // Reset modem
                        COMcmds[n++] = ".5~ATL0M0~3";       // Disable modem speaker / sounds
                        COMcmds[n++] = ".5~ATE0~3";         //Turn off echoing

                        if (notifyObject.NotifierSettings["COMportInitString"].ToStr().Length > 0)
                        {
                            COMcmds[n++] = ".5~" + notifyObject.NotifierSettings["COMportInitString"].ToStr() + "~1";
                        }

                        //If Len(InitString) Then COMcmds[n] = ".5~" & InitString & "~1";
                        COMcmds[n++] = ".5~ATDT" + phone + new System.Text.StringBuilder().Append(',', pagerDelay).ToString() + pagerMessage + "~40";
                        //COMcmds[n++] = "0~+~.1";
                        //COMcmds[n++] = ".3~+~.1";
                        //COMcmds[n++] = ".3~+~.1";
                        COMcmds[n++] = ".5~ATH~3";
                        nCOMcmds = n;

                        comPort = new IOPort();//for COMM port


                        //ping the com port
                        comPort.Handshake("COM" + comPortId, comSettings, DataRecieveFromCom);


                        SendCommand();
                    }
                    else
                    {

                        //snnpObj = new KTGUtil.SNPP();
                        using (SNPPWrapper snppObj = new SNPPWrapper())
                        {



                            /*SNPP Host*/
                            snppObj.Host = _snppServer;

                            /*SNPP Port Number*/
                            snppObj.Port = _snppPort;

                            /*Subject*/
                            snppObj.Subject = _pagerSubject;

                            /*Body*/
                            snppObj.Body = _pagerBody;

                            /*Pager To Address*/
                            snppObj.SendTo = _pagerToAddress;

                            /*Send information to pager*/
                            int res = snppObj.Send();
                            ProcessCompleted = true;



                            if (res == 0)
                            {
                                /*Record notify response*/
                                notifyComResponse.IsError = false;
                                notifyComResponse.IsSucceeded = true;
                                notifyComResponse.ResponseContent = "Page to [" + _name + "] " + _pagerToAddress + ", sent successfully.";
                            }
                            else
                            {
                                notifyComResponse.IsError = true;
                                notifyComResponse.IsSucceeded = false;
                                notifyComResponse.ResponseContent = "Page to [" + _name + "] " + _pagerToAddress + ", Error:." + snppObj.LastErrorText;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    /*Record notify response*/
                    notifyComResponse.IsError = true;
                    notifyComResponse.IsSucceeded = false;
                    notifyComResponse.ResponseContent = "Page to [" + _name + "] " + _pagerToAddress + ", Failed.";
                    Message += "Page to [" + _name + "] " + _pagerToAddress + ", Failed (" + ex.Message + ").";
                    ProcessCompleted = true;
                    /*Debug Object values for reference*/
                    LogBook.Debug(notifyComResponse, this);

                    /*Write exception log*/
                    LogBook.Write("Error has occurred while sending pager to ." + _pagerToAddress, ex, "CooperAtkins.NotificationServer.NotifyEngine.PagerHelper");
                }
            }
            catch (Exception ex)
            {
                /*Record notify response*/
                notifyComResponse.IsError = true;
                notifyComResponse.IsSucceeded = true;
                notifyComResponse.ResponseContent = "Page to [" + _name + "] " + _pagerToAddress + ", Failed.";
                Message += "Page to [" + _name + "] " + _pagerToAddress + ", Failed (" + ex.Message + ").";
                ProcessCompleted = true;
                /*Debug Object values for reference*/
                LogBook.Debug(notifyComResponse, this);

                /*Write exception log*/
                LogBook.Write("Error has occurred while preparing SNPP setting.", ex, "CooperAtkins.NotificationServer.NotifyEngine.PagerHelper");
            }
            return notifyComResponse;
        }

        public NotifyComResponse Send()
        {
            notifyComResponse = new NotifyComResponse();
            try
            {

                /*Send notification to Pager using SNPP Server*/
                try
                {
                    //snnpObj = new KTGUtil.SNPP();
                    using (SNPPWrapper snppObj = new SNPPWrapper())
                    {

                        /*SNPP Host*/
                        snppObj.Host = _snppServer;

                        /*SNPP Port Number*/
                        snppObj.Port = _snppPort;

                        /*Subject*/
                        snppObj.Subject = _pagerSubject;

                        /*Body*/
                        snppObj.Body = _pagerBody;

                        /*Pager To Address*/
                        snppObj.SendTo = _pagerToAddress;

                        /*Send information to pager*/
                        int res = snppObj.Send();
                        ProcessCompleted = true;



                        if (res == 0)
                        {
                            /*Record notify response*/
                            notifyComResponse.IsError = false;
                            notifyComResponse.IsSucceeded = true;
                            notifyComResponse.ResponseContent = "Page to [" + _name + "] " + _pagerToAddress + ", sent successfully.";
                        }
                        else
                        {
                            notifyComResponse.IsError = true;
                            notifyComResponse.IsSucceeded = false;
                            notifyComResponse.ResponseContent = "Page to [" + _name + "] " + _pagerToAddress + ", Error:." + snppObj.LastErrorText;
                        }
                    }
                }

                catch (Exception ex)
                {
                    /*Record notify response*/
                    notifyComResponse.IsError = true;
                    notifyComResponse.IsSucceeded = false;
                    notifyComResponse.ResponseContent = "Page to [" + _name + "] " + _pagerToAddress + ", Failed.";
                    Message += "Page to [" + _name + "] " + _pagerToAddress + ", Failed (" + ex.Message + ").";
                    ProcessCompleted = true;
                    /*Debug Object values for reference*/
                    LogBook.Debug(notifyComResponse, this);

                    /*Write exception log*/
                    LogBook.Write("Error has occurred while sending pager to ." + _pagerToAddress, ex, "CooperAtkins.NotificationServer.NotifyEngine.PagerHelper");
                }
            }
            catch (Exception ex)
            {
                /*Record notify response*/
                notifyComResponse.IsError = true;
                notifyComResponse.IsSucceeded = true;
                notifyComResponse.ResponseContent = "Page to [" + _name + "] " + _pagerToAddress + ", Failed.";
                Message += "Page to [" + _name + "] " + _pagerToAddress + ", Failed (" + ex.Message + ").";
                ProcessCompleted = true;
                /*Debug Object values for reference*/
                LogBook.Debug(notifyComResponse, this);

                /*Write exception log*/
                LogBook.Write("Error has occurred while preparing SNPP setting.", ex, "CooperAtkins.NotificationServer.NotifyEngine.PagerHelper");
            }
            return notifyComResponse;
        }

        
        private string receivedData = string.Empty;
        private bool dataReceived = false;
        private bool waiting = false;
        private void SendCommandWait()
        {
            Thread.Sleep(1000);
            SendCommand();
        }
        private void SendCommand()
        {
            if (waiting == true)
            {
                var thread = new Thread(SendCommandWait);
                thread.IsBackground = true;
                thread.Start();
                return;
            }

            dataReceived = false;

            LogBook.Write("SendCommand, CmdNo: " + curCOMcmd.ToStr());
            string sBuf = string.Empty;
            string[] tempStr;



            /*Paging commands complete, processing next page.*/
            if (curCOMcmd > nCOMcmds || COMcmds[curCOMcmd] == null)
            {
                LogBook.Write("Paging commands complete, processing next page.");
                Message += "\r\nPaging commands complete";
                comPort.CloseComPort();
                ProcessCompleted = true;
                return;
            }
            sBuf = COMcmds[curCOMcmd];
            LogBook.Write("sBuf:" + sBuf);
            curCOMcmd++;


            tempStr = sBuf.Split('~');

            currentCmd = tempStr[1];

            if (currentCmd.Length > 1)
                currentCmd = currentCmd + Environment.NewLine;

            currentRespTimeout = 0;

            //  Save response timeout value, if specified

            if (tempStr.Length > 1 & (tempStr[2].IsNumericDecimal()))// || tempStr[2].IsNumericDecimal()))
            {
                currentRespTimeout = (int)(tempStr[2].ToDecimal() * 1000);
            }

            if (tempStr[0].ToDecimal() > 0)
            {
                Thread.Sleep((int)(tempStr[0].ToDecimal() * 1000));
            }




            modemResponse = string.Empty;
            currentCmdSent = true;

            /*Close COM port before you send the data, COM port will be automatically opened if it was closed while sending the data.*/
            // comPort.CloseComPort();

            /*send data using COM port*/
            comPort.SerialPortOutput(Encoding.ASCII.GetBytes(currentCmd), false);

            if (currentRespTimeout > 0)
            {
                var waitThread = new Thread(ResponseWait);
                waitThread.IsBackground = true;
                waitThread.Start();
            }

            LogBook.Write("Pager COM send: " + currentCmd);
            //comment//Message += "\r\nPager COM send: " + currentCmd;

            if (currentCmd.Contains("ATDT"))
            {
                aTDTsentTime = DateTime.Now;
            }


            //ProcessNextCommand();
            //var thread = new Thread(ProcessNextCommand);
            //thread.IsBackground = true;
            //thread.Start();
        }

        private void ResponseWait()
        {
            waiting = true;
            LogBook.Write("Sleeping for command " + currentCmd + " " + currentRespTimeout + " milliseconds");
            Thread.Sleep(currentRespTimeout);
            waiting = false;
            //SendCommand();
            //ProcessNextCommand();
            LogBook.Write("Return from sleep for command " + currentCmd);
            if (dataReceived == false)
            {
                LogBook.Write("Timeout waiting for response from pager / modem...continuing " + currentCmd + " " + currentRespTimeout.ToStr() + " is timeout"); //, d_DEBUG, frmPagerStatus.tPagerStatus)
                //comment// Message += "\r\nTimeout waiting for response from pager / modem...continuing " + currentCmd + " " + currentRespTimeout.ToStr() + " is timeout";
                ProcessNextCommand();
            }
        }



        /// <summary>
        /// callback method being called after pinging the serial port
        /// </summary>
        /// <param name="data"></param>
        private void DataRecieveFromCom(string data)
        {
            dataReceived = true;
            receivedData = data.Replace("\r", "").Replace("\n", "").Replace("\0", "");
            Message += "\r\nCommand " + currentCmd.Replace("\r\n", "") + ", Data Received: " + receivedData;

            ProcessNextCommand();
        }

        private void ProcessNextCommand()
        {

            //Thread.Sleep(1 * 1000);

            string data = receivedData;

            if (receivedData == "" || dataReceived == false)
            {
                return;
            }
            //set the last error to the data received from serial port 
            _lastError = data;


            bool bRet;
            bool bContinue;
            //if ((currentPagerInfo == null))
            //{
            //    LogBook.Write("No active page in progress...ignoring pager port input");
            //    return;
            //}

            bContinue = false;
            modemResponse = (modemResponse.Replace("\0", "") + data.Replace("\0", ""));
            LogBook.Write("Pager COMreceive: " + data.Replace("\r", "<CR>").Replace("\n", "<LF>" + "\r\n" + ""));
            if (modemResponse.ToUpper().IndexOf("BUSY") > -1 || modemResponse.ToUpper().IndexOf("DIALTONE") > -1)
            {

                //   Line was busy...re-queue request or check if been a while...some paging
                //   services seem to busy-out the line after sending a page
                if ((DateTime.Now - aTDTsentTime).Seconds > BUSYWAITSECS)
                {
                    //  Been a while...assume went thru
                    //comment//Message += "\r\nPager COM receive: " + modemResponse.Replace(Convert.ToChar(15).ToString(), "<CR>").Replace(Convert.ToChar(10).ToString(), "<LF>") + ", Received BUSY signal but it's been a while...assuming page went thru";
                    LogBook.Write("      Received BUSY signal but it\'s been a while...assuming page went thru");
                    bContinue = true;
                }
                else
                {
                    callAttempts++;
                    if (callAttempts < MAXRETRIES)
                    {
                        LogBook.Write(" Phone line BUSY / No DIALTONE...re-queueing pager request (#" + callAttempts.ToString() + ")");
                        //comment//Message += "\r\n Phone line BUSY / No DIALTONE...re-queueing pager request (#" + callAttempts.ToString() + ")";
                        /*   Re-queue to try again in 30 seconds*/
                        sendPageTime = sendPageTime.AddSeconds(30);
                        // ----------  bRet = queuePage(currentPagerInfo)
                    }
                    else
                    {
                        LogBook.Write("Phone line still BUSY...max retries exceeded...cancelling page request");
                        //comment//Message += "\r\nPhone line still BUSY...max retries exceeded...cancelling page request";
                    }
                    curCOMcmd = nCOMcmds;
                    /* Stop sending commands for current page*/
                    bContinue = true;
                }
            }
            else if (currentCmdSent && (modemResponse.ToUpper().IndexOf("OK") > -1 && ((currentCmd.ToUpper().IndexOf("ATDT") + 1) < 1)))
            {
                //Message += "\r\n" + modemResponse.Replace(Convert.ToChar(13).ToString(), "<CR>").Replace(Convert.ToChar(10).ToString(), "<LF>");
                LogBook.Write("\r\n" + modemResponse.Replace(Convert.ToChar(13).ToString(), "<CR>").Replace(Convert.ToChar(10).ToString(), "<LF>"));
                bContinue = true;
            }
            else if (modemResponse.ToUpper().IndexOf("NO ANSWER") > -1 || modemResponse.ToUpper().IndexOf("NO CARRIER") > -1)
            {
                LogBook.Write("\r\n" + modemResponse.Replace(Convert.ToChar(13).ToString(), "<CR>").Replace(Convert.ToChar(10).ToString(), "<LF>"));
                notifyComResponse.IsError = false;
                notifyComResponse.IsSucceeded = true;
                bContinue = true;
            }

            else if (currentCmd == "+" && modemResponse == "")
            {
                Message += "\r\n Pager COM receive: " + modemResponse;
                LogBook.Write("\r\n Pager COM receive: " + modemResponse);
                bContinue = true;
            }

            if (modemResponse.ToUpper().IndexOf("NO DIALTONE") > -1)
            {
                Message += "\r\n No dial tone, page process still in queue.";
                notifyComResponse.IsError = true;
                bContinue = false;
                LogBook.Write("\r\n NO DIALTONE response received, seems the phone line was not connected.");
                comPort.CloseComPort();
                ProcessCompleted = true;
            }

            receivedData = "";
            if (bContinue)
            {
                SendCommand();
            }
        }
    }
}
