namespace MSBuild.Front.Nuget.IO
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Allows you to manage file system symbolic links
    /// <remarks>The current implementation works only on Windows operating system</remarks>
    /// </summary>
    public class LinkUtil : ILinkUtil
    {
        /// <summary>
        /// <see cref="ILinkUtil.CreateLink"/>
        /// </summary>
        public void CreateLink(string link, string source)
        {
            var absoluteLink = Path.GetFullPath(link);
            var absoluteSource = Path.GetFullPath(source);
            if (Directory.Exists(link)) Directory.Delete(link, true);

            try
            {
                CreateSymbolicLink(absoluteLink, absoluteSource, SymbolicLink.Directory);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    "[ERROR] Could not invoke kernel32.CreateSymbolicLink. Probable causes are: UAC is active, the current OS is not Windows or target hard drive is full",
                    ex);
            }
        }

        [DllImport("kernel32.dll")]
        private static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        private enum SymbolicLink
        {
            // ReSharper disable once UnusedMember.Local
            File = 0,
            Directory = 1
        }
         
    }
}