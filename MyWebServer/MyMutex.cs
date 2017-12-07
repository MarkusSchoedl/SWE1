using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MyWebServer
{
    /// <summary>
    /// This Class provides a Mutex which doesnt stop the thread if we want to lock.
    /// It provides Methods which just Try to lock and return a certain flag indicating if it worked or not.
    /// </summary>
    class MyMutex
    {
        #region Fields
        private bool _IsLocked = false;
        private Mutex _BoolLock = new Mutex();

        private Mutex _RealLock = new Mutex();
        #endregion Fields

        /// <summary>
        /// Creates a new Instance of the <see cref="MyMutex"/> class
        /// </summary>
        public MyMutex() { }

        #region Methods
        /// <summary>
        /// Checks if the <see cref="MyMutex"/> is already locked.
        /// </summary>
        /// <returns>True if locked; False if not locked</returns>
        public bool IsLocked()
        {
            return _IsLocked;
        }

        /// <summary>
        /// Just tries to lock the mutex. Instead of blocking the thread we return a flag to indicate if locking worked or if it did'nt.
        /// </summary>
        /// <returns>True on successful Lock, False otherwise.</returns>
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

        /// <summary>
        /// Releases the <see cref="MyMutex"/> once.
        /// </summary>
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
