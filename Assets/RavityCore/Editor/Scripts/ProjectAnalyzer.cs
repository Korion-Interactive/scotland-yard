using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ravity.Editor
{
    public static class ProjectAnalyzer
    {
        [MenuItem("Ravity/Count Lines of/C# Code")]
        public static void CountLinesOfCode()
        {
            CountLinesWithExtension("cs");
        }

        [MenuItem("Ravity/Count Lines of/Unity Scenes")]
        public static void CountLinesOfUnityScenes()
        {
            CountLinesWithExtension("unity");
        }

        [MenuItem("Ravity/Count Lines of/Materials")]
        public static void CountLinesOfMaterials()
        {
            CountLinesWithExtension("mat");
        }

        [MenuItem("Ravity/Count Lines of/Animation Clips")]
        public static void CountLinesOfAnimationClips()
        {
            CountLinesWithExtension("anim");
        }

        [MenuItem("Ravity/Count Lines of/Animation Controller")]
        public static void CountLinesOfAnimationControllers()
        {
            CountLinesWithExtension("controller");
        }

        [MenuItem("Ravity/Count Lines of/Prefabs")]
        public static void CountLinesOfPrefabs()
        {
            CountLinesWithExtension("controller");
        }

        [MenuItem("Ravity/Count Lines of/Shader Code")]
        public static void CountLinesOfShaders()
        {
            CountLinesWithExtension("shader");
        }

        [MenuItem("Ravity/Count Lines of/Text")]
        public static void CountLinesOfText()
        {
            CountLinesWithExtension("txt");
        }

        private static void CountLinesWithExtension(string fileExtension)
        {
            // analyse entire project
            string basePath = System.IO.Directory.GetCurrentDirectory() + "/Assets";
            CountLines(basePath, fileExtension);

            // analyse subfolder which should be modules
            string[] modules = Directory.GetDirectories(basePath);
            foreach (string module in modules)
            {
                CountLines(module, fileExtension);
            }
        }

        private class AnalysisData
        {
            public List<FileStats> Files = new List<FileStats>();
            public Dictionary<string, int> WordOccurrence = new Dictionary<string, int>();
        }

        private struct FileStats
        {
            public readonly string Name;
            public readonly int NumberOfLines;
            public readonly int EmptyLines;

            public FileStats(string name, int numberOfLines, int emptyLines)
            {
                this.Name = name;
                this.NumberOfLines = numberOfLines;
                this.EmptyLines = emptyLines;
            }
        }

        private static void CountLines(string folder, string fileExtension)
        {
            if (Directory.Exists(folder) == false)
            {
                Debug.LogWarning("Directory is missing: " + folder);
                return;
            }

            // analyse
            AnalysisData analysisData = new AnalysisData();
            ProcessDirectory(analysisData, folder, fileExtension);

            int totalLinesOfCode = 0;
            int emptyLines = 0;
            foreach (FileStats f in analysisData.Files)
            {
                totalLinesOfCode += f.NumberOfLines;
                emptyLines += f.EmptyLines;
            }

            // output result
            StringBuilder output = new StringBuilder();
            output.Append("'");
            output.Append(folder);
            output.Append("' with ");
            output.Append(totalLinesOfCode.ToString("N0"));
            output.Append(" LOC (");
            output.Append(emptyLines.ToString("N0"));
            output.Append(" empty) in ");
            output.Append(analysisData.Files.Count);
            output.Append(" files");
            output.Append(" and ");
            output.Append(analysisData.WordOccurrence.Count);
            output.Append(" different words");

            output.Append("\n");

            // show WordOccurrenceList sorted by Occurrences
            List<KeyValuePair<string, int>> wordOccurrenceList = new List<KeyValuePair<string, int>>();
            foreach (string key in analysisData.WordOccurrence.Keys)
            {
                wordOccurrenceList.Add(new KeyValuePair<string, int>(key, analysisData.WordOccurrence[key]));
            }
			wordOccurrenceList.Sort(delegate(KeyValuePair<string, int> x, KeyValuePair<string, int> y) {
                return y.Value.CompareTo(x.Value);
            });

            int topWords = 0;
            foreach (KeyValuePair<string, int> wordOccurrence in wordOccurrenceList)
            {
                output.Append(wordOccurrence.Key);
                output.Append(" = ");
                output.Append(wordOccurrence.Value);
                output.Append("\n");
                topWords++;
                if (topWords > 100)
                {
                    break;
                }
            }

            // show files sorted by NumberOfLines
			analysisData.Files.Sort(delegate(FileStats x, FileStats y) {
                return y.NumberOfLines.CompareTo(x.NumberOfLines);
            });

            foreach (FileStats file in analysisData.Files)
            {
                string shortedPath = file.Name.Replace(folder, string.Empty);
                output.Append(shortedPath);
                output.Append(" = ");
                output.Append(file.NumberOfLines);
                output.Append("\n");
            }
            Debug.Log(output.ToString());
        }

        private static void ProcessDirectory(AnalysisData analysisData, string dir, string fileExtension)
        {
            // process files
            string[] fileNames = Directory.GetFiles(dir, "*." + fileExtension);
            foreach (string fileName in fileNames)
            {
                ProcessFile(analysisData, fileName);
            }

            // process subfolders
            string[] subfolder = Directory.GetDirectories(dir);
            foreach (string folder in subfolder)
            {
                ProcessDirectory(analysisData, folder, fileExtension);
            }
        }

        private static void ProcessFile(AnalysisData analysisData, string filename)
        {
            StreamReader reader = File.OpenText(filename);
            int numberOfLines = 0;
            int emptyLines = 0;
            while (reader.Peek() >= 0)
            {
                string line = reader.ReadLine();
                numberOfLines++;

                if (string.IsNullOrWhiteSpace(line))
                {
                    emptyLines++;
                    continue;
                }

				char[] separator = {' ',';','{','}','=','+','-','%','!','[',']','\'',
					'-','/','*','\t',')','(','"',',','<','>','&','|','.',':'
                };
                string[] words = line.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string word in words)
                {
                    string trimmedWord = word.Trim();
                    if (analysisData.WordOccurrence.ContainsKey(trimmedWord))
                    {
                        analysisData.WordOccurrence[trimmedWord]++;
                    }
                    else
                    {
                        analysisData.WordOccurrence.Add(trimmedWord, 1);
                    }
                }
            }
            analysisData.Files.Add(new FileStats(filename, numberOfLines, emptyLines));
            reader.Close();
        }
    }
}