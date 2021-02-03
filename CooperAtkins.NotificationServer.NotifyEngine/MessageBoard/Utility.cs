namespace CooperAtkins.NotificationServer.NotifyEngine
{
    using System;
    using System.Text;

    public static class Utility
    {
        public static string HexToASCII(string hexString)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i <= hexString.Length - 2; i += 2)
            {
                sb.Append(Convert.ToString(Convert.ToChar(Int32.Parse(hexString.Substring(i, 2), System.Globalization.NumberStyles.HexNumber))));
            }
            return sb.ToString();
        }

        public static string CalcCheckSum(string data)
        {
            Int32 s = 0;
            for (int i = 0; i < data.Length; i++)
            {
                char c = Convert.ToChar(data.Substring(i, 1));
                int ascii = c;
                s = s + ascii;
            }
            string checkSum = "0000" + String.Format("{0:X}", s);
            return checkSum.Substring(checkSum.Length - 4, 4);
        }

        public static string Digits2(string numValue)
        {
            numValue = "00" + numValue;
            return numValue.Substring(numValue.Length - 2, 2);
        }
    }
}
