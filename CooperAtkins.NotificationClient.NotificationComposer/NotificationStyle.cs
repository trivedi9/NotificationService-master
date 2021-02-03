/*
 *  File Name : NotificationStyle.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/24/2010
 */

namespace CooperAtkins.NotificationClient.NotificationComposer
{

    using System;
    using CooperAtkins.Generic;
    using CooperAtkins.Interface.Alarm;
    using CooperAtkins.NotificationClient.Generic;
    using CooperAtkins.NotificationClient.Generic.DataAccess;
    using Microsoft.VisualBasic;

    public class NotificationStyle
    {

        /// <summary>
        /// Get Format String
        /// </summary>
        /// <param name="alarmObject"></param>
        /// <param name="languageID"></param>
        /// <param name="formatType"></param>
        /// <returns></returns>
        public string GetFormatString(Interface.Alarm.AlarmObject alarmObject, int languageID, string formatType)
        {
            string sensorType = "", frmtstring = "";
            int i = 0, formatID = 0;
            //Sensor Type
            sensorType = (alarmObject.AlarmType == AlarmType.COMMUNICATIONS ? "MISS" : alarmObject.SensorType);
            try
            {
                while ((sensorType != "" && i < 2))
                {
                    if (i > 0)
                        sensorType = "";
                    //Get Sensor FomratID
                    while (true)
                    {
                        //Individual message format
                        formatID = GetFormatID(alarmObject, formatType, 1);
                        if (formatID != 0)
                            break;
                        //Next, check for Group default message format...
                        formatID = GetFormatID(alarmObject, formatType, 2);
                        if (formatID != 0)
                            break;
                        //Finally, get default message format...
                        formatID = GetFormatID(alarmObject, formatType, 3);
                        break;
                    }
                    if (formatID != 0)
                    {
                        //Get Sensor format string.
                        using (FormatString formatString = new FormatString())
                        {
                            formatString.FormatID = formatID;
                            formatString.LanguageID = languageID;
                            formatString.SensorType = sensorType;
                            formatString.Execute(formatString);
                            frmtstring = formatString.ComposeString;
                            if (frmtstring.ToStr() != "")
                                break;
                        }
                    }
                    i = i + 1;
                }
            }
            catch (Exception ex)
            {
                /*Write Log*/
                LogBook.Write("Error occurred getting the format string.", ex, "CooperAtkins.NotificationClient.NotificationComposer.NotificationStyle");
            }

            //If FormatString is empty set default value.
            if (frmtstring.ToStr() == string.Empty)
                frmtstring = GetDefaultFormatString(alarmObject, languageID, formatType);
            else
                return frmtstring;

            return frmtstring;
        }

        /// <summary>
        /// Get default format string
        /// </summary>
        /// <param name="alarmObject"></param>
        /// <param name="languageID"></param>
        /// <param name="formatType"></param>
        /// <returns></returns>
        private string GetDefaultFormatString(Interface.Alarm.AlarmObject alarmObject, int languageID, string formatType)
        {
            string formatString = "";

            if (alarmObject.AlarmType == AlarmType.COMMUNICATIONS)
                formatString = "MISSED COMM: %%TYPE%% %%NAME%% In %%GROUP%% (%%TimeOutOfRange%%)";
            else if (AlarmHelper.IsContactSensor(alarmObject.SensorType))
            {
                //Check logic for vbTextCompare in c#

                /*Body Pager*/
                if (formatType.IndexOf("BodyPager") >= 0)
                    formatString = "";

                /*Notify Tree*/
                else if (formatType.IndexOf("NotifyTree") >= 0)
                    formatString = "";

                /*Email Body*/
                else if (formatType.IndexOf("EmailBody") >= 0)
                    formatString = "";

                /*Pager Subject*/
                else if (formatType.IndexOf("SubjectPager") >= 0)
                    formatString = "TempTrak ALERT - %%NAME%% In %%GROUP%%: %%VALUE%% (%%THRESHOLDMINS%%)";

                /*Email Subject*/
                else if (formatType.IndexOf("EmailSubject") >= 0)
                    formatString = "TempTrak ALERT - %%NAME%% In %%GROUP%%: %%VALUE%% (%%THRESHOLDMINS%%)";

                /*Net send*/
                else if (formatType.IndexOf("NetSend") >= 0)
                    formatString = "!! TEMP TRAK ALERT !!\n%%NAME%% In %%GROUP%%: %%VALUE%% (%%THRESHOLDMINS%%)";

                /*Popup*/
                else if (formatType.IndexOf("Popup") >= 0)
                    formatString = "!! TEMP TRAK ALERT !!\n%%NAME%% In %%GROUP%%: %%VALUE%% (%%THRESHOLDMINS%%)";

                /*Message Board*/
                else if (formatType.IndexOf("Board") >= 0)
                    formatString = "WARNING: %%NAME%% In %%GROUP%%: at %%VALUE%% (%%THRESHOLDMINS%%)";

                /*Default*/
                else
                    formatString = "TempTrak ALERT - %%NAME%% In %%GROUP%%: %%VALUE%% (%%THRESHOLDMINS%%)";
            }
            else
            {
                /*Pager Body*/
                if (formatType.IndexOf("BodyPager") >= 0)
                    formatString = "";

                /*Notify Tree*/
                else if (formatType.IndexOf("NotifyTree") >= 0)
                    formatString = "";

                /*Email Body*/
                else if (formatType.IndexOf("EmailBody") >= 0)
                    formatString = "";

                /*Pager Subject*/
                else if (formatType.IndexOf("SubjectPager") >= 0)
                    formatString = "TempTrak ALERT - %%NAME%% In %%GROUP%% at %%VALUE%% (%%MIN%%-%%MAX%%)";

                /*Email Subject*/
                else if (formatType.IndexOf("EmailSubject") >= 0)
                    formatString = "TempTrak ALERT - %%NAME%% In %%GROUP%%: at %%VALUE%% (Normal Range %%MIN%%-%%MAX%%)";

                /*Net send*/
                else if (formatType.IndexOf("NetSend") >= 0)
                    formatString = "!! TEMP TRAK ALERT !!\n%%NAME%% In %%GROUP%%: at %%VALUE%% (Normal Range %%MIN%%-%%MAX%%)";

                /*Popup*/
                else if (formatType.IndexOf("Popup") >= 0)
                    formatString = "!! TEMP TRAK ALERT !!\n%%NAME%% In %%GROUP%%: at %%VALUE%% (Normal Range %%MIN%%-%%MAX%%)";

                /*Message Board*/
                else if (formatType.IndexOf("Board") >= 0)
                    formatString = "WARNING: %%NAME%% In %%GROUP%%: at %%VALUE%% (Range %%MIN%%-%%MAX%%)";

                /*Default*/
                else
                    formatString = "TempTrak ALERT - %%NAME%% In %%GROUP%%: at %%VALUE%% (Range %%MIN%%-%%MAX%%)";
            }
            return formatString;
        }

        /// <summary>
        /// Get E-Mail body
        /// </summary>
        /// <param name="alarmObject"></param>
        /// <returns></returns>
        public string GetEmailBody(Interface.Alarm.AlarmObject alarmObject)
        {
            string emilBodyStr = "";
            string iUOM = "", sUOM = "", sURL = "";
            string[] alarmVals = new string[5];

            //Set Alarm values
            alarmVals = FormatSensorValues(alarmObject);
            if (alarmVals.Length >= 4)
            {
                sUOM = alarmVals[3];
                iUOM = alarmVals[4];
            }

            //sURL--> Set local host name //Pending

            /*Get Email Body*/
            using (DataAccess.EmailBody emailBody = new DataAccess.EmailBody())
            {
                emailBody.EmailFormat = 0; //1=Text, 2=HTML, 0=Default (HTML)
                emailBody.UTID = alarmObject.UTID;
                emailBody.Probe = alarmObject.Probe;
                emailBody.LogTime = (DateTime?)alarmObject.AlarmTime.MinDateToDateTime();
                emailBody.TzOffset = Convert.ToDecimal(Common.TzOffset() / 60);
                emailBody.SensorReading = alarmObject.Value;
                emailBody.AlarmStartTime = (DateTime?)alarmObject.AlarmStartTime.MinDateToDateTime();
                emailBody.AlarmProfileRecID = 0;
                emailBody.ThresholdMins = alarmObject.CurrentAlarmMinutes;
                emailBody.CondThresholdMins = alarmObject.Threshold.ToInt();
                emailBody.CondMinValue = alarmObject.AlarmMinValue.ToDecimal();
                emailBody.CondMaxValue = alarmObject.AlarmMaxValue.ToDecimal();
                emailBody.IncludeHistory = 0; //NULL in VB
                emailBody.ValuesUOM = iUOM.ToInt(); //-- 0 = Unknown / None,9 = Celsius,31  = Fahrenheit
                emailBody.HTMLlink = sURL;
                emailBody.Severity = alarmObject.Severity;

                /*Execute*/
                emailBody.Execute(emailBody);
                /*E-Mail Body*/
                emilBodyStr = emailBody.Body;
            }
            return emilBodyStr;
        }

        /// <summary>
        /// Get format string
        /// </summary>
        /// <param name="formatString"></param>
        /// <param name="alarmObject"></param>
        /// <returns></returns>
        public string SubstituteFormatString(string formatString, Interface.Alarm.AlarmObject alarmObject)
        {
            string temperatureReading = "Reading OK", sUCOM = "", sValue;
            decimal dMin, dMax;
            string[] aValues = new string[4];

            /*Set Alarm Values*/
            //Check for contact sensor
            if (AlarmHelper.IsContactSensor(alarmObject.SensorType))
                temperatureReading = (alarmObject.Value == 0) ? "CLOSED" : "OPEN";
            else
                temperatureReading = (alarmObject.Value < alarmObject.AlarmMinValue) ? "LOW" : "HIGH";

            dMin = alarmObject.AlarmMinValue;
            dMax = alarmObject.AlarmMaxValue;
            sValue = alarmObject.DisplayValue;
            sUCOM = "";


            if (alarmObject.AlarmType == AlarmType.COMMUNICATIONS)
            {
                sValue = alarmObject.CurrentAlarmMinutes.ToString();
            }
            else
            {
                aValues = FormatSensorValues(alarmObject);
                if (aValues.Length >= 3)
                {
                    sValue = aValues[0];
                    dMin = aValues[1].ToDecimal();
                    dMax = aValues[2].ToDecimal();
                    sUCOM = aValues[3];
                }
            }

            //Substitute values for format string
            //formatString = formatString.Replace("\\n", Environment.NewLine);
            //formatString = formatString.Replace("\\n", "\\n");
            //formatString = formatString.Replace("\n", Environment.NewLine);
             formatString = Microsoft.VisualBasic.Strings.Replace(formatString, "%%ID%%", FormatFactoryID(alarmObject.FactoryID), 1, -1, Constants.vbTextCompare);
            formatString = Microsoft.VisualBasic.Strings.Replace(formatString, "%%NAME%%", alarmObject.ProbeName, 1, -1, Constants.vbTextCompare);
            formatString = Microsoft.VisualBasic.Strings.Replace(formatString, "%%NAME2%%", alarmObject.ProbeName2, 1, -1, Constants.vbTextCompare);
            formatString = Microsoft.VisualBasic.Strings.Replace(formatString, "%%TYPE%%", (alarmObject.AlarmType == AlarmType.COMMUNICATIONS.ToInt16()) ? alarmObject.SensorClass : alarmObject.SensorType, 1, -1, Constants.vbTextCompare);
            formatString = Microsoft.VisualBasic.Strings.Replace(formatString, "%%VALUE%%", sValue, 1, -1, Constants.vbTextCompare);
            formatString = Microsoft.VisualBasic.Strings.Replace(formatString, "%%MIN%%", dMin.ToStr(), 1, -1, Constants.vbTextCompare);
            formatString = Microsoft.VisualBasic.Strings.Replace(formatString, "%%MAX%%", dMax.ToStr(), 1, -1, Constants.vbTextCompare);
            formatString = Microsoft.VisualBasic.Strings.Replace(formatString, "%%UOM%%", sUCOM.ToStr(), 1, -1, Constants.vbTextCompare);
            formatString = Microsoft.VisualBasic.Strings.Replace(formatString, "%%TIME%%", alarmObject.AlarmTime.ToLocalTime().ToString(), 1, -1, Constants.vbTextCompare);
            formatString = Microsoft.VisualBasic.Strings.Replace(formatString, "%%GMTTIME%%", alarmObject.AlarmTime.ToStr(), 1, -1, Constants.vbTextCompare);
            formatString = Microsoft.VisualBasic.Strings.Replace(formatString, "%%NOW%%", DateTime.Now.ToStr(), 1, -1, Constants.vbTextCompare);
            formatString = Microsoft.VisualBasic.Strings.Replace(formatString, "%%GROUP%%", alarmObject.GroupName, 1, -1, Constants.vbTextCompare);
            formatString = Microsoft.VisualBasic.Strings.Replace(formatString, "%%HOTCOLD%%", temperatureReading, 1, -1, Constants.vbTextCompare);
            formatString = Microsoft.VisualBasic.Strings.Replace(formatString, "%%CORF%%", (alarmObject.IsCelsius) ? "C" : "F", 1, -1, Constants.vbTextCompare);
            formatString = Microsoft.VisualBasic.Strings.Replace(formatString, "%%THRESHOLD%%", alarmObject.Threshold.ToStr(), 1, -1, Constants.vbTextCompare);
            formatString = Microsoft.VisualBasic.Strings.Replace(formatString, "%%THRESHOLDMINS%%", alarmObject.CondThresholdMins.ToStr(), 1, -1, Constants.vbTextCompare);
            formatString = Microsoft.VisualBasic.Strings.Replace(formatString, "%%TimeOutOfRange%%", Common.FormatMinutes(GetCurrentMinInAlarm(alarmObject)), 1, -1, Constants.vbTextCompare);
            formatString = Microsoft.VisualBasic.Strings.Replace(formatString, "%%AlarmStartTime%%", alarmObject.AlarmStartTime.ToLocalTime().ToString(), 1, -1, Constants.vbTextCompare);
            formatString = Microsoft.VisualBasic.Strings.Replace(formatString, "%%ESCALATION%%", (alarmObject.IsEscalationNotification ? "** ESCALATION NOTIFICATION **" : ((alarmObject.IsFailsafeEscalationNotification) ? "** FAILSAFE ESCALATION NOTIFICATION **" : "")), 1, -1, Constants.vbTextCompare);
            formatString = Microsoft.VisualBasic.Strings.Replace(formatString, "%%SEVERITY%%", alarmObject.Severity.ToStr(), 1, -1, Constants.vbTextCompare);
            // Does this put in the actual current value?
            if (formatString != null)
            {
                if (formatString.Contains("%%CURRENTVALUE%%", StringComparison.OrdinalIgnoreCase))
                {
                    // Need to hit datbase for current values

                    SensorCurrentStatus sensorCurrentStatus = new SensorCurrentStatus();
                    try
                    {
                        LogBook.Write("Getting current value and time for sensor");
                        sensorCurrentStatus.UTID = alarmObject.UTID;
                        sensorCurrentStatus.Probe = alarmObject.Probe;
                        sensorCurrentStatus.Execute();
                    }
                    catch (Exception ex)
                    {
                        LogBook.Write(ex, "CooperAtkins.NotificationClient.NotificationComposer.NotificationStyle", ErrorSeverity.High);
                    }
                    finally
                    {
                        sensorCurrentStatus.Dispose();
                    }
                    formatString = Microsoft.VisualBasic.Strings.Replace(formatString, "%%CURRENTVALUE%%", sensorCurrentStatus.CurrentValue.ToStr(), 1, -1, Constants.vbTextCompare);
                    formatString = Microsoft.VisualBasic.Strings.Replace(formatString, "%%CURRENTVALUETIME%%", sensorCurrentStatus.CurrentTime.ToLocalTime().ToString(), 1, -1, Constants.vbTextCompare);

                }
            }



            return formatString;
        }


        /// <summary>
        /// Format the sensor values
        /// </summary>
        /// <param name="alarmObject"></param>
        /// <returns></returns>
        private string[] FormatSensorValues(Interface.Alarm.AlarmObject alarmObject)
        {
            string[] aValues = new string[5];

            //Define local variables
            string sValue, sMin, sMax, sUOM;
            decimal iUOM, dUOM;
            int nDecimals;
            string[] aParts = new string[5];

            //Assign variables
            sUOM = "";
            sValue = alarmObject.DisplayValue.ToStr();
            sMin = alarmObject.AlarmMinValue.ToStr();
            sMax = alarmObject.AlarmMaxValue.ToStr();
            iUOM = 0;
            nDecimals = 1;

            switch (((alarmObject.SensorType.ToStr() != string.Empty) && (alarmObject.SensorType.ToStr().Length >= 4)) ? alarmObject.SensorType.Substring(0, 4) : "")
            {
                case "TEMP":
                case "THER":
                    sUOM = (alarmObject.IsCelsius) ? "C" : "F";
                    if (alarmObject.IsCelsius)
                    {

                        sValue = Common.FahrenheitToCelsius(alarmObject.Value.ToStr());
                        sMin = Common.FahrenheitToCelsius(alarmObject.AlarmMinValue.ToStr());
                        sMax = Common.FahrenheitToCelsius(alarmObject.AlarmMaxValue.ToStr());
                        iUOM = Measure.UOMcelsius.ToInt();
                    }
                    else
                        iUOM = Measure.UOMfahrenheit.ToInt();
                    nDecimals = 1;
                    break;
                case "CONT":
                case "SECU":
                    sValue = GetContactStateString(alarmObject.UTID, alarmObject.Probe, alarmObject.Value.ToDecimal(), 1); //Language = 1
                    nDecimals = 0;
                    break;
                case "NAFE": //NAFEM Protocol Device Sensor
                    int index = 0;
                    foreach (string value in alarmObject.SensorType.Split(':'))
                    {
                        aParts[index] = value;
                        index++;
                        if (index == 4)
                            break;
                    }
                    dUOM = 0;
                    if (aParts[1] != "")
                        dUOM = aParts[1].ToDecimal();
                    if (dUOM == Measure.UOMBitMask.ToDecimal())
                    {
                        sUOM = "";
                        sValue = GetContactStateString(alarmObject.UTID, alarmObject.Probe, alarmObject.Value.ToDecimal(), 1); //Language = 1
                    }
                    else
                        sUOM = NAFEMuom2DisplayString(dUOM);
                    if (dUOM == Measure.UOMcelsius.ToDecimal())
                    {
                        sValue = Common.FahrenheitToCelsius(alarmObject.Value.ToStr());
                        sMin = Common.FahrenheitToCelsius(alarmObject.AlarmMinValue.ToStr());
                        sMax = Common.FahrenheitToCelsius(alarmObject.AlarmMaxValue.ToStr());
                    }
                    nDecimals = 1;
                    iUOM = dUOM;
                    break;
                case "HUMI":
                    sUOM = "%RH";
                    nDecimals = 1;
                    break;
                case "V5":
                case "V10":
                    sUOM = "Volts";
                    break;
                default:
                    sUOM = GetSensorTypeUOM(alarmObject);
                    nDecimals = GetSensorTypeDecimals(alarmObject.SensorType);
                    break;
            }

            /* Contact sensor will give the value as "Opened or Closed", before casting the to double, check the value type.*/
            double outValue;
            if (double.TryParse(sValue, out outValue))
            {
                if (sValue != "")
                {
                    sValue = Microsoft.VisualBasic.Strings.FormatNumber(Double.Parse(sValue), nDecimals);
                    sMin = (sMin != "") ? Microsoft.VisualBasic.Strings.FormatNumber(Double.Parse(sMin), nDecimals) : "";
                    sMax = (sMax != "") ? Microsoft.VisualBasic.Strings.FormatNumber(Double.Parse(sMax), nDecimals) : "";

                }

            }

            //sValue, sMin, sMax, sUOM, iUOM

            aValues[0] = sValue;
            aValues[1] = sMin;
            aValues[2] = sMax;
            aValues[3] = sUOM;
            aValues[4] = iUOM.ToStr();
            return aValues;
        }

        /// <summary>
        /// Format the sensor values
        /// </summary>
        /// <param name="alarmObject"></param>
        /// <returns></returns>
        private string FormatCurrentSensorValue(Interface.Alarm.AlarmObject alarmObject, SensorCurrentStatus sensorCurrentStatus)
        {

            //Define local variables
            string sValue;
            int nDecimals;

            sValue = sensorCurrentStatus.CurrentValue.ToStr();
            nDecimals = 1;

            switch (((alarmObject.SensorType.ToStr() != string.Empty) && (alarmObject.SensorType.ToStr().Length >= 4)) ? alarmObject.SensorType.Substring(0, 4) : "")
            {
                case "TEMP":
                case "THER":
                    if (alarmObject.IsCelsius)
                    {
                        sValue = Common.FahrenheitToCelsius(alarmObject.Value.ToStr());
                    }

                    nDecimals = 1;
                    break;
                case "CONT":
                case "SECU":
                    sValue = GetContactStateString(alarmObject.UTID, alarmObject.Probe, alarmObject.Value.ToDecimal(), 1); //Language = 1
                    nDecimals = 0;
                    break;
                //case "NAFE": //NAFEM Protocol Device Sensor
                //    int index = 0;
                //    foreach (string value in alarmObject.SensorType.Split(':'))
                //    {
                //        aParts[index] = value;
                //        index++;
                //        if (index == 4)
                //            break;
                //    }
                //    dUOM = 0;
                //    if (aParts[1] != "")
                //        dUOM = aParts[1].ToDecimal();
                //    if (dUOM == Measure.UOMBitMask.ToDecimal())
                //    {
                //        sUOM = "";
                //        sValue = GetContactStateString(alarmObject.UTID, alarmObject.Probe, alarmObject.Value.ToDecimal(), 1); //Language = 1
                //    }
                //    else
                //        sUOM = NAFEMuom2DisplayString(dUOM);
                //    if (dUOM == Measure.UOMcelsius.ToDecimal())
                //    {
                //        sValue = Common.FahrenheitToCelsius(alarmObject.Value.ToStr());
                //        sMin = Common.FahrenheitToCelsius(alarmObject.AlarmMinValue.ToStr());
                //        sMax = Common.FahrenheitToCelsius(alarmObject.AlarmMaxValue.ToStr());
                //    }
                //    nDecimals = 1;
                //    iUOM = dUOM;
                //    break;
                case "HUMI":
                    nDecimals = 1;
                    break;
                case "V5":
                case "V10":
                    break;
                default:
                    nDecimals = GetSensorTypeDecimals(alarmObject.SensorType);
                    break;
            }

            /* Contact sensor will give the value as "Opened or Closed", before casting the to double, check the value type.*/
            double outValue;
            if (double.TryParse(sValue, out outValue))
            {
                if (sValue != "")
                {
                    sValue = Microsoft.VisualBasic.Strings.FormatNumber(Double.Parse(sValue), nDecimals);
                }
            }

            return sValue;
        }

        /// <summary>
        /// Get FormatID
        /// </summary>
        /// <param name="alarmObject"></param>
        /// <param name="formatType"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private int GetFormatID(Interface.Alarm.AlarmObject alarmObject, string formatType, int action)
        {
            int sensorFormatID = 0;
            SensorFormatString sensorFormatString = null;
            try
            {
                sensorFormatString = new SensorFormatString();
                sensorFormatString.UTID = alarmObject.UTID;
                sensorFormatString.Probe = alarmObject.Probe;
                sensorFormatString.FormatType = formatType;
                sensorFormatString.Action = action;

                sensorFormatString.Execute(sensorFormatString);
                sensorFormatID = sensorFormatString.SensorFormatID;
            }
            catch (Exception ex)
            {
                /*Debug Object values for reference*/
                LogBook.Debug(sensorFormatString, this);

                /*Write exception log*/
                LogBook.Write("Error has occurred while retrieving SensorFormatID", ex, "CooperAtkins.NotificationClient.NotificationComposer.Notification");
            }
            finally
            {
                sensorFormatString.Dispose();
            }
            return sensorFormatID;
        }

        /// <summary>
        /// Get contact state string
        /// </summary>
        /// <returns></returns>
        private string GetContactStateString(string utid, int probe, decimal state, int language)
        {
            string contactStateString = "";
            ContactState contactState = null;

            try
            {
                contactState = new ContactState();
                contactState.UTID = utid;
                contactState.Probe = probe;
                contactState.contactState = state;
                contactState.LanguageID = language;
                contactState.Execute(contactState);

                contactStateString = contactState.contactStateString;
            }
            catch (Exception ex)
            {
                /*Debug Object values for reference*/
                LogBook.Debug(contactState, this);

                /*Write exception log*/
                LogBook.Write("Error has occurred while retrieving contactStateString", ex, "CooperAtkins.NotificationClient.NotificationComposer.Notification");
            }
            finally
            {
                contactState.Dispose();
            }
            //Set default string if contact string doesn't exist
            if (contactStateString == string.Empty && state == 0)
                contactStateString = "Closed";
            else if (contactStateString == string.Empty)
                contactStateString = "Open";

            return contactStateString;
        }

        /// <summary>
        /// Returns unit of measure based on dUOM 
        /// </summary>
        /// <param name="dUOM"></param>
        /// <returns></returns>
        private string NAFEMuom2DisplayString(decimal dUOM)
        {
            decimal uom2;
            string displayString = "";
            int uom = dUOM.ToInt();
            string[] names = new string[] { "Undefined", "m", "mm", "g", "Kg", "sec", "Amp", "mA", "K", "C", "candela", "lumen", "lux", "Hz", "newton", "pascal", "joule", "W", "V", "Ohm", "radian", "degree", "gray", "sievert", "katal", "poiseuille", "rayl", "dB", "Inch", "Ft", "Lb", "F", "psi", "btu" };
            string[] customNames = new string[] { " " };
            if (uom >= 0 && uom < names.Length)
                displayString = names[uom];
            else if (uom >= Measure.UOM_CustomFirst.ToDecimal())
            {
                uom2 = uom - Measure.UOM_CustomFirst.ToDecimal();
                if (uom2 >= 0 && uom2 < customNames.Length)
                    displayString = customNames[uom2.ToInt()];
            }

            if (displayString == "")
            {
                displayString = NAFEMuom2string(uom);
            }
            return "";
        }

        /// <summary>
        ///  Returns unit of measure based on dUOM 
        /// </summary>
        /// <param name="dUOM"></param>
        /// <returns></returns>
        private string NAFEMuom2string(decimal dUOM)
        {
            decimal uom2;
            string displayString = "";
            int uom = dUOM.ToInt();
            string[] names = new string[] { "Undefined", "meter", "millimeter", "gram", "kilogram", "second", "ampere", "milliampere", "kelvin", "celsius", "candela", "lumen", "lux", "hertz", "newton", "pascal", "joule", "watt", "volt", "ohm", "radian", "degree", "gray", "sievert", "katal", "poiseuille", "rayl", "dB", "inch", "foot", "pound", "fahrenheit", "psi", "btu" };
            string[] customNames = new string[] { "Bitmask" };
            if (uom >= 0 && uom < names.Length)
                displayString = names[uom];
            else if (uom >= Measure.UOM_CustomFirst.ToDecimal())
            {
                uom2 = uom - Measure.UOM_CustomFirst.ToDecimal();
                if (uom2 >= 0 && uom2 < customNames.Length)
                    displayString = customNames[uom2.ToInt()];
            }

            if (displayString == "")
            {
                displayString = "Type:" + uom;
            }
            return displayString;
        }

        /// <summary>
        /// Check for sensor type Units Of Measure
        /// </summary>
        /// <param name="alarmObject"></param>
        /// <returns></returns>
        private string GetSensorTypeUOM(Interface.Alarm.AlarmObject alarmObject)
        {
            string sensorType = "", sensorTypeUOM = "";
            SensorTypeInfo sensorTypeInfo = null;
            sensorType = alarmObject.SensorType.ToStr().Trim().ToUpper();
            if (AlarmHelper.IsTempSensor(sensorType))
                sensorTypeUOM = (alarmObject.IsCelsius) ? "C" : "F";
            else if (sensorType.IndexOf("HUMI") >= 0)
                sensorTypeUOM = "%RH";
            else if (AlarmHelper.IsContactSensor(sensorType))
                sensorTypeUOM = "";
            else if (sensorType.Length > 0)
            {
                sensorTypeInfo = AlarmHelper.GetSensorTypeInfo(sensorType);
                if (sensorTypeInfo != null)
                {
                    sensorTypeUOM = sensorTypeInfo.UOM;
                    sensorTypeInfo = null;
                }
            }
            return sensorTypeUOM;
        }


        private int GetSensorTypeDecimals(string sensorType)
        {
            int sensorTypeDecimals = 1;
            SensorTypeInfo sensorTypeInfo = null;
            sensorType = sensorType.ToStr().Trim().ToUpper();
            if (AlarmHelper.IsTempSensor(sensorType))
                sensorTypeDecimals = 1;
            else if (sensorType.IndexOf("HUMI") >= 0)
                sensorTypeDecimals = 0;
            else if (AlarmHelper.IsContactSensor(sensorType))
                sensorTypeDecimals = 0;
            else if (sensorType.IndexOf("REPEA") >= 0)
                sensorTypeDecimals = 0;
            else if (sensorType.Length > 0)
            {
                sensorTypeInfo = AlarmHelper.GetSensorTypeInfo(sensorType);
                if (sensorTypeInfo.ID != 0)
                {
                    sensorTypeDecimals = sensorTypeInfo.nDecimals;
                    sensorTypeInfo = null;
                }
            }
            return sensorTypeDecimals;
        }

        /// <summary>
        /// Record notification results in the database
        /// </summary>
        /// <param name="logText"></param>
        /// <param name="notificationID"></param>
        /// <param name="status"></param>
        /// <param name="notificationType"></param>
        public void RecordNotification(string logText, int notificationID, int transactionID, NotifyStatus status, NotifyTypes notificationType)
        {
            RecordNotification recordNotification = null;
            try
            {
                recordNotification = new RecordNotification();
                /*Error or Notification response is recorded*/
                recordNotification.LogText = logText;
                recordNotification.NotificationID = notificationID;
                recordNotification.TransID = transactionID;
                recordNotification.Status = status;
                recordNotification.NotifyType = notificationType;
                recordNotification.Execute();
            }
            catch (Exception ex)
            {
                /*Debug Object values for reference*/
                LogBook.Debug(recordNotification, this);

                /*Write exception log*/
                LogBook.Write("Error has occurred while inserting record notification parameters into 'ttNotificationLog' table", ex, "CooperAtkins.NotificationClient.NotificationComposer.EmailNotificationComposer");
            }
            finally
            {
                recordNotification.Dispose();
            }
        }

        /// <summary>
        /// Get Current minutes in alarm based on alarm exit time
        /// </summary>
        /// <param name="alarmObject"></param>
        private int GetCurrentMinInAlarm(AlarmObject alarmObject)
        {
            int currentMinInAlarm = 0;
            DateTime exitTime = DateTime.MinValue;
            try
            {

                if (alarmObject.AlarmStateExitTime < alarmObject.AlarmStartTime)
                {
                    exitTime = DateTime.UtcNow;
                }
                //Get the date difference
                TimeSpan diffTime;
                diffTime = alarmObject.AlarmStartTime - exitTime;
                currentMinInAlarm = diffTime.Minutes + (60 * diffTime.Hours) + ((diffTime.Days * 24) *60);
                //LogBook.Write("Minutes to for missed comm: " + diffTime.Minutes + (60 * diffTime.Hours));
                //currentMinInAlarm = diffTime.TotalMinutes.ToInt(); //ASSUMES NO OVERFLOW
                

            }
            catch (Exception ex)
            {
                //Write Log
                LogBook.Write("Error occurred while getting current alarm minutes", ex, "CooperAtkins.NotificationClient.NotificationComposer.NotificationStyle");

            }
            return currentMinInAlarm;
        }

        /// <summary>
        /// Format Factory ID
        /// </summary>
        /// <returns></returns>
        public string FormatFactoryID(string ID)
        {
            string factoryID = "";
            if (Strings.Len(Strings.Trim(ID)) < 7)
            {
                factoryID = Strings.Right("000000" + Strings.Trim(ID), 6);
                factoryID = Strings.Left(factoryID, 3) + "-" + Strings.Right(factoryID, 3);
                for (int i = 1; i <= 3; i++)
                {
                    if (Strings.Mid(factoryID, i, 1) != "0")
                    {
                        factoryID = Strings.Mid(factoryID, i);
                        return factoryID;
                    }
                }
            }
            else
            {
                factoryID = Strings.Trim(ID);
            }
            return factoryID;
        }


    }
}
