using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using test.ui;
using ClientWorker;

namespace test.Background
{
    public class WatcherUtil
    {
        //private static Thread workerThread;
        Watcher watch;
        String email;
        Listing list;
        public WatcherUtil(String email)
        {
            this.email = email;
        }

        public  void startWatcher()
        {
             watch = new Watcher(email); 
             watch.StartWatch();
                
            //workerThread = new Thread(watch.StartWatch);
            //workerThread.IsBackground = true;
            //workerThread.Start();
        }

        public void startServerSync()
        {
            new Notify().notifyChange(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\DBLite", "Sync started");
            list = new Listing(email, this);
            list.startListing();
        }

        public  void stopWatcher()
        {
            watch.stopWatching();
        }

        public void stopServerSync()
        {
            list.stopWatching();
        }

        public void startListing()
        {
            SearchFile sf = new SearchFile();
            list.ListFolder(sf.Listing());
        }

    }
}
