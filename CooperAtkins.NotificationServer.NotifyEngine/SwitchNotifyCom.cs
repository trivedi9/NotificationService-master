/*
 *  File Name : SwitchNotifyCom.cs
 *  Author : Pradeep
 *  @ PCC Technology Group LLC
 *  Created Date : 11/30/2010
 */

namespace CooperAtkins.NotificationServer.NotifyEngine
{
    using System;
    using CooperAtkins.Generic;
    using System.ComponentModel.Composition;
    using CooperAtkins.Interface.NotifyCom;

    [Export(typeof(INotifyCom))]
    public class SwitchNotifyCom : INotifyCom
    {
        #region INotifyCom Members

        public NotifyComResponse Invoke(INotifyObject notifyObject)
        {
            NotifyComResponse notifyComResponse = new NotifyComResponse();
            LogBook.Write("Executing Invoke method");
            RelaySwitch relaySwitch = null;
            try
            {


                //set the configuration, get the relay switch object 
                relaySwitch = GetRelaySwitchObject(notifyObject);
                //call the send data method 

                notifyComResponse.IsSucceeded = true;

                if (notifyObject.NotifierSettings["IsDynamicNotificationCleared"].ToBoolean())
                {
                    relaySwitch.ClearData(notifyObject.NotifierSettings["SensorAlarmID"].ToStr());
                    notifyComResponse.ResponseContent = "Clearing notification from " + relaySwitch.ResponseContent;
                }
                else
                {
                    relaySwitch.SendData();

                    notifyComResponse.ResponseContent = "Notification to [" + notifyObject.NotifierSettings["Name"].ToStr() + "] " + (notifyObject.NotifierSettings["SwitchInfo"].ToStr().Contains("NET") == true ? "IP:" : " ") + notifyObject.NotifierSettings["SwitchInfo"].ToStr().Replace("NET:", "") + ", Current state " + relaySwitch.CurrentState + ", New State:" + relaySwitch.NewState + ", sent successfully.";
                }
            }
            catch (Exception ex)
            {
                notifyComResponse = new NotifyComResponse();
                notifyComResponse.IsSucceeded = false;
                notifyComResponse.IsError = true;
                if (notifyObject.NotifierSettings["IsDynamicNotificationCleared"].ToBoolean())
                {
                    notifyComResponse.ResponseContent = "Clearing notification failed for switch(s), " + relaySwitch.ResponseContent;
                }
                else
                {
                    notifyComResponse.ResponseContent = "Notification to [" + notifyObject.NotifierSettings["Name"].ToStr() + "] " + (notifyObject.NotifierSettings["SwitchInfo"].ToStr().Contains("NET") == true ? "IP:" : "COM") + notifyObject.NotifierSettings["SwitchInfo"].ToStr().Replace("NET:", "") + ", Failed.";
                }

                LogBook.Write(ex, "NotifyEngine-SwicthNotifyCom");
            }
            return notifyComResponse;
        }

        public void UnLoad()
        {

        }

        #endregion

        #region HelperMethods



        private RelaySwitch GetRelaySwitchObject(INotifyObject notifyObject)
        {
            //create relay switch object
            RelaySwitch relaySwitch = new RelaySwitch();
            try
            {
                //populate relay the switch settings from notifier settings hashtable
                string[] switchInfo = notifyObject.NotifierSettings["SwitchInfo"].ToStr().Split(':');

                if (switchInfo.Length == 0)
                    return relaySwitch;

                //set the LPT/COM/Net settings for the switch
                switch (switchInfo[0].ToLower())
                {
                    case "lpt1":
                        relaySwitch.IoAddress = (int)0x378;
                        break;
                    case "lpt2":
                        relaySwitch.IoAddress = (int)0x278;
                        break;
                    case "lpt3":
                        relaySwitch.IoAddress = (int)0x3BC;
                        break;
                    case "net":
                        relaySwitch.IsNetworkAttached = true;
                        relaySwitch.IpAddress = switchInfo[1];
                        relaySwitch.IpPort = switchInfo[2].ToInt();
                        relaySwitch.IoAddress = 0;
                        break;
                    default:
                        int i = 0;
                        if (int.TryParse(switchInfo[0], out i))
                        {
                            relaySwitch.IoAddress = switchInfo[0].ToInt();
                        }
                        else
                        {
                            if (switchInfo[0].Substring(0, 3) == "COM")
                            {
                                relaySwitch.ComPort = switchInfo[0];
                                relaySwitch.ComSettings = new SocketManager.IOPort.ComSettings();
                                relaySwitch.ComSettings.BaudRate = Convert.ToInt32(switchInfo[1].Split(',')[0]);
                                relaySwitch.ComSettings.DataBits = Convert.ToInt32(switchInfo[1].Split(',')[2]);
                                //switch comm settings 'n' represents even parity, in the database it is stored as 'N'
                                if (switchInfo[1].Split(',')[1].ToLower() == "n")
                                    relaySwitch.ComSettings.ParityBit = System.IO.Ports.Parity.Even;
                                //relaySwitch.ComSettings.ParityBit = (System.IO.Ports.Parity)Enum.Parse(typeof(System.IO.Ports.Parity), switchInfo[1].Split(',')[1]);
                                relaySwitch.ComSettings.StopBit = (System.IO.Ports.StopBits)Enum.Parse(typeof(System.IO.Ports.StopBits), switchInfo[1].Split(',')[3]);

                            }
                            else
                            {
                                LogBook.Write("Error loading switch" + notifyObject.NotifierSettings["SwitchInfo"]);
                                return relaySwitch;
                            }
                        }
                        break;
                }
                if (notifyObject.NotifierSettings["SensorAlarmID"] != null)
                    relaySwitch.SensorAlarmID = notifyObject.NotifierSettings["SensorAlarmID"].ToString();

                if (notifyObject.NotifierSettings["FactoryID"] != null)
                    relaySwitch.FactoryID = notifyObject.NotifierSettings["FactoryID"].ToString();

                if (notifyObject.NotifierSettings["IsEnabled"] != null)
                    relaySwitch.IsEnabled = notifyObject.NotifierSettings["IsEnabled"].ToBoolean();
                if (notifyObject.NotifierSettings["Name"] != null)
                    relaySwitch.SwitchName = notifyObject.NotifierSettings["Name"].ToString();

                relaySwitch.SwitchBitMask = Convert.ToInt16(notifyObject.NotificationData);
                relaySwitch.LastValue = GetSwitchValue(Convert.ToInt16(notifyObject.NotificationData));

                if (notifyObject.NotifierSettings["IsDynamicNotificationCleared"] != null)
                    relaySwitch.IsDynamicNotificationCleared = notifyObject.NotifierSettings["IsDynamicNotificationCleared"].ToBoolean();
                else
                    relaySwitch.IsDynamicNotificationCleared = null;
            }
            catch (Exception ex)
            {
                LogBook.Write("  *** Error in GetRelaySwitchObject method : " + ex.Message);
            }
            return relaySwitch;

        }
        /// <summary>
        /// calculate bit mask value for the relay switch,
        /// if the transmitter is configured to relay 1, we get a value as 1, if 3 we get value 3
        /// if 1 and 2 we get value 3, if 3 and 5 are configured we get value 2 pow 2 + 2 pow 5 --> 36
        /// </summary>
        /// <param name="switchBitmask"></param>
        /// <returns></returns>
        private int GetSwitchValue(int switchBitmask)
        {
            int value = 0;
            for (int i = 0; i <= 7; i++)
            {
                if ((switchBitmask & Convert.ToInt32(Math.Pow(2, i))) != 0)
                    value = value + (int)Math.Pow(2, i);
            }
            return value;
        }

        #endregion
    }
}
