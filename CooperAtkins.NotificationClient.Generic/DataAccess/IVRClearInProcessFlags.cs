

namespace CooperAtkins.NotificationClient.Generic.DataAccess
{
    using System;
    using System.Collections.Generic;
    using EnterpriseModel.Net;
    using CooperAtkins.Generic;

   public class IVRClearInProcessFlags : DomainCommandEx<IVRClearInProcessFlags>
    {

        protected override IVRClearInProcessFlags ExecuteCommand()
        {
            //Initialize the CSqlDbCommand for execute the stored procedure
            CSqlDbCommand cmd = new CSqlDbCommand(DBCommands.USP_NS_IVR_CLEARINPROCESS);

            //Execute command 
            CDAO.ExecCommand(cmd);

            return this;
        }

        
    }
}

