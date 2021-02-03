/*
 *  File Name : TypeCommonExtensions.cs
 *  Author : Aditya
 *  @ PCC Technology Group LLC
 *  Created Date : 11/23/2010
 */

namespace CooperAtkins.Generic
{
    using System;
    using System.Collections.Generic;

    public static class TypeCommonExtensions
    {

        public static bool IsNumeric(this string str)
        {
            int retValue;
            return int.TryParse(str, out retValue);
        }

        public static string DefaultText(this string str, string text)
        {
            return str.Trim() == "" ? text : str;
        }

        public static char ToChar(this string str)
        {
            char retValue;
            char.TryParse(str, out retValue);
            return retValue;
        }
        public static char ToChar(this object str)
        {
            if (str == DBNull.Value)
            {
                return ' ';
            }
            else
            {
                char retValue;
                char.TryParse(str.ToString(), out retValue);
                return retValue;
            }
        }

        public static int ToInt(this string str)
        {
            int retValue;
            int.TryParse(str, out retValue);
            return retValue;
        }

        public static int ToInt(this object str)
        {
            if (str == DBNull.Value || str == null)
            {
                return 0;
            }
            else
            {
                int retValue;
                int.TryParse(str.ToString(), out retValue);
                return retValue;
            }
        }

        public static long ToLong(this object str)
        {
            if (str == DBNull.Value || str == null)
            {
                return 0;
            }
            else
            {
                long retValue;
                long.TryParse(str.ToString(), out retValue);
                return retValue;
            }
        }

        public static Int16 ToInt16(this string str)
        {
            Int16 retValue;
            Int16.TryParse(str, out retValue);
            return retValue;
        }

        public static Int16 ToInt16(this object str)
        {
            if (str == DBNull.Value || str == null)
            {
                return 0;
            }
            else
            {
                Int16 retValue;
                Int16.TryParse(str.ToString(), out retValue);
                return retValue;
            }
        }
        public static DateTime ToDateTime(this string str)
        {
            DateTime retValue;
            DateTime.TryParse(str, out retValue);
            return retValue;
        }

        public static DateTime ToDateTime(this object str)
        {
            if (str == DBNull.Value)
            {
                return DateTime.MinValue;
            }
            else
            {
                DateTime retValue;
                DateTime.TryParse(str.ToString(), out retValue);
                return retValue;
            }
        }

        public static object MinDateToDateTime(this DateTime str)
        {
            if (str == DateTime.MinValue)
            {
                return null;
            }
            else
            {
                return str;
            }
        }

        public static bool IsNumericDecimal(this string str)
        {
            decimal retValue;
            return decimal.TryParse(str, out retValue);
        }

        public static decimal ToDecimal(this object str)
        {
            if (str == DBNull.Value)
            {
                return 0;
            }
            else
            {
                decimal retValue;
                decimal.TryParse(str.ToString(), out retValue);
                return retValue;
            }
        }

        public static double ToDouble(this object str, double secondaryValue = 0)
        {
            if (str == DBNull.Value)
            {
                if (secondaryValue != 0)
                    return secondaryValue;
                else
                    return 0;
            }
            else
            {
                double retValue;
                double.TryParse(str.ToString(), out retValue);
                return retValue;
            }
        }

        public static DateTime IsDateTime(this string str)
        {
            DateTime retValue;
            DateTime.TryParse(str, out retValue);
            return retValue;
        }

        public static bool IsEmpty(this string str)
        {
            return str.Trim() == string.Empty;
        }
        public static bool IsNullOrEmpty(this string str)
        {
            return str.Trim() == string.Empty || str == null;
        }

        public static object ToObject(this int? val)
        {
            if (val == null)
                return DBNull.Value;
            else
                return val;
        }

        public static object ToObject(this int val)
        {
            if (val == 0)
                return DBNull.Value;
            else
                return val;
        }

        public static object ToDbObject(this string val)
        {
            if (val == null)
                return DBNull.Value;
            else
                return val;
        }
        public static object ToDbObject(this object val)
        {
            if (val == null)
                return DBNull.Value;
            else
                return val;
        }

        /// <summary>
        /// Converts the DBNull or null to string.Empty, it will return secondaryValue, if it is provided.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="secondaryValue"></param>
        /// <returns></returns>
        public static string ToStr(this object val, string secondaryValue = "")
        {
            if (val == DBNull.Value || val == null)
            {
                if (secondaryValue != "")
                    return secondaryValue;
                else
                    return string.Empty;
            }
            else
                return val.ToString();
        }


        /// <summary>
        /// To convert the list of strings to a single string.
        /// </summary>
        /// <param name="val">List of strings</param>
        /// <param name="breakLine">To add each string in the list as a new line.</param>
        /// <returns></returns>
        public static string ToText(this List<string> val, bool breakLine)
        {
            if (val == null)
                return string.Empty;

            string text = string.Empty;
            foreach (string str in val)
            {
                text += (breakLine ? "\r\n" : "") + str;
            }
            return text;
        }



        /// <summary>
        /// To convert empty string to null.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string EmptyToNull(this object val)
        {
            if (val.ToString() == string.Empty)
                return null;
            else
                return val.ToString();
        }

        public static bool ToBoolean(this object val)
        {
            if (val == DBNull.Value || val == null)
                return false;
            if (val.ToStr() == string.Empty)
                return false;
            else if (val.ToString().ToLower() == "true")
                return true;
            else if (val.ToString().ToLower() == "false" || val.ToString() == "0" || val.ToInt() <= 0)
                return false;
            else
                return true;
        }

        public static char ToChar(this bool val)
        {
            if (val)
                //return '1';
                return 'Y';
            else
                //return '0';
                return 'N';
        }

        /// <summary>
        /// To split the given string with split value.
        /// Use of FlexSplit: If the given index exceeds the split array then it will return the empty value.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="splitValue"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string FlexSplit(object val, char splitValue, Int16 index)
        {
            if (val == null)
                return string.Empty;
            if (index <= val.ToString().Split(splitValue).Length - 1)
                return val.ToString().Split(splitValue)[index];
            else
                return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string FlexSubString(object val, Int16 startIndex, Int16 length)
        {
            if (val == null)
                return string.Empty;
            if (val.ToString().Length >= startIndex + length)
                return val.ToString().Substring(startIndex, length);
            else
                return string.Empty;
        }

        public static System.Collections.IDictionary ToDictionary(this object obj)
        {
            System.Collections.Generic.Dictionary<string, object> objDescr = new System.Collections.Generic.Dictionary<string, object>();

            foreach (System.ComponentModel.PropertyDescriptor descriptor in System.ComponentModel.TypeDescriptor.GetProperties(obj))
            {
                objDescr.Add(descriptor.Name, descriptor.GetValue(obj));
            }
            return objDescr;
        }



        public static object IfNull(object value, object secondaryValue)
        {
            if (value == DBNull.Value || value == null)
            {
                return secondaryValue;
            }
            else
            {
                return value;
            }

        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }
    }
}
