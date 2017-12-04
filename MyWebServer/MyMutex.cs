using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MyWebServer
{
    class MyMutex
    {
        #region Properties
        private bool _IsLocked = false;
        private Mutex _BoolLock = new Mutex();

        private Mutex _RealLock = new Mutex();
        #endregion Properties

        public MyMutex() { }

        #region Methods
        public bool IsLocked()
        {
            return _IsLocked;
        }

        public bool TryWait()
        {
            _BoolLock.WaitOne();
            if (_IsLocked == false)
            {
                _IsLocked = true;

                _RealLock.WaitOne();

                _BoolLock.ReleaseMutex();
                
                return true;
            }
            _BoolLock.ReleaseMutex();
            return false;
        }

        public void Release()
        {
            _BoolLock.WaitOne();

            _RealLock.ReleaseMutex();

            _IsLocked = false;
            _BoolLock.ReleaseMutex();
        }
        #endregion Methods
    }
}
