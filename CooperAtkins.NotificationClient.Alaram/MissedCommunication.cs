/*
 *  File Name : MissedCommunication.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 12/24/2010
 */

namespace CooperAtkins.NotificationClient.Alarm
{
    using System;
    using System.Text;
    using CooperAtkins.NotificationClient.Alarm.DataAccess;
    using EnterpriseModel.Net;
    using CooperAtkins.Generic;
    using CooperAtkins.Interface.Alarm;

    public class MissedCommunication
    {
        private const double puckIntervalSecs = 7.5;

        /// <summary>
        /// Check for missed communication and build the return the missed communication data
        /// </summary>
        public string[] GetMissedCommunicationData()
        {
            /*Store the missed communication sensor information and sensor count in a string array*/
            string[] missedCommContent = new string[2];
            StringBuilder missedCommStr = new StringBuilder();
            /*To hold missed communication sensor count*/
            int missedCommSensorCount = 0;

            /*Get all the missed communication
             * Loop through each missed communication to get the detailed information of each missed communication sensor*/
            MissedCommunicationList missedCommunicationList = GetMissedCommunicationList();
            if (missedCommunicationList.Count != 0)
            {
                /*Create HTML table to hold the missed communication information*/
                missedCommStr.Append("<table border='1'>");

                /*Create Table Column Header */
                string tableHeader = "<tr><td><strong>Group Name</strong></td>" +
                                          "<td><strong>Puck Name</strong></td>" +
                                          "<td><strong>Sensor Type</strong></td>" +
                                          "<td><strong>Factory ID</strong></td>" +
                                          "<td><strong>UTID</strong></td>" +
                                          "<td><strong>Probe</strong></td>" +
                                          "<td><strong>Last Contact Reading</strong></td></tr>";

                /*Add the table header to the string builder*/
                missedCommStr.Append(tableHeader);

                /*Loop through MissedCommunicationList*/
                foreach (MissedComm missedCommunication in missedCommunicationList)
                {
                    /*Check whether the sensor is in miss communication range or not.*/
                    if (HasMissedCom(missedCommunication.Interval, missedCommunication.LogIntervalMins, missedCommunication.LastContact))
                    {
                        /*Append the missed communication information to the table*/
                        missedCommStr.Append(GetHTMLMissedComData(missedCommunication));
                        /*Missed Communication sensor count*/
                        missedCommSensorCount = missedCommSensorCount + 1;


                        AlarmObject alarmObject = new AlarmObject();
                        alarmObject.AlarmType = AlarmType.COMMUNICATIONS;




                        AlarmQueue.AlarmObjectQueue.Enqueue(alarmObject);
                    }
                }

                /*Close the HTML table*/
                missedCommStr.Append("</table>");
            }
            missedCommContent[0] = missedCommStr.ToStr();
            missedCommContent[1] = missedCommSensorCount.ToStr();

            return missedCommContent;
        }

        /// <summary>
        /// Get missed communication sensor list
        /// </summary>
        /// <returns></returns>
        private MissedCommunicationList GetMissedCommunicationList()
        {
            MissedCommunicationList missedCommunicationList = null;
            try
            {
                /*Create MissedCommunicationList instance*/
                missedCommunicationList = new MissedCommunicationList();

                /*Create empty Criteria object*/
                Criteria criteria = new Criteria();

                /*Execute to get the missed communication list*/
                missedCommunicationList = missedCommunicationList.Load(criteria);

            }
            catch (Exception ex)
            {
                /*Debug Object values for reference*/
                LogBook.Debug(missedCommunicationList, this);

                /*Write exception log*/
                LogBook.Write("Error has occurred while fetching missed communication data.", ex, "CooperAtkins.NotificationClient.NotificationComposer");
            }
            finally
            {
                missedCommunicationList.Dispose();
            }
            return missedCommunicationList;
        }

        /// <summary>
        /// Check whether the sensor is in miss communication range or not.
        /// </summary>
        /// <returns></returns>
        private bool HasMissedCom(int intervalVal, int logIntervalMins, DateTime lastContact)
        {
            double lastPacketSecs = 0;
            double chkInterval = 0;
            bool hasCommError = false;

            /*Calculate the difference between current time and last contact time to get the missed communication time interval*/
            System.TimeSpan timeSpam = DateTime.Now - lastContact.ToLocalTime();

            /*Get the last packet seconds*/
            lastPacketSecs = timeSpam.TotalSeconds;

            /*If intervalVal < 0, then never report a missed communication*/
            if (logIntervalMins > 0 && intervalVal >= 0)
                chkInterval = logIntervalMins * 60;
            else
            {
                if (intervalVal > 0)
                    chkInterval = intervalVal * puckIntervalSecs;
                else
                    chkInterval = 0;
            }
            if (chkInterval > 0 && lastPacketSecs > chkInterval * 2.5)  //Show non-Communication at 2.5 * expected interval
                hasCommError = true;
            //overdueSecs = lastPacketSecs - chkInterval

            return hasCommError;
        }

        /// <summary>
        /// Generate HTML table with the missed communication data
        /// </summary>
        /// <returns></returns>
        private string GetHTMLMissedComData(MissedComm missedComm)
        {
            /*Fill the column data with MissedComm object*/
            string missedCommData = "<tr>" +
                                "<td>" + missedComm.GroupName + "</td>" +
                                "<td>" + missedComm.PuckName + "</td>" +
                                "<td>" + missedComm.SensorType + "</td>" +
                                "<td>" + missedComm.FactoryID + "</td>" +
                                "<td>" + missedComm.UTID + "</td>" +
                                "<td>" + missedComm.Probe.ToStr() + "</td>" +
                                "<td>" + missedComm.LastContact.ToStr() + "</td>" +
                                "</tr>";
            return missedCommData;
        }

    }
}
