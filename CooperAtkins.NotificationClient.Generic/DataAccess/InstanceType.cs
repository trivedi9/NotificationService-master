
/*
 *  File Name : RecordNotification.cs
 *  Author : Pradeep.I
 *  @ PCC Technology Group LLC
 *  Created Date : 02/17/2011
 */
namespace CooperAtkins.NotificationClient.Generic.DataAccess
{
    using EnterpriseModel.Net;

    public class InstanceType : DomainCommandEx<InstanceType>
    {
        public bool IsMultiDB { get; set; }
        protected override InstanceType ExecuteCommand()
        {
            try
            {
                //Initialize the CSqlDbCommand for execute the stored procedure
                CSqlDbCommand cmd = new CSqlDbCommand("SELECT COUNT(TABLE_NAME) AS [Count] FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OrganizationalUnits'", System.Data.CommandType.Text);

                //Execute command 
                CDAO.ExecReader(cmd);
                if (CDAO.DataReader.Read())
                {
                    IsMultiDB = CDAO.DataReader["Count"].ToString() == "1" ? true : false;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                CDAO.CloseDataReader();
                CDAO.Dispose();
            }
            
            return this;
        }
    }
}
