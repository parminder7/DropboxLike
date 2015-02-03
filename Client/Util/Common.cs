using ClientWorker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using test.Model;

namespace test
{
    class Common
    {
        public Boolean validateEmail(string email)
        {
            RegexUtilities util = new RegexUtilities();
            if (!util.IsValidEmail(email))
            {
                return false;
            }
            return true;
        }

        public Boolean isEmailPwdNull(string email, string password)
        {
            if (email == "" || password == "")
            {
                return true;
            }
            return false;
        }

        public FileStream isFileInUse(string path)
        {
            try
            {
                FileStream fs = File.OpenRead(path);
                return fs;
            }
            catch (IOException ex)
            {
                return null;
            }
        }

        public String getFileMD5(String file)
        {
            try
            {
                if (File.Exists(file))
                {
                    FileStream fstream = new FileStream(file, FileMode.Open, FileAccess.Read);
                    byte[] hash;
                    fstream.Seek(0, SeekOrigin.Begin);
                    using (var md5 = MD5.Create())
                    {
                        hash = md5.ComputeHash(fstream);
                    }

                    fstream.Seek(0, SeekOrigin.Begin);
                    string value = Convert.ToBase64String(hash);
                    fstream.Close();
                    return value;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception e)
            {
                return null;
            }
            
            
        }

        public string getOldHash(cloudinfoContainer container, FileInfo fileInfo)
        {
            string hash = null;
            cloudinfoContainerFile[] filesInCloud = container.file;
            if (!(filesInCloud == null || filesInCloud.Length == 0))
            {
                foreach (cloudinfoContainerFile file in filesInCloud)
                {
                    if (file.name.Equals(fileInfo.Name))
                    {
                        hash = file.md5;
                        return hash;
                    }
                }
            }
            return hash;
        }


        public string getOldHash(string email, FileInfo fileInfo)
        {
            cloudinfo cloud = SearchFile.cloud;
            cloudinfoContainer[] itemsField = cloud.Items;

            
            foreach (cloudinfoContainer container in itemsField)
            {
                if (container.name == email)
                {
                    return getOldHash(container, fileInfo);
                }
                
            }

            return null;
        }
    }
}
