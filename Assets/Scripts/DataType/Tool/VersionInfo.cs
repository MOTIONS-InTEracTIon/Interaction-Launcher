using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public static class VersionInfo
{
    public static void WriteVersionToFile()
    {
        string version = "v" + Application.version;
        string filePath = Path.GetDirectoryName(Application.dataPath) + "/version.txt";

        File.WriteAllText(filePath, version);
    }
}