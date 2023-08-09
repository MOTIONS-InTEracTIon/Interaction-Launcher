using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class FolderHelper
{

    public static bool DeleteFoldersRecursively(string folderPath, List<string> protectedFolderNames)
    {
        // Check if there are any subfolders
        string[] subFolders = Directory.GetDirectories(folderPath);

        bool shouldDelete = true;
        foreach (string subFolder in subFolders)
        {
            string folderName = Path.GetFileName(subFolder);

            if (protectedFolderNames.Contains(folderName))
            {
                shouldDelete = false;
                break; // Stop searching if a protected folder is found
            }
            else
            {
                // Recursively check subfolders
                if (!DeleteFoldersRecursively(subFolder, protectedFolderNames))
                {
                    shouldDelete = false;
                }
            }
        }

        if (shouldDelete)
        {
            // Delete the current folder and its contents
            Directory.Delete(folderPath, true);
        }

        return shouldDelete;
    }
}
