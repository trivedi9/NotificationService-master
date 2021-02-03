/*
 *  File Name : AlarmType.cs
 *  Author : Vasu 
 *           @ PCC Technology Group LLC
 *  Created Date : 12/04/2010
 *  
 */

namespace CooperAtkins.Interface.Alarm
{
    /// <summary>
    /// Sensor Alarm Type Codes
    /// </summary>
    public struct AlarmType
    {
        public const short SENSOR = 0;
        public const short COMMUNICATIONS = 1;
        public const short BATTERY = 2;
        public const short RESETSENSOR = 100;
        public const short RESETCOMMUNICATIONS = 101;
        public const short RESETBATTERY = 102;
        public const short RESETMODE = 100;

        public static string GetAlarmTypeString(short alarmType)
        {

            string alarmTypeString = string.Empty;
            switch (alarmType)
            {
                case AlarmType.SENSOR: alarmTypeString = "SENSOR"; break;
                case AlarmType.COMMUNICATIONS: alarmTypeString = "COMMUNICATIONS"; break;
                case AlarmType.BATTERY: alarmTypeString = "BATTERY"; break;
                case AlarmType.RESETSENSOR: alarmTypeString = "RESET SENSOR"; break;
                case AlarmType.RESETCOMMUNICATIONS: alarmTypeString = "RESET COMMUNICATIONS"; break;
                case AlarmType.RESETBATTERY: alarmTypeString = "RESET BATTERY"; break;
            }
            return alarmTypeString;
        }
    }

    /// <summary>
    /// Enumeration list for the NotifyTypes
    /// </summary>
    public enum NotifyTypes
    {
        NONE = 0,
        POPUP = 1,
        PAGER = 2,
        EMAIL = 4,
        SOUND = 8,
        SWITCH = 16,
        REMOTEPOPUP = 32,
        MSGBOARD = 64,
        SCRIPT = 128,
        VOICE = 256,// Dial-out voice messaging
        SMS = 512
    }

    /// <summary>
    ///  Enumeration list for the NotifyStatus
    /// </summary>
    public enum NotifyStatus
    {
        PASS = 0,
        FAIL = 1,
        STATUS = -1,
    }

    /// <summary>
    /// Enumeration list for the Severity
    /// </summary>
    public enum Severity
    {
        DEFAULT = -1,
        INFORMATIONAL = 0,
        NORMAL = 1,
        MINOR = 2,
        WARNING = 3,
        MAJOR = 4,
        CRITIAL = 5
    }
}
