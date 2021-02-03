using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnterpriseModel.Net;

namespace CooperAtkins.NotificationClient.Generic.DataAccess
{
    public class ServiceEntityContext : DomainContext<ServiceEntity,Criteria>
    {
        protected override ServiceEntity Insert(ServiceEntity entityObject)
        {
            /* Initializing the command object. */
            CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_SERVICE);
            cmd.Parameters.AddWithValue("action", "C");

            /*Bind parameters to command object.*/
            BindParameters(cmd, entityObject);

            /* Executing the procedure.*/
            CDAO.ExecScalar(cmd);

            return entityObject;
        }

        private void BindParameters(CSqlDbCommand cmd, ServiceEntity entityObject)
        {
            cmd.Parameters.AddWithValue("OUID", entityObject.OUID);
            cmd.Parameters.AddWithValue("PID", entityObject.PID);
            cmd.Parameters.AddWithValue("ExeName", entityObject.ExeName);
            cmd.Parameters.AddWithValue("TimeStamp", entityObject.TimeStamp);
        }

        protected override ServiceEntity Load(Criteria criteria)
        {
            throw new NotImplementedException();
        }

        protected override int Remove(Criteria criteria)
        {
            throw new NotImplementedException();
        }

        protected override ServiceEntity Update(ServiceEntity entityObject)
        {
            CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_SERVICE);
            cmd.Parameters.AddWithValue("action", "U");
            cmd.Parameters.AddWithValue("PID", entityObject.PID);
            CDAO.ConnectionString = entityObject.ExeName;
            CDAO.ExecScalar(cmd);
            return entityObject;
        }
    }
}