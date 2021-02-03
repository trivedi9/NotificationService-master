using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnterpriseModel.Net;
using CooperAtkins.Generic;

namespace CooperAtkins.NotificationClient.Generic.DataAccess
{
    public class OrganizationalUnitList : DomainListBase<OrganizationalUnitList ,OrganizationalUnit>
    {
        protected override OrganizationalUnitList LoadList(BaseCriteria criteria)
        {
            try
            {
                Criteria listCriteria = (Criteria)criteria;
                //Initialize the CSqlDbCommand for execute the stored procedure
                CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_ORGANIZATIONALUNITS);
                //Execute reader 
                CDAO.ExecReader(cmd);
                while (CDAO.DataReader.Read())
                {
                    OrganizationalUnit orgUnit = new OrganizationalUnit()
                    {
                        OUID = CDAO.DataReader["OUID"].ToInt(),
                        OGID = CDAO.DataReader["OGID"].ToInt(),
                        OUName = CDAO.DataReader["OUName"].ToStr(),
                        DBName = CDAO.DataReader["DBName"].ToStr(),
                        DSN = CDAO.DataReader["DSN"].ToStr(),
                        isActive = CDAO.DataReader["isActive"].ToBoolean()
                    };

                    this.Add(orgUnit);
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
