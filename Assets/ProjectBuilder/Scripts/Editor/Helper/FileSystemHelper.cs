using System.IO;
using UnityEngine;

namespace Ravity.ProjectBuilder
{
    public static class FileSystemHelper
    {
        public static string ProjectRootPath => Application.dataPath.Replace("Assets",string.Empty);

        public static bool CopyFolder(string from, string to)
        {
            if (Directory.Exists(from))
            {
                string[] files = Directory.GetFiles(from,"*.*",SearchOption.AllDirectories);
                foreach (string file in files)
                {     
                    if (file.Contains("/."))
                    {
                        // ignore .svn and .DS_Store etc.
                        continue;
                    }
					
                    string destination = file.Replace(from,to);
					
                    string directory = Path.GetDirectoryName(destination);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
					
                    CopyFile(file,destination);
                }
                return true;
            }
            else
            {
                Debug.LogWarning("Could not find " + from);
            }
            return false;
        }

        public static void CopyFile(string from, string to)
        {
            if (File.Exists(from))
            {
                File.Copy(from,to,true);
            }
            else
            {
                Debug.LogWarning($"{nameof(FileSystemHelper)}.{nameof(CopyFile)} file does not exist: " + from);
            }
        }
    }
}