/*
* File : TTIVRNotifications.cs
* Author : Vasu
* Created Date :1/3/2011
* File Version : 1 .0
*/
namespace CooperAtkins.NotificationClient.IVRNotificationComposer.DataAccess
{
    using System;
    using System.Collections.Generic;
    using EnterpriseModel.Net;
    using CooperAtkins.Generic;

    public class TTIvrNotifications : DomainCommandEx<TTIvrNotifications>
    {


        #region Properties

        /// <summary>
        /// Gets or sets the RecID value.
        /// </summary>
        public int RecID { get; set; }

        /// <summary>
        /// Gets or sets the QueueTime value.
        /// </summary>
        public DateTime QueueTime { get; set; }

        /// <summary>
        /// Gets or sets the Notification_RecID value.
        /// </summary>
        public int Notification_RecID { get; set; }

        /// <summary>
        /// Gets or sets the Notification_TransID value.
        /// </summary>
        public int TransID { get; set; }

        /// <summary>
        /// Gets or sets the PhoneNumber value.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the PhoneNumber value.
        /// </summary>
        public string PersonName { get; set; }

        /// <summary>
        /// Gets or sets the AlarmID value.
        /// </summary>
        public int AlarmID { get; set; }

        /// <summary>
        /// Gets or sets the isSuccess value.
        /// </summary>
        public Int16 isSuccess { get; set; }

        /// <summary>
        /// Gets or sets the isSuccess value.
        /// </summary>
        public Int16 isInProcess { get; set; }

        /// <summary>
        /// Gets or sets the numAttempts value.
        /// </summary>
        public int NumAttempts { get; set; }

        /// <summary>
        /// Gets or sets the LastAttemptTime value.
        /// </summary>
        public DateTime LastAttemptTime { get; set; }

        /// <summary>
        /// Gets or sets the UserID value.
        /// </summary>
        /// 
        public int UserID { get; set; }

        public int ThreadID { get; set; }

        public string Action { get; set; }


        #endregion


        protected override TTIvrNotifications ExecuteCommand()
        {
            //Initialize the CSqlDbCommand for execute the stored procedure
            CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_IVR_RECORDIVRNOTIFICATION);

            //Add action parameter (C -Create), for insert new record in the database table
            cmd.AddWithValue("Action", Action);

            //Bind TTIVRNotifications entity property values as input parameters for procedure
            BindParams(cmd, this);

            //Execute command 
            CDAO.ExecCommand(cmd);

            //Bind output values to TTIVRNotifications object
            this.RecID = Convert.ToInt32(CDAO.Parameters["RecID"].Value);
            this.TransID = Convert.ToInt32(CDAO.Parameters["TransID"].Value);

            this.PhoneNumber = CDAO.Parameters["PhoneNumber"].Value.ToStr();
            this.isInProcess = Convert.ToInt16(CDAO.Parameters["isInProcess"].Value);

            //return TTIVRNotifications object
            return this;
        }

        private void BindParams(CSqlDbCommand cmd, TTIvrNotifications entityObject)
        {
            CSqlDbParamter parm = new CSqlDbParamter("RecID", entityObject.RecID);
            parm.DbType = System.Data.DbType.Int32;
            parm.Direction = System.Data.ParameterDirection.InputOutput;
            cmd.Parameters.Add(parm);

            CSqlDbParamter phoneNumber = new CSqlDbParamter("PhoneNumber", entityObject.PhoneNumber);
            phoneNumber.DbType = System.Data.DbType.String;
            phoneNumber.Size = 20;
            phoneNumber.Direction = System.Data.ParameterDirection.InputOutput;
            cmd.Parameters.Add(phoneNumber);

          //  cmd.AddWithValue("QueueTime", entityObject.QueueTime);
            cmd.AddWithValue("Notification_RecID", entityObject.Notification_RecID);
            cmd.AddWithValue("TransID", entityObject.TransID);

            cmd.AddWithValue("AlarmID", entityObject.AlarmID);
            cmd.AddWithValue("isSuccess", entityObject.isSuccess);
            cmd.AddWithValue("isInProcess", entityObject.isInProcess);
            cmd.AddWithValue("LastAttemptTime", entityObject.LastAttemptTime);
            cmd.AddWithValue("UserID", entityObject.UserID);
            cmd.AddWithValue("NumAttempts", entityObject.NumAttempts);
            cmd.AddWithValue("ThreadID", entityObject.ThreadID);
        }
    }
}