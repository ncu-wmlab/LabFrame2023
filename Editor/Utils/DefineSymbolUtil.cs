using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public static class DefineSymbolsUtil
{
    private const char DEFINE_SEPARATOR = ';';
    private static readonly List<string> _allDefines = new List<string>();
    
    public static void Add(params string[] defines)
    {
        _allDefines.Clear();
        _allDefines.AddRange(GetDefines());
        _allDefines.AddRange(defines.Except(_allDefines));
        UpdateDefines(_allDefines);
    }

    public static void Remove(params string[] defines)
    {
        _allDefines.Clear();
        _allDefines.AddRange(GetDefines().Except(defines));
        UpdateDefines(_allDefines);
    }

    public static void Clear()
    {
        _allDefines.Clear();
        UpdateDefines(_allDefines);
    }

    public static IEnumerable<string> GetDefines()
    {
        return PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(DEFINE_SEPARATOR).ToList();
    }

    private static void UpdateDefines(List<string> allDefines) 
    {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(
            EditorUserBuildSettings.selectedBuildTargetGroup, 
            string.Join(DEFINE_SEPARATOR.ToString(),
            allDefines.ToArray()));
    }
}