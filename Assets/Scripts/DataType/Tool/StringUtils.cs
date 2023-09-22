using System;

public static class StringUtils
{
    public static string GithubAssetToNormal(string input)
    {
        // Find the last index of the dot (for the extension)
        int lastDotIndex = input.LastIndexOf('.');

        // If a dot is found, strip the extension
        string strippedString = lastDotIndex >= 0 ? input.Substring(0, lastDotIndex) : input;

        // Replace remaining dashes with spaces
        string result = strippedString.Replace('-', ' ');

        return result;
    }
}