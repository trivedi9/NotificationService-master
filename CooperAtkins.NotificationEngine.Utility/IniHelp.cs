using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;

namespace CooperAtkins.NotificationEngine.Utility
{
    public class IniHelp
    {
        public string path = AppDomain.CurrentDomain.BaseDirectory + "Commands.ini";

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,string key, string def, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, byte[] retVal, int size, string filePath);
        
        /// <summary>
        /// Write Data to the INI File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// Section name
        /// <PARAM name="Key"></PARAM>
        /// Key Name
        /// <PARAM name="Value"></PARAM>
        /// Value Name
        public void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, this.path);
        }

        /// <summary>
        /// Read Data Value From the Ini File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// <PARAM name="Key"></PARAM>
        /// <PARAM name="Path"></PARAM>
        /// <returns></returns>
        public string IniReadValue(string Section, string Key)
        {
            
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, this.path);
            return temp.ToString();
        }
        /// <summary>
        /// Read all the keys in a section
        /// </summary>
        /// <param name="Section">section to be read</param>
        /// <returns></returns>
        public string[] IniReadKeys(string Section)
        {

            string retValue = "";
            try
            {
                // prepare the default return and create a byte array to pass as a pointer
                List<string> keys = new List<string>();
                byte[] bRet = new byte[32768];
                // call the API function, passing a Section name, value name, default
                // value, buffer length (length of the passed byte array) and the ini
                // filename.
                int i = GetPrivateProfileString(Section, null, "", bRet, 32768, this.path);
                // as referred above we use a framework function to collect the string
                // from the byte array used as a pointer. Note the 'i' variable is the
                // return from the API call, this is passed to the text encoding as the
                // total length of the return.
                retValue = System.Text.Encoding.GetEncoding(1252).GetString(bRet, 0, i).TrimEnd((char)0);
            }
            catch (FileNotFoundException)
            {
               
            }
            catch(Exception ex)
            {
            }
            //if no command exists with the given user input return null else return keys
            return retValue == ""?  null : retValue.Split('\0');

        }

    }
}
