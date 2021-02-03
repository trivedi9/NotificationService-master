
/*
*  File Name : AlarmHelper.cs
*  Author : Aditya
*  @ PCC Technology Group LLC
*  Created Date : 11/24/2010
*/


namespace CooperAtkins.NotificationClient.Generic
{
    using System.Linq;
    using System.Collections.Generic;
    using CooperAtkins.Interface.Alarm;
    using CooperAtkins.Generic;
    

    public static class AlarmHelper
    {
        //to maintain the list of db connections, in multi db scenario
        /// <summary>
        /// Get the sensor information.
        /// </summary>
        /// <param name="sensorType"></param>
        /// <returns></returns>
        public static SensorTypeInfo GetSensorTypeInfo(string sensorType)
        {
            DataAccess.SensorTypeInfoList sensorTypeInfoList = new DataAccess.SensorTypeInfoList();
            sensorTypeInfoList.Load(null);
            sensorTypeInfoList.Dispose();

            //search for the sensor type object by sensorType
            var list = from obj in sensorTypeInfoList where obj.SensorType == sensorType select obj;

            //List will always contain only one record, return that object
            if (list.ToList<SensorTypeInfo>().Count == 0)
                return null;
            else
                return list.ToList<SensorTypeInfo>()[0];
        }

        /// <summary>        
        /// Check for the temperature type
        /// </summary>
        /// <param name="sensorType"></param>
        /// <returns></returns>
        public static bool IsTempSensor(string sensorType)
        {
            bool returnValue = false;
            sensorType = sensorType.Trim().ToUpper();
            if (sensorType.IndexOf("TEMP") > -1 || sensorType.IndexOf("THERM") > -1 || sensorType.IndexOf("NAFEM:" + Measure.UOMfahrenheit.ToString()) > -1 || sensorType.IndexOf("NAFEM:" + Measure.UOMcelsius.ToString()) > -1)
            {
                returnValue = true;
            }
            else if (sensorType.Length > 0)
            {
                SensorTypeInfo sensorTypeInfo = GetSensorTypeInfo(sensorType);
                if (sensorTypeInfo != null)
                {
                    if (sensorTypeInfo.isTemp)
                        returnValue = true;

                    sensorTypeInfo = null;
                }
            }
            return returnValue;
        }


        /// <summary>
        /// Check for the contact sensor 
        /// </summary>
        /// <param name="sensorType"></param>
        /// <returns></returns>
        public static bool IsContactSensor(string sensorType)
        {
            bool returnValue = false;
            sensorType = sensorType.ToStr().Trim().ToUpper();
            if ((sensorType.IndexOf("CONTACT") + 1 + sensorType.IndexOf("SECUR") + 1) > 0 || sensorType == "NAFEM:" + Measure.UOM_CustomLast.ToString())
            {
                returnValue = true;
            }
            return returnValue;
        }

        /// <summary>
        /// Format the basic alarm information.
        /// </summary>
        /// <returns></returns>
        public static string BasicAlarmInformation(AlarmObject alarmObject)
        {
            string info = string.Empty;
            /*In case of missed communication skip info*/
            if (alarmObject.FactoryID.ToStr() != string.Empty && alarmObject.SensorAlarmID.ToStr() != string.Empty)
                info = " SensorID: " + alarmObject.FactoryID.ToString().Trim() + ", SensorAlarmID: " + alarmObject.SensorAlarmID.ToString().Trim();

            return info;
        }

        /// <summary>
        /// Mark the notification process alarm object as completed.
        /// </summary>
        /// <param name="alarmObject"></param>
        public static void MarkAsCompleted(AlarmObject alarmObject, string msg)
        {
            LogBook.Write(AlarmHelper.BasicAlarmInformation(alarmObject) + ","+msg+" marking alarm to complete process, object will be removed from current alarm process queue.");
            alarmObject.IsProcessCompleted = true;
        }

    }
}
