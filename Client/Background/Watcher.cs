using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Permissions;
using test.ui;
using ClientWorker;
using test.Model;
using System.Text.RegularExpressions;
using test.ClientManager;

namespace test
{
    class Watcher
    {
        // Create a new FileSystemWatcher and set its properties.
        FileSystemWatcher watcher = new FileSystemWatcher();

        //email of the logged in user
        private string email;

        //for multiple notifications on single update - bugfix
        private DateTime lastChange = DateTime.MinValue;

        //for diff publica nd private upload
        private string uploadPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\DBLite";

        //constructor
        public Watcher(String email)
        {
            this.email = email;
        }

        public void StartWatch()
        {
            //setting the DBLITE path
            watcher.Path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\DBLite";

            //Creating a folder if it does not exist
            bool isExists = System.IO.Directory.Exists((watcher.Path));
            if (!isExists)
            {
                System.IO.Directory.CreateDirectory((watcher.Path));
            }

            //opens DBLITE in windows explorer
            System.Diagnostics.Process.Start(watcher.Path);

            /* Watch for changes in LastAccess and LastWrite times, and
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            // Watches all files.
            watcher.Filter = "*.*";

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(onDeleted);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);

            watcher.IncludeSubdirectories = true;
            // Begin watching.
            watcher.EnableRaisingEvents = true;

        }

        public void stopWatching()
        {
            //Disable all events
            watcher.EnableRaisingEvents = false;

            //remove all events
            watcher.Changed -= new FileSystemEventHandler(OnChanged);
            watcher.Created -= new FileSystemEventHandler(OnChanged);
            watcher.Deleted -= new FileSystemEventHandler(onDeleted);
            watcher.Renamed -= new RenamedEventHandler(OnRenamed);

        }

        private void onDeleted(object sender, FileSystemEventArgs e)
        {
            //client
            // new Notify().notifyChange(e.FullPath.ToString(), "Delete File");
            //check if the file or folder is shared or not
            cloudinfoContainer container = getContainerIfShared(e.Name);
            if (container != null)
            {
                if (container.user != null && container.user.Length != 0)
                {
                    new Delete().delete(container.name, null);
                    return;
                }


            }

            if (e.Name.Contains('.'))
            {
                //new Delete().delete(container.name, e.Name);

                Regex regex = new Regex(@"\bDBLite\\\b");
                String[] nameSplit = regex.Split(e.FullPath);
                if (nameSplit[1].Contains("\\"))
                {
                    Regex regexForBacklash = new Regex(@"\\\b");
                    String[] filenameSplit = regexForBacklash.Split(nameSplit[1], 2);
                    cloudinfoContainer contain = getContainerIfShared(filenameSplit[0]);
                    if (contain != null)
                    {
                        if (contain.user != null && contain.user.Length != 0)
                        {
                            new Delete().delete(contain.name, filenameSplit[1].Replace('\\', '/'));
                            return;
                        }
                        else
                        {
                            new Delete().delete(email, e.Name.Replace('\\', '/'));
                        }
                    }
                    else
                    {
                        new Delete().delete(email, e.Name.Replace('\\', '/'));
                    }

                }




            }


        }

        private cloudinfoContainer getContainerIfShared(String e)
        {
            cloudinfo cloud = SearchFile.cloud;
            cloudinfoContainer[] itemsField = cloud.Items;


            foreach (cloudinfoContainer container in itemsField)
            {
                if (container.name == e)
                {
                    return container;
                }
            }
            return null;
        }



        private void delete(String folderPath, String changeType)
        {
            String[] localFiles = Directory.GetFiles(folderPath);
            String[] localDirs = Directory.GetDirectories(folderPath);
            // String[] files = e.FullPath.ToString().Split('\\');
            foreach (String file in localFiles)
            {
                Regex regex = new Regex(@"\bDBLite\\\b");
                String[] nameSplit = regex.Split(file);
                if (new Delete().delete(email, nameSplit[1].Replace('\\', '/')))
                {

                    //Notifying the change
                    new Notify().notifyChange(file, changeType);
                    new Notify().notifyChange(file, "Delete Successful");
                }
                else
                {
                    new Notify().notifyChange(file, "Delete ERROR");
                }
            }

            foreach (String dir in localDirs)
            {
                delete(folderPath + "\\" + dir, changeType);
                //delete the file in the local
                try
                {
                    Directory.Delete(dir);
                }
                catch (Exception e)
                {
                    new Notify().notifyChange(dir, "Unable to delete");
                }

            }
        }



        // Define the event handlers. 
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            //getting the lastwrite time to check with lastRead to avoid multiple updates - bugfix
            DateTime lastWriteTime = File.GetLastWriteTime(e.FullPath);

            //checking with the last change
            if (lastWriteTime != lastChange)
            {
                //setting the lastVhange time
                lastChange = lastWriteTime;

                //Getting the changed file info to upload
                var fileInfo = new FileInfo(e.FullPath.ToString());
                Regex regex = new Regex(@"\bDBLite\\\b");
                Regex regexForBacklash = new Regex(@"\\\b");
                String[] nameSplit = regex.Split(e.FullPath.ToString(), 2);
                FileAttributes attr = File.GetAttributes(e.FullPath.ToString());
                String[] fName = regexForBacklash.Split(nameSplit[1], 2);
                //detect whether its a directory or file
                if (!((attr & FileAttributes.Directory) == FileAttributes.Directory))
                {
                    String fileName;
                    if (nameSplit[1].Contains("\\"))
                    {
                        String path = uploadPath;
                        String[] files = nameSplit[1].Split('\\');
                        fileName = files.Last();
                        int totalFolders = files.Length - 1;

                        string contName = "\\";
                        for (int i = 0; i < totalFolders; i++)
                        {
                            contName = contName + files[i];
                            path = path + contName;
                            contName = "\\";
                        }
                        cloudinfo cloud = SearchFile.cloud;

                        cloudinfoContainer[] itemsField = cloud.Items;
                        if (!(itemsField == null || itemsField.Length == 0))
                        {
                            bool isShared = false;
                            foreach (cloudinfoContainer container in itemsField)
                            {
                                if ((container.name.Equals(files[0])))
                                {
                                    if ((container.user != null && container.user.Length != 0 || container.access != "owner"))
                                    {
                                        isShared = true;
                                        //upload the file
                                        if (new Upload().DocUplaod(files[0], path, fileInfo.Length.ToString(), fName[1].Replace('\\', '/'),
                                                                        new Common().getFileMD5(e.FullPath.ToString()),
                                                                                    fileInfo.LastWriteTimeUtc.Ticks.ToString(), new Common().getOldHash(container, fileInfo)))
                                        {
                                            //Notifying the change
                                            new Notify().notifyChange(e.FullPath.ToString(), e.ChangeType.ToString());
                                            new Notify().notifyChange(e.FullPath.ToString(), "Upload Successful");
                                        }
                                        
                                        
                                    }


                                }
                            }
                            if (!isShared)
                            {
                                //upload the file
                                if (new Upload().DocUplaod(email, path, fileInfo.Length.ToString(), nameSplit[1].Replace("\\", "/"),
                                                                       new Common().getFileMD5(e.FullPath.ToString()),
                                                                                   fileInfo.LastWriteTimeUtc.Ticks.ToString(), new Common().getOldHash(email, fileInfo)))
                                    {
                                        //Notifying the change
                                        new Notify().notifyChange(e.FullPath.ToString(), e.ChangeType.ToString());
                                        new Notify().notifyChange(e.FullPath.ToString(), "Upload Successful");
                                    }
                                    
 
                                }
                               
                            }
                        

                    }


                    else
                    {
                        //upload the file
                        if (new Upload().DocUplaod(email, uploadPath, fileInfo.Length.ToString(), nameSplit[1],
                                                        new Common().getFileMD5(e.FullPath.ToString()),
                                                                    fileInfo.LastWriteTimeUtc.Ticks.ToString(), new Common().getOldHash(email, fileInfo)))
                        {
                            //Notifying the change
                            new Notify().notifyChange(e.FullPath.ToString(), e.ChangeType.ToString());
                            new Notify().notifyChange(e.FullPath.ToString(), "Upload Successful");
                        }
                        
                    }

                }

            }
        }



        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            Notify notify = new Notify();
            notify.notifyChange(e.FullPath.ToString(), e.ChangeType.ToString());
        }
    }
}
