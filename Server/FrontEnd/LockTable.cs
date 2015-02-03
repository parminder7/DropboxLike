using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Services;

namespace Server.FrontEnd
{
    class Lock
    {
        public Boolean isWriteLock;
        public DateTime lockCreation;
        public int count;
    }
    class LockTable : ILocks
    {
        private ILocks distLockMgr;
        private Dictionary<string, Lock> activeLocks;
        private System.Threading.Thread updaterThread;
        public LockTable(ILocks lockMgr)
        {
            this.distLockMgr = lockMgr;
            activeLocks = new Dictionary<string, Lock>();
            updaterThread = new System.Threading.Thread(new System.Threading.ThreadStart(clean));
            updaterThread.IsBackground = true;
            updaterThread.Start();
        }

        const int W_TIMEOUT = 5;
        const int R_TIMEOUT = 15;
        public void clean()
        {
            while (true)
            {
                if (DBTransferManager.getManager(null) != null)
                {
                    doCleanup();
                }
                const int MIN = 60, SEC = 1000;
                System.Threading.Thread.Sleep(2 * MIN * SEC);
            }
        }
        private void doCleanup()
        {
            lock (activeLocks)
            {
                List<String> dropKey = new List<String>();
                foreach (String key in activeLocks.Keys)
                {
                    Lock l = activeLocks[key];
                    if (l.isWriteLock)
                    {
                        if (l.lockCreation.AddMinutes(W_TIMEOUT).CompareTo(new DateTime()) < 0)
                        {
                            if (DBTransferManager.getManager(null).abortDownload(key))
                            {
                                dropKey.Add(key);
                                //activeLocks.Remove(key);
                            }
                            else
                            {
                                l.lockCreation = l.lockCreation.AddMinutes(W_TIMEOUT);
                            }
                        }
                    }
                    else
                    {
                        if (l.lockCreation.AddMinutes(R_TIMEOUT).CompareTo(new DateTime()) < 0)
                        {
                            dropKey.Add(key);
                            //((ILocks)(this)).releaseReadLock(key);
                            //activeLocks.Remove(key);
                        }
                    }
                } //forEach
                ILocks ilock = (ILocks)this;
                foreach (string s in dropKey)
                {
                    Lock l = activeLocks[s];
                    if (l.isWriteLock)
                    {
                        ilock.releaseWriteLock(s);
                    }
                    else
                    {
                        ilock.releaseReadLock(s);
                    }
                }
            } //lock
        }

        bool ILocks.acquireReadLock(string filePath)
        {
            lock (activeLocks)
            {
                if (activeLocks.ContainsKey(filePath))
                {
                    Lock l = activeLocks[filePath];
                    if (l.isWriteLock)
                    {
                        return false;
                    }
                    else
                    {
                        activeLocks[filePath].count++;
                        activeLocks[filePath].lockCreation = new DateTime();
                        return true;
                    }
                }
                else if (distLockMgr.acquireReadLock(filePath))
                {
                    Lock l = new Lock();
                    l.count = 1;
                    l.isWriteLock = false;
                    l.lockCreation = new DateTime();
                    activeLocks.Add(filePath, l);
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        bool ILocks.acquireWriteLock(string filePath)
        {
            lock (activeLocks)
            {
                if (activeLocks.ContainsKey(filePath))
                {
                    return false;
                }
                else if (distLockMgr.acquireWriteLock(filePath))
                {
                    Lock l = new Lock();
                    l.count = 1;
                    l.isWriteLock = true;
                    l.lockCreation = new DateTime();
                    activeLocks.Add(filePath, l);
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        bool ILocks.existsAnyLock(string filePath)
        {
            if (activeLocks.ContainsKey(filePath))
            {
                return true;
            }
            else
            {
                return distLockMgr.existsAnyLock(filePath);
            }
        }

        bool ILocks.existsWriteLock(string filePath)
        {
            if (activeLocks.ContainsKey(filePath) && activeLocks[filePath].isWriteLock)
            {
                return true;
            }
            else
            {
                return distLockMgr.existsWriteLock(filePath);
            }
        }

        void ILocks.releaseReadLock(string filePath)
        {
            lock (activeLocks)
            {
                if (!activeLocks.ContainsKey(filePath))
                {
                    Console.WriteLine("WARNING: attempt to release non-acquired READ lock");
                    return;
                }
                if ((--activeLocks[filePath].count) == 0)
                {
                    distLockMgr.releaseReadLock(filePath);
                    if (!activeLocks.Remove(filePath))
                    {
                        Console.WriteLine("ERROR: releaseReadLock did not remove from local db");
                    }
                }
            }
        }

        void ILocks.releaseWriteLock(string filePath)
        {
            lock (activeLocks)
            {
                if (!activeLocks.ContainsKey(filePath))
                {
                    Console.WriteLine("WARNING: attempt to release non-acquired WRITE lock");
                    return;
                }
                distLockMgr.releaseWriteLock(filePath);
                if (!activeLocks.Remove(filePath))
                {
                    Console.WriteLine("ERROR: releaseWriteLock did not remove from local db");
                }
            }
        }

        void IDisposable.Dispose()
        {
            distLockMgr.Dispose();
        }
    }
}
