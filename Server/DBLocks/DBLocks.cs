using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DBLocks
{
    class DBLocks: IDisposable, Server.Services.ILocks
    {
        //private SqlConnection connection; <--bad idea, sigh
        private static DBLocks instance;
        private string hostname;
        private int pid;
        private int lockingId;
        private Boolean isValid = false;
        public static DBLocks initLocks(String hostname, int pid)
        {
            if (instance == null)
                instance = new DBLocks(hostname, pid);
            return instance;
        }
 
        public Boolean existsWriteLock(String filePath) 
        {
            if (!isValid)
            {
                throw new InvalidOperationException();
            }
            using (var conn = getDBAccess())
            {
                using (var stmt = conn.CreateCommand())
                {
                    stmt.CommandText = "SELECT * FROM LOCKS WHERE LockTarget = @lockObj AND WriteLock = 1";
                    stmt.Parameters.AddWithValue("@lockObj", filePath);
                    using (var reader = stmt.ExecuteReader())
                    {
                        return reader.HasRows;
                    }
                }
            }
        }
        public Boolean existsAnyLock(String filePath)
        {
            if (!isValid)
            {
                throw new InvalidOperationException();
            }
            using (var conn = getDBAccess())
            {
                using (var stmt = conn.CreateCommand())
                {
                    stmt.CommandText = "SELECT * FROM LOCKS WHERE LockTarget = @lockObj";
                    stmt.Parameters.AddWithValue("@lockObj", filePath);
                    using (var reader = stmt.ExecuteReader())
                    {
                        return reader.HasRows;
                    }
                }
            }
        }
        public Boolean acquireReadLock(String filePath)
        {
            if (!isValid)
            {
                throw new InvalidOperationException();
            }
            while (true)
            {
                //First check if a write lock exists
                if (existsWriteLock(filePath))
                {
                    return false;
                }
                using (var conn = getDBAccess())
                {
                    //try to create a new read lock
  
                        using (var transaction = conn.BeginTransaction())
                        {
                            var stmt = conn.CreateCommand();
                            stmt.CommandText = "INSERT INTO Locks VALUES (@lockObj, 0, NULL, 1);";
                            stmt.CommandText += " INSERT INTO ReadLocks VALUES (@holder, @lockObj)";
                            stmt.Parameters.AddWithValue("@lockObj", filePath);
                            stmt.Parameters.AddWithValue("@holder", this.lockingId);
                            stmt.Transaction = transaction;                            
                            try
                            {
                                stmt.ExecuteNonQuery();
                                transaction.Commit();
                                return true;
                            }
                            catch (SqlException)
                            {
                                transaction.Rollback();
                                Console.WriteLine("DBLocks#acquireReadLock: could not create new read lock");
                            }
                        } //using
                        using (var transaction = conn.BeginTransaction())
                        {
                            var stmt = conn.CreateCommand();
                            stmt.CommandText = "UPDATE Locks SET Count = Count + 1 WHERE LockTarget = @lockObj;";
                            stmt.CommandText += " INSERT INTO ReadLocks VALUES (@holder, @lockObj)";
                            stmt.Parameters.AddWithValue("@lockObj", filePath);
                            stmt.Parameters.AddWithValue("@holder", this.lockingId);
                            stmt.Transaction = transaction;
                            
                            try
                            {
                                stmt.ExecuteNonQuery();
                                transaction.Commit();
                                return true;
                            }
                            catch (SqlException)
                            {
                                transaction.Rollback();
                            }
                        }
                    }
                } //while true
            
        }
        public Boolean acquireWriteLock(String filePath)
        {
            if (!isValid)
            {
                throw new InvalidOperationException();
            }
            while (true)
            {
                //First check if a write lock exists
                using (var conn = getDBAccess())
                {
                    if (existsAnyLock(filePath))
                    {
                        return false;
                    }
                    //try to create a new read lock
                    using (var stmt = getDBAccess().CreateCommand())
                    {
                        stmt.CommandText = "INSERT INTO Locks VALUES (@lockObj, 1, @holder, NULL)";
                        stmt.Parameters.AddWithValue("@lockObj", filePath);
                        stmt.Parameters.AddWithValue("@holder", this.lockingId);
                        try
                        {
                            stmt.ExecuteNonQuery();
                            return true;
                        }
                        catch (SqlException) { }
                    }
                }
            }
        }

        public void releaseWriteLock(String filePath)
        {
            if (!isValid)
            {
                throw new InvalidOperationException();
            }
            using (var conn = getDBAccess())
            {
                using (var stmt = conn.CreateCommand())
                {
                    stmt.CommandText = "DELETE FROM Locks WHERE LockTarget = @lockObj AND Holder = @holder";
                    stmt.Parameters.AddWithValue("@lockObj", filePath);
                    stmt.Parameters.AddWithValue("@holder", this.lockingId);
                    try
                    {
                        int nRows = stmt.ExecuteNonQuery();
                        if (nRows < 1)
                        {
                            Console.WriteLine("WARNING: write lock not released for {0} - no rows affected", filePath);
                        }
                    }
                    catch (SqlException e)
                    {
                        Console.WriteLine("WARNING: write lock not released for {0}", filePath);
                        Console.WriteLine("SQL EXCEPTION #{0}: {1}", e.Number, e.Message);
                    }
                }
            }
        }

        public void releaseReadLock(String filePath)
        {
            if (!isValid)
            {
                throw new InvalidOperationException();
            }
            using (var conn = getDBAccess())
            {
                using (var stmt = conn.CreateCommand())
                {
                    stmt.CommandText = "DELETE FROM ReadLocks WHERE LockTarget = @lockObj AND LockID = @holder";
                    stmt.Parameters.AddWithValue("@lockObj", filePath);
                    stmt.Parameters.AddWithValue("@holder", this.lockingId);
                    try
                    {
                        int nRows = stmt.ExecuteNonQuery();
                        if (nRows < 1)
                        {
                            Console.WriteLine("WARNING: read lock not released for {0} - no rows affected", filePath);
                        }
                    }
                    catch (SqlException e)
                    {
                        Console.WriteLine("WARNING: read lock not released for {0}", filePath);
                        Console.WriteLine("SQL EXCEPTION #{0}: {1}", e.Number, e.Message);
                    }
                }
            }
        }
        private DBLocks(String hostname, int pid)
        {
            this.hostname = hostname;
            this.pid = pid;
            //connection = getDBAccess();            
            registerInDB();            
        }

        private void registerInDB()
        {
            using (var conn = getDBAccess())
            {
                using (var stmt = conn.CreateCommand())
                {
                    stmt.CommandText = "DELETE FROM Lockholder WHERE SERVER = @server";
                    stmt.Parameters.AddWithValue("@server", hostname);
                    int nRows = stmt.ExecuteNonQuery();
                    if (nRows > 0)
                    {
                        Console.WriteLine("DBLocks: Recovered from previous crash");
                    }
                    else
                    {
                        Console.WriteLine("DBLocks: No issues detected with previous shutdown");
                    }
                }
                using (var stmt = conn.CreateCommand())
                {
                    stmt.CommandText = "INSERT INTO Lockholder VALUES (@server, @pid)";
                    stmt.Parameters.AddWithValue("@server", hostname);
                    stmt.Parameters.AddWithValue("@pid", pid);
                    int nRows = stmt.ExecuteNonQuery();
                    if (nRows != 1)
                    {
                        Console.WriteLine("Could not register in locks db, exiting.");
                        Environment.Exit(0);
                    }
                }

                using (var stmt = conn.CreateCommand())
                {
                    stmt.CommandText = "SELECT LockID FROM Lockholder WHERE SERVER = @server AND PID = @pid";
                    stmt.Parameters.AddWithValue("@server", hostname);
                    stmt.Parameters.AddWithValue("@pid", pid);
                    try
                    {
                        object lockid = stmt.ExecuteScalar();
                        this.lockingId = (int)lockid;
                        Console.WriteLine("Acquired lock instance: {0}", this.lockingId);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Could find lockholder id in db, exiting.");
                        Environment.Exit(0);
                    }
                }
                this.isValid = true;
            }
        }
        private SqlConnection getDBAccess()
        {
            //String connectionString = conSettings.ConnectionString;
            String connectionString = "Data Source=i2fgak0wp2.database.windows.net;Initial Catalog=FileLocks;User ID=myadmin@i2fgak0wp2;Password=ABC!1234;MultipleActiveResultSets=true";

            try
            {
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                return connection;
                //Console.WriteLine("Set");
            }
            catch (SqlException e)
            {
                Console.WriteLine("SQL EXCEPTION " + e.Message);
                throw;
            }
        }
        public void Dispose()
        {
            if (!isValid)
            {
                return;
            }
            /* Not clear why this doesn't work the first time... */
            while (true)
            {
                try
                {
                    using (var stmt = getDBAccess().CreateCommand())
                    {
                        //stmt.Connection = connection;
                        stmt.CommandText = "DELETE FROM Lockholder WHERE LockID = @lock";
                        stmt.Parameters.AddWithValue("@lock", this.lockingId);
                        int nRows = stmt.ExecuteNonQuery();
                        if (nRows != 1)
                        {
                            Console.WriteLine("SEVERE: possible issue deleting lock id from locks db!!");
                            //Environment.Exit(0);
                        }
                    }
                    isValid = false;
                    Console.WriteLine("INFO: lock instance {0} deleted from database.", this.lockingId);
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("SEVERE: could not delete lock from db, reason was {0}\r\nStill trying...", e.Message);
                }
            }
        }
    }
}
