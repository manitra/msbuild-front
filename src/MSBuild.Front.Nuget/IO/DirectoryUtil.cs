﻿using System.IO;
using System.Text.RegularExpressions;

namespace MSBuild.Front.Nuget.IO
{
    public class DirectoryUtil : IDirectoryUtil
    {
        public void DirectoryCopy(string sourceDirName, string destDirName, string excludedFolderPattern, bool copySubDirs)
        {
            if (Regex.IsMatch(sourceDirName, excludedFolderPattern))
            {
                return;
            }
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);
            var dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                var destFile = new FileInfo(temppath);

                if (destFile.Exists)
                {
                    if (destFile.LastWriteTime < file.LastWriteTime)
                    {
                        file.CopyTo(temppath, true);
                    }
                }
                else
                {
                    file.CopyTo(temppath);
                }
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, excludedFolderPattern, copySubDirs);
                }
            }
        }
    }
}
