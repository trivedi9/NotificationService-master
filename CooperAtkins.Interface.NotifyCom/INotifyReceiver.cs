using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CooperAtkins.Interface.NotifyCom
{
    /// <summary>
    /// INotifyReceiver Interface
    /// </summary>
    public interface INotifyReceiver  
    {
        NotifyComResponse Execute(INotifyObject notifyObject);
        INotifyObject PrepareNotifyObject(object data);
        void StopReceiving();
    }

   
}
