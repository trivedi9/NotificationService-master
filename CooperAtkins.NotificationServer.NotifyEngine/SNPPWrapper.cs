/*
 *  File Name : SNPPWrapper.cs
 *  Author : Cooper Atkins 
 *  Created Date : 
 *  
 */

namespace CooperAtkins.NotificationServer.NotifyEngine
{
    using System;
    using System.Runtime.InteropServices;

    public class SNPPWrapper : IDisposable
    {
        private KTGUtil.SNPP _snpp = new KTGUtil.SNPP();
        private bool hasBeenDisposed = false;

        public string Host
        {
            get
            {
                return _snpp.Host;
            }
            set
            {
                _snpp.Host = value;
            }
        }

        public int Port
        {
            get
            {
                return _snpp.Port;
            }
            set
            {
                _snpp.Port = value;
            }
        }

        public string Subject
        {
            get
            {
                return _snpp.Subject;
            }
            set
            {
                _snpp.Subject = value;
            }
        }

        public string Body
        {
            get
            {
                return _snpp.Body;
            }
            set
            {
                _snpp.Body = value;
            }
        }

        public string SendTo
        {
            get
            {
                return _snpp.SendTo;
            }
            set
            {
                _snpp.SendTo = value;
            }
        }

        public string LastErrorText
        {
            get
            {
                return _snpp.LastErrorText;
            }
        }

        public SNPPWrapper()
        {
            //_SNPP = pSNPP;
            //_SNPP = new KTGUtil.SNPP();
        }

        public void Dispose()
        {
            Dispose(true);
            //GC.SuppressFinalize(this);

        }

        protected virtual void Dispose(bool disposeManagedObjects)
        {

            if (!hasBeenDisposed)
            {
                try
                {
                    if (disposeManagedObjects)
                    {
                        // Modified on 2/19/2012
                        // Srinivas Rao Eranti
                        // To release the unmanaged object
                        Marshal.FinalReleaseComObject(_snpp);
                        // GC.SuppressFinalize(_snpp);
                    }
                }
                catch
                {
                    hasBeenDisposed = false;
                    throw;
                }

                hasBeenDisposed = true;
            }
        }


        ~SNPPWrapper()
        {
            Dispose(false);
        }


        public int Send()
        {
            if (hasBeenDisposed == true)
            {
                throw (new ObjectDisposedException(this.ToString(), "Object has been disposed"));
            }

            return _snpp.Send();


        }



    }
}


