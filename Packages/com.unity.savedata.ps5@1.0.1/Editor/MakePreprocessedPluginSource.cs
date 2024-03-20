#if UNITY_EDITOR && UNITY_PS5
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Diagnostics;
using System;
using System.IO;


public class MakePreprocessedPluginSource : IPreprocessBuildWithReport
{
      public int callbackOrder => 0;


        // could be made more effiecent ... perhaps just returning the sdk string rather than the int
        private static int SDKVersion()
        {
            int version = 0;
            string sdkPath = System.Environment.GetEnvironmentVariable("SCE_PROSPERO_SDK_DIR");
            if (sdkPath != null)
            {
                string versionFile = Path.Combine(sdkPath, "target/include/sdk_version.h");
                if (File.Exists(versionFile))
                {
                    string[] lines = File.ReadAllLines(versionFile);
                    foreach (string line in lines)
                    {
                        if (line.Contains("SCE_PROSPERO_SDK_VERSION"))
                        {
                            if (line.Contains("#define"))
                            {
                                Console.WriteLine("[ps5] Found SDK version: " + versionFile + ": " + line);
                                string[] words = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                Console.WriteLine("[ps5] words {0} {1} {2}", words[0], words[1], words[2]);
                                if (words[2].StartsWith("(0x"))
                                {
                                    string verstring = words[2].Substring(3, 8);
                                    version = Convert.ToInt32(verstring, 16);
                                    Console.WriteLine("[ps5] sdk version data {0} {1} ", verstring, version);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return version;
        }


      // create a preprocessed package source file that can be directly passed into the il2cpp build process. Only recreates it if detects source code change.
      public void OnPreprocessBuild(BuildReport report)
      {
            ProcessStartInfo processInfo;
            Process process;


            System.Console.WriteLine($"SDKVersion():0x{SDKVersion():x}");


            string packagename="com.unity.savedata.ps5";
            string sourcepath = $"Packages/{packagename}/Source~";  // this will correctly handle packages wherever they are located.


            string filepath = $"{sourcepath}/savedata_lump.cpp";
            string fullpath = Path.GetFullPath(filepath);

            FileInfo fi = new FileInfo(fullpath);
            string preprocessedSourcePath = Path.GetFullPath($"{fi.Directory}/../Plugins/PS5/Source/{packagename}_preprocessed.cpp");
            var lastwritepre = File.GetLastWriteTimeUtc(preprocessedSourcePath);

            String test = $"//0x{SDKVersion():x} sdk\n//";
            byte[] currentSdk =  System.Text.Encoding.ASCII.GetBytes(test);

            byte[] buffer = new byte[currentSdk.Length];
            bool sdkVersionsMatch = true;

            if (File.Exists(preprocessedSourcePath))
            {
                using(FileStream
                        fileStream = new FileStream(preprocessedSourcePath, FileMode.Open, FileAccess.Read))
                    {
                        // Set the stream position to the beginning of the file.
                        fileStream.Seek(0, SeekOrigin.Begin);
                        fileStream.Read( buffer,0,currentSdk.Length);
                    }
                for (int i=0;i<buffer.Length;i++)
                {
                    if (currentSdk[i] != buffer[i])
                    {
                        sdkVersionsMatch=false;
                        break;
                    }
                }
            }
            else
            {
                sdkVersionsMatch = false;   // if the preprocessed source file doesn't exist then there is no point in doing any of the other tests, we always need a rebuild
            }

            System.Console.WriteLine($"currentSdk.Length:{currentSdk.Length} buffer.Length:{buffer.Length} sdkVersionsMatch:{sdkVersionsMatch}");

            // if the sdk string doesn't match then we always need to rebuild the preprocessed file
            if (sdkVersionsMatch)
            {
                // if the sdk string does match, then we need to check the last write time of all the files
                DateTime latestWriteTime = DateTime.MinValue;
                DirectoryInfo di = new DirectoryInfo(sourcepath);
                foreach (var file in  di.GetFiles("*.cpp", SearchOption.AllDirectories))
                {
                    var fileLW = File.GetLastWriteTimeUtc(file.FullName);
                    latestWriteTime = (fileLW > latestWriteTime) ? fileLW: latestWriteTime;
                }
                foreach (var file in  di.GetFiles("*.h", SearchOption.AllDirectories))
                {
                    var fileLW = File.GetLastWriteTimeUtc(file.FullName);
                    latestWriteTime = (fileLW > latestWriteTime) ? fileLW: latestWriteTime;
                }

                System.Console.WriteLine($"latestWriteTime:{latestWriteTime} lastwritepre:{lastwritepre}");
                if (latestWriteTime < lastwritepre)
                {
                    System.Console.WriteLine($"skipping package prebuild step. No changes detected");
                    return;
                }
            }





            File.SetAttributes(preprocessedSourcePath, FileAttributes.Normal);
            string Command = $"-E \"{fullpath}\" -o \"{preprocessedSourcePath}\"";
            var sdkloc = Environment.GetEnvironmentVariable("SCE_PROSPERO_SDK_DIR");
            processInfo = new ProcessStartInfo($"{sdkloc}/host_tools/bin/prospero-clang.exe", Command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            process = Process.Start(processInfo);
            StreamReader streamReader = process.StandardError;
            process.WaitForExit();
            if (process.ExitCode!=0)
                throw new Exception(streamReader.ReadToEnd());  // if we get a error throw an exception with the whole stderr output

            // Here we take advantage of the fact that the preprocessed file starts with a range of comments to overwrite the start of the file with the sdk version.
            using(FileStream
                    fileStream = new FileStream(preprocessedSourcePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    // Set the stream position to the beginning of the file.
                    fileStream.Seek(0, SeekOrigin.Begin);
                    fileStream.Write( currentSdk );
                    fileStream.Seek(0, SeekOrigin.End);
                }
      }
}

#if UNITY_2020 || UNITY_2021
public class SaveDataPS5ProjectPostProcess : UnityEditor.PS5.IPostProcessPS5
{
    public void OnPostProcessPS5(string projectFolder, string outputFolder)
    {
        string directoryPath = outputFolder+"\\Data\\SourcePlugins";
        if (Directory.Exists(directoryPath))
            Directory.Delete(directoryPath, true);
    }

    public int callbackOrder { get { return 1; } }
}
#endif

#endif
