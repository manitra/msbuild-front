namespace MSBuild.Front.Nuget.IO
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Kind of symbolic link
    /// </summary>
    public enum SymbolicLinkType
    {
        /// <summary>
        /// The symlink point to a file
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        File = 0,

        /// <summary>
        /// The symlink points to a directory
        /// </summary>
        Directory = 1
    }
}