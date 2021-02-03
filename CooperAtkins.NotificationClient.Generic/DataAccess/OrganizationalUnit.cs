using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CooperAtkins.NotificationClient.Generic.DataAccess
{
    public class OrganizationalUnit
    {
        public int OUID { get; set; }
        public int OGID { get; set; }
        public string OUName { get; set; }
        public string DBName { get; set; }
        public string DSN { get; set; }
        public bool isActive { get; set; }
    }
}
