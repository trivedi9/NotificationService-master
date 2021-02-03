using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CooperAtkins.NotificationClient.Generic.DataAccess
{
    public class ServiceEntity
    {
        public string ExeName { get; set; }
        public int OUID { get; set; }
        public int PID { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
