/*
 *  File Name : EnterpriseModelConfig.cs
 *  Author : Rajesh
 *  @ PCC Technology Group LLC
 *  Created Date : 11/22/2010
 */


namespace CooperAtkins.NotificationClient.Generic.DataAccess
{
    using System;
    using EnterpriseModel.Net;

    public class EnterpriseModelConfig : DefaultLibConfig, ILibConfig
    {
        private string _sqlConnectionString = null;
        public EnterpriseModelConfig(string sqlConnectionString) {
            _sqlConnectionString = sqlConnectionString;
        }
        /// <summary>
        /// set the database type to be connected to ex: Sql Server, MySql, etc
        /// </summary>
        public override Type DataAccessType
        {
            get { return typeof(SqlDataAccess); }
        }
        #region ILibConfig Members
        /// <summary>
        /// set the data base connection string
        /// </summary>
        /// <returns></returns>
        public EnterpriseModel.Net.DataAccess GetDbGateway()
        {
            return new SqlDataAccess(_sqlConnectionString);
        }

        #endregion

        #region ILibClientConfig Members

        public AppClientDomainConfig AppClientDomainConfig
        {
            get { return null; }
        }
        #endregion
        public override string ParameterPrefixText
        {
            get
            {
                return "@";
            }
        }
    }
}
