
/*
 *  File Name : Common.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/23/2010
 */

namespace CooperAtkins.NotificationClient.Generic
{
    using System;
    using CooperAtkins.Generic;

    public class Common
    {
        /// <summary>
        /// To get offset seconds for the current time zone.
        /// </summary>
        /// <returns></returns>
        public static double TzOffset()
        {
            /* as per the previous application logic we are multiplying with -1*/
            return (-1 * TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalSeconds);
        }


        /// <summary>
        /// calculate minutes difference between two time spans
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static long DateDiff(string interval, DateTime startDate, DateTime endDate)
        {
            return Microsoft.VisualBasic.DateAndTime.DateDiff("n", startDate, endDate);
        }

        /// <summary>
        /// Convert Celsius to Fahrenheit
        /// </summary>
        /// <param name="temperatureCelsius"></param>
        /// <returns></returns>
        public static string CelsiusToFahrenheit(string temperatureCelsius)
        {
            double celsius = System.Double.Parse(temperatureCelsius);
            return ((celsius * 9 / 5) + 32).ToStr();
        }

        /// <summary>
        /// Convert Fahrenheit to Celsius  
        /// </summary>
        /// <param name="temperatureCelsius"></param>
        /// <returns></returns>
        public static string FahrenheitToCelsius(string temperatureFahrenheit)
        {
            double fahrenheit = System.Double.Parse(temperatureFahrenheit);
            return ((fahrenheit - 32) * 5 / 9).ToStr();
        }

        /// <summary>
        /// Construct time string based on Current Alarm Minutes
        /// </summary>
        /// <param name="currrentMinInAlarm"></param>
        /// <returns></returns>
        public static string FormatMinutes(int nMins)
        {
            nMins = Math.Abs(nMins);
            string currentMinStr = "";
            int currentMin = nMins;
            int days, hours, mins;
            mins = currentMin;

            try
            {
                //Get Days
                days = (int)(mins / (60 * 24)); mins = mins - days * 60 * 24;
                currentMinStr = (days > 0) ? days + " days " : "";

                //Get Hours
                hours = (int)(mins / 60); mins = mins - hours * 60;
                currentMinStr = currentMinStr + ((hours > 0) ? hours + " hours " : "");

                //Get Minutes 
                currentMinStr = (mins > 0) ? ((currentMinStr != string.Empty) ? currentMinStr + (mins + " minutes") : (mins + " minutes")) : ((currentMinStr != string.Empty) ? currentMinStr : (mins + " minutes"));

                //LogBook.Write("Missed Comm Final Str: " + currentMinStr.Trim());
                currentMinStr = currentMinStr.Trim();
            }
            catch (Exception ex)
            {
                //Write log
                //LogBook.Write("Missed Comm Minute Error");
            }

            return currentMinStr;
        }

        public const int FORMAT_HTML = 2;
        public const int FORMAT_TEXT = 1;

#if FOUR_FIVE_SYMBOL
        public const string TEMP_TRAK_REG_KEY = "SOFTWARE\\KTG, Inc.\\Intelli-ware\\";
        public const string TEMP_TRAK_DEFAULT_PATH = "C:\\Program Files\\Intelli-ware\\";
#else
        public const string TEMP_TRAK_REG_KEY = "SOFTWARE\\Cooper-Atkins\\TempTrak\\";
        public const string TEMP_TRAK_DEFAULT_PATH = "C:\\Program Files\\TempTrak\\";

#endif
    }
}
