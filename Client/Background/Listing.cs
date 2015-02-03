using ClientWorker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using test.Model;
using System.Security.Principal;
using client;

namespace test.Background
{
    class Listing
    {
        private Timer aTimer;
        SearchFile sf = new SearchFile();
        cloudinfo cloudinfo;
        String cloudPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\DBLite";
        string email;
        WatcherUtil watcherUtil;
        List<String> allLocalDirs;
        public Listing(string email, WatcherUtil watcherUtil)
        {
            this.email = email;
            this.watcherUtil = watcherUtil;
        }

        public void startListing()
        {
            cloudinfo = sf.Listing();
            ListFolder(cloudinfo);
            //Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(300000);
            //Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.Enabled = true;

        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            watcherUtil.stopWatcher();
            cloudinfo cloudinfo = sf.Listing();
            watcherUtil.startWatcher();
        }

        public void stopWatching()
        {
            aTimer.Enabled = false;
            aTimer.Elapsed -= OnTimedEvent;
        }

        public void ListFolder(cloudinfo cloudinfo)
        {
            bool isExists = System.IO.Directory.Exists((cloudPath));

            if (!isExists)
            {
                System.IO.Directory.CreateDirectory((cloudPath));
            }

            String[] dirs = Directory.GetDirectories(cloudPath);

            cloudinfoContainer[] itemsField = cloudinfo.Items;

            allLocalDirs = getListOfdirs(dirs);

            foreach (cloudinfoContainer container in itemsField)
            {
                checkContainer(container, dirs);
                if (allLocalDirs.Contains(container.name))
                {
                    allLocalDirs.Remove(container.name);
                }
            }

            foreach (String dir in allLocalDirs)
            {
                String[] files = Directory.GetFiles(cloudPath + "\\" + dir);
                foreach (String file in files)
                {
                    Upload upload = new Upload();
                    var fileInfo = new FileInfo(file);
                    upload.DocUplaod(email, cloudPath, fileInfo.Length.ToString(), dir + "\\" + fileInfo.Name, new Common().getFileMD5(file), fileInfo.LastWriteTimeUtc.Ticks.ToString(), new Common().getOldHash(email, fileInfo));
                }


            }

        }


        private void checkContainer(cloudinfoContainer container, string[] dirs)
        {
            //check if the container is root folder
            if (container.name.Equals(email))
            {
                cloudinfoContainerFile[] fileField = container.file;
                if (!(fileField == null || fileField.Length == 0))
                {
                    foreach (cloudinfoContainerFile file in fileField)
                    {
                        //checkfile to upload or download    
                        //updateFile(file, cloudPath, container.name);
                        checkFileForContainer(file, cloudPath, container);
                    }

                }



            }

            //create container if it does not exist in local
            else if (!getListOfdirs(dirs).Contains(container.name))
            {
                DirectorySecurity securityRules = new DirectorySecurity();

                securityRules.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().Name, getSystemRights(container.access), AccessControlType.Allow));

                System.IO.Directory.CreateDirectory(cloudPath + "\\" + container.name, securityRules);

                cloudinfoContainerFile[] fileField = container.file;
                if (!(fileField == null || fileField.Length == 0))
                {
                    foreach (cloudinfoContainerFile file in fileField)
                    {
                        checkFileForContainer(file, cloudPath + "\\" + container.name, container);
                    }
                }

            }
            //directory exists checking the file
            else
            {
                String[] filesInLocalContainer = Directory.GetFiles(cloudPath + "\\" + container.name);
                cloudinfoContainerFile[] filesInCloud = container.file;
                //checking if file is present in cloud and in local
                if (!(filesInCloud == null || filesInCloud.Length == 0))
                {
                    foreach (cloudinfoContainerFile file in filesInCloud)
                    {
                        if (!getListOfdirs(filesInLocalContainer).Contains(file.name))
                        {
                            //file is not present in local so download
                            checkFileForContainer(file, cloudPath + "\\" + container.name, container);
                        }
                        else
                        {
                            //file is present in both local and cloud so compare
                            //checks if the file's timestamp and uploads or downloads the doc
                            updateFile(file, cloudPath + "\\" + container.name, container.name, file.name, container);

                        }

                    }
                    //checking if file is present in local and not on cloud
                    if (!(filesInLocalContainer == null || filesInLocalContainer.Length == 0))
                    {
                        foreach (String file in filesInLocalContainer)
                        {
                            bool isPresent = false;
                            var fileInfo = new FileInfo(file);
                            if (!(filesInCloud == null || filesInCloud.Length == 0))
                            {
                                foreach (cloudinfoContainerFile cloudfile in filesInCloud)
                                {
                                    if (cloudfile.name.Equals(fileInfo.Name))
                                    {
                                        isPresent = true;
                                    }
                                }
                            }
                            if (!isPresent)
                            {
                                //upload the file                                
                                Upload upload = new Upload();
                                upload.DocUplaod(container.name, cloudPath + "\\" + container.name, fileInfo.Length.ToString(), fileInfo.Name, new Common().getFileMD5(file), fileInfo.LastWriteTimeUtc.Ticks.ToString(), new Common().getOldHash(container, fileInfo));


                            }
                        }
                    }
                    else
                    {
                        if (!(filesInLocalContainer == null || filesInLocalContainer.Length == 0))
                        {
                            foreach (String file in filesInLocalContainer)
                            {
                                //upload each file
                                Upload upload = new Upload();
                                var fileInfo = new FileInfo(file);
                                upload.DocUplaod(container.name, cloudPath + "\\" + container.name, fileInfo.Length.ToString(), fileInfo.Name, new Common().getFileMD5(file), fileInfo.LastWriteTimeUtc.Ticks.ToString(), new Common().getOldHash(container, fileInfo));
                                //if (result)
                                {
                                    //MessageBox.Show("Uploaded Successfully");
                                }
                            }
                        }

                    }
                }
            }
        }

        private List<String> getListOfdirs(string[] dirs)
        {
            List<String> directories = new List<string>();
            foreach (string dir in dirs)
            {
                var fileInfo = new FileInfo(dir);
                directories.Add(Path.GetFileName(fileInfo.Name));
            }

            return directories;
        }


        private void checkFileForContainer(cloudinfoContainerFile file, string cloudPath, cloudinfoContainer container)
        {
            String filePath = cloudPath;
            String fileName = file.name.Replace("/", "\\"); ;
            if (fileName.Contains("\\"))
            {
                String[] files = fileName.Split('\\');
                fileName = files.Last();
                int totalFolders = files.Length - 1;
                string contName = "\\";
                for (int i = 0; i < totalFolders; i++)
                {
                    contName = contName + files[i];
                    bool isExists = System.IO.Directory.Exists(filePath + contName);

                    if (!isExists)
                    {
                        System.IO.Directory.CreateDirectory(filePath + contName);
                    }
                    //String[] dirs = Directory.GetDirectories(filePath + contName);
                    allLocalDirs.Remove(files[i]);
                    filePath = filePath + contName;
                    contName = "\\";

                }

            }
            //checkFile and upload or download
            updateFile(file, filePath, container.name, fileName, container);
        }

        private void updateFile(cloudinfoContainerFile file, string filePath, string containerName, string fileName, cloudinfoContainer container)
        {
            String[] files = Directory.GetFiles(filePath);

            if (getListOfdirs(files).Contains(fileName))
            {
                //check timestamp
                var fileInfo = new FileInfo(filePath + "\\" + fileName);

                //local.compateTo(cloud)
                //string format = "ddd dd MMM h:mm tt yyyy";
                String[] date = file.timestamp.Split('+');
                DateTime dateTime = Convert.ToDateTime(date[0]);

                //long t = TimeSpan.TryParse(file.timestamp, format,  out dateTime).Ticks;
                int i = fileInfo.LastWriteTimeUtc.CompareTo(dateTime);

                if (i < 0 && new Common().getFileMD5(filePath + "\\" + fileName) != file.md5 && (file.deleted == "False"))
                {
                    //download the cloud file @filePath + "\\" + file.name
                    downloadAsync da = new downloadAsync();
                    da.download(containerName, file.name, filePath, email, file.size);
                }
                else if (i > 0 && new Common().getFileMD5(filePath + "\\" + fileName) != file.md5)
                {
                    if (file.deleted != "False")
                    {
                        //delete the file in the local
                        File.Delete(filePath + "\\" + fileName);
                    }
                    else
                    {
                        //upload the local file filePath + "\\" + file.name
                        Upload upload = new Upload();
                        upload.DocUplaod(containerName, filePath, fileInfo.Length.ToString(), file.name,
                                                                    new Common().getFileMD5(filePath + "\\" + fileName),
                                                                            fileInfo.LastWriteTimeUtc.Ticks.ToString(), new Common().getOldHash(container, fileInfo));


                    }
                    //if (result)
                    {
                        //MessageBox.Show("Uploaded Successfully");
                    }
                }
            }
            else
            {
                if (file.deleted == "False")
                {
                    //download the file
                    downloadAsync da = new downloadAsync();
                    da.download(containerName, file.name, filePath, email, file.size);

                }

            }

        }

        private FileSystemRights getSystemRights(String access)
        {
            if (access == "owner")
            {
                return FileSystemRights.FullControl;
            }
            else if (access == "readwrite")
            {
                return FileSystemRights.FullControl;
            }
            else
            {
                return FileSystemRights.FullControl;
            }
        }
    }
}
