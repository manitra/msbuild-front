using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSBuild.Front.Nuget.IO
{
    public interface IDirectoryUtil
    {
        /// <summary>
        /// Copy all subdirectories and files
        /// </summary>
        /// <param name="sourceDirName">The full path of the source directory</param>
        /// <param name="destDirName">The full path of the destination directory</param>
        /// <param name="copySubDirs">Whether to copy sub directories</param>
        void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs);
    }
}
