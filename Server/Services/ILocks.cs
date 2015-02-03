using System;
namespace Server.Services
{
    public interface ILocks : IDisposable
    {
        bool acquireReadLock(string filePath);
        bool acquireWriteLock(string filePath);
        bool existsAnyLock(string filePath);
        bool existsWriteLock(string filePath);
        void releaseReadLock(string filePath);
        void releaseWriteLock(string filePath);
    }
}
