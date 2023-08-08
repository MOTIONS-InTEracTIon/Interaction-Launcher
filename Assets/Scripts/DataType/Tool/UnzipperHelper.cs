using UnityEngine;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.IO.Compression;
using Unity.VisualScripting;

using SharpZipLib = ICSharpCode.SharpZipLib.Zip;
using System;

public static class UnzipperHelper
{
    public static bool Unzip(string zipFilePath, string extractionPath)
    {
                try
        {
            using (var zipFile = new SharpZipLib.ZipFile(zipFilePath))
            {
                foreach (ZipEntry entry in zipFile)
                {
                    if (!entry.IsFile)
                        continue;

                    var entryStream = zipFile.GetInputStream(entry);
                    var entryPath = Path.Combine(extractionPath, entry.Name);

                    // Create directories if they don't exist
                    Directory.CreateDirectory(Path.GetDirectoryName(entryPath));

                    using (var outputStream = File.Create(entryPath))
                    {
                        entryStream.CopyTo(outputStream);
                    }
                }
            }

            Debug.Log("Zip extraction completed.");
            return true; // Return true to indicate success
        }
        catch (Exception e)
        {
            Debug.LogError("Zip extraction failed: " + e.Message);
            return false; // Return false to indicate failure
        }
    }
}
