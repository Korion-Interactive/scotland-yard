#if UNITY_EDITOR
using UnityEditor;

namespace Ravity.ProjectBuilder
{
    public static class PlayerSettingsHelper
    {
        public static void AddPreprocessor(BuildTargetGroup buildTarget, string addSymbol)
        {
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);

            // remove it first (to avoid duplicates)
            symbols = symbols.Replace(addSymbol,string.Empty);

            // add symbol
            symbols += ";" + addSymbol;

            // filter and save 
            FilterAndSavePreprocessors(buildTarget,symbols);
        }

        public static void RemovePreprocessor(BuildTargetGroup buildTarget,string removeSymbol)
        {
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
            // remove symbol
            symbols = symbols.Replace(removeSymbol,string.Empty);
            FilterAndSavePreprocessors(buildTarget,symbols);
        }

        private static void FilterAndSavePreprocessors(BuildTargetGroup buildTarget, string symbols)
        {
            symbols = symbols.Replace(";;;",";");
            symbols = symbols.Replace(";;",";");
            symbols = symbols.Trim(';');
            symbols = symbols.Trim();

            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget,symbols);
        }
    }
}
#endif