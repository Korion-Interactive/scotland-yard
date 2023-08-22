using System.IO;
using UnityEngine;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
namespace Ravity
{
    public static class FilePatchUtils
    {
        public static void InjectTextIntoFile(string filePath, string startMarker, string endMarker, string injectContent)
        {
            if (File.Exists(filePath) == false)
            {
                Debug.LogError("could not find file at filePath=" + filePath);
                return;
            }
            string content = File.ReadAllText(filePath);

            int startIndex = content.IndexOf(startMarker);
            if (startIndex <= 0)
            {
                Debug.LogError("could not find: " + startMarker);
                return;
            }

            int endIndex = content.IndexOf(endMarker,startIndex+startMarker.Length);
            if (endIndex <= 0)
            {
                Debug.LogError("could not find: " + endMarker);
                return;
            }
            endIndex -= 1;

            // remove old part
            int length = endIndex - startIndex + 1;
            content = content.Remove(startIndex, length);

            // insert replacement
            content = content.Insert(startIndex,startMarker + injectContent);

            // write to file
            File.WriteAllText(filePath,content);
        }
    }
}