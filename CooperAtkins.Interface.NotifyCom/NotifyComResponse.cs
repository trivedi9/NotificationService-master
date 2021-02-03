/*
 *  File Name : NotifyComResponse.cs
 *  Author : Rajesh Jinaga 
 *           @ PCC Technology Group LLC
 *  Created Date : 11/23/2010
 *  
 */
namespace CooperAtkins.Interface.NotifyCom
{
    using System;
    using System.Text;
    using System.Xml;
    public class NotifyComResponse:  MarshalByRefObject
    {
        /// <summary>
        /// It is set to true if the notification request has been executed successfully, else false.
        /// </summary>
        public bool IsSucceeded { get; set; }

        /// <summary>
        /// It is set to true if the notification is still in progress
        /// </summary>
        public bool IsInProcess { get; set; }
        /// <summary>
        /// Response data that is sent back after executing the request.
        /// </summary>
        public object ResponseContent { get; set; }
        /// <summary>
        /// It is set to true if the notification request enconuntered any errors while executing, else false.
        /// </summary>
        public bool IsError { get; set; }
        /// <summary>
        /// Returns xml string for the notification response object, to be transferd to notification client.
        /// </summary>
        /// <returns>xml string</returns>
        public string GetXML() {
            StringBuilder sb = new StringBuilder();
            sb.Append("<notifyComResponse>");
            sb.AppendFormat("<IsSucceeded>{0}</IsSucceeded>", IsSucceeded ? "true": "false");
            sb.AppendFormat("<IsInProcess>{0}</IsInProcess>", IsInProcess ? "true" : "false");
            sb.AppendFormat("<ResponseContent><![CDATA[{0}]]></ResponseContent>", ResponseContent??string.Empty);
            sb.AppendFormat("<IsError>{0}</IsError>", IsError ? "true" : "false");
            sb.AppendFormat("<TransactionID>{0}</TransactionID>", TransactionIDReturned);
            sb.Append("</notifyComResponse>");
            return sb.ToString();
        }
        /// <summary>
        /// when using CDyne service, need to Transaction ID to get response of call
        /// </summary>
        public long TransactionIDReturned { get; set; }

        /// <summary>
        /// constructs the response object based on the xml data recieved
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static NotifyComResponse Create(string data)
        {
            NotifyComResponse response = new NotifyComResponse();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(data.ToString());

            foreach (XmlNode xmlNode in xmlDoc.DocumentElement.ChildNodes)
            {
                switch (xmlNode.Name)
                {
                    case "IsSucceeded":
                        response.IsSucceeded = Convert.ToBoolean(xmlNode.InnerText) ;
                        break;
                    case "IsInProcess":
                        response.IsInProcess = Convert.ToBoolean(xmlNode.InnerText);
                        break;
                    case "ResponseContent":
                        response.ResponseContent = xmlNode.InnerText;
                        break;
                    case "IsError":
                        response.IsError = Convert.ToBoolean(xmlNode.InnerText);
                        break;
                }

            }

            return response;

        }
    }
}
