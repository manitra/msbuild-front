namespace MSBuild.Front.Nuget.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Xml.Linq;

    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    /// <summary>
    /// For each dependencies of a given project, this task creates a symlink to its front-end modules folder right inside the target project 'app' folder
    /// </summary>
    public class IncludeFrontDependencies : Task
    {
        /// <summary>
        /// Should contains the absolute path to the project file (csproj) for which you want to include front end dependencies
        /// </summary>
        [Required]
        public string ProjectPath { get; set; }

        public override bool Execute()
        {
            var destDir = Path.Combine(Path.GetDirectoryName(ProjectPath), "app");
            var ignoreFile = Path.Combine(destDir, ".gitignore");

            if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
            File.WriteAllText(ignoreFile, ".gitignore" + Environment.NewLine);

            //Note that we don't handle recursion here because
            //the nested dependancies have been built already
            //which means their respective dependancies
            //will be in their own app folder
            foreach (var sourceDir in GetSourceDirs(ProjectPath))
            {
                //just means that there is no "app" folder in the referenced project
                //which means it's not a MSBuild.Front-ready project
                if (!Directory.Exists(sourceDir))
                {
                    Log.LogMessage(MessageImportance.Low, "Skipping '{0}' because it doesn't exist", sourceDir);
                    continue;
                }

                foreach (var directory in Directory.GetDirectories(sourceDir, "*", SearchOption.TopDirectoryOnly))
                {
                    var shortDirectoryName = Path.GetFileName(directory);
                    CreateLink(Path.Combine(destDir, shortDirectoryName), directory);
                    File.AppendAllText(ignoreFile, shortDirectoryName + Environment.NewLine);
                }
            }

            return true;
        }

        /// <summary>
        /// Get the list of the absolute path of the "app" folder of every dependancies of the given csproj file
        /// </summary>
        /// <param name="projectFile">The full path of a csproj file</param>
        /// <returns></returns>
        private IEnumerable<string> GetSourceDirs(string projectFile)
        {
            const string Ns = "http://schemas.microsoft.com/developer/msbuild/2003";
            var projectContent = XDocument.Load(projectFile);
            var projectDir = Path.GetDirectoryName(projectFile);

            foreach (var projectRefNode in projectContent.Descendants(XName.Get("ProjectReference", Ns)))
            {
                var nameNode = projectRefNode.Element(XName.Get("Name", Ns));
                var includeAttribute = projectRefNode.Attribute("Include");

                if (nameNode == null)
                {
                    Log.LogWarning("Found a project reference which lacks the 'Name' element. It will be ignored by MSBuild.Front. " + projectRefNode);
                    continue;
                }

                if (includeAttribute == null)
                {
                    Log.LogWarning("The reference to project '{0}' doesn't have an 'Include' attribute. MSBuild.Front can't create the symlink without that attribute.", nameNode.Value);
                    continue;
                }

                var csprojFile = includeAttribute.Value;
                if (!File.Exists(Path.Combine(projectDir, csprojFile)))
                {
                    Log.LogWarning(
                        "[WARN] The referenced project '{0}' points to a project file '{1}' which is not found locally. Did you forget to checkout your MSBuild.Front dependancies?",
                        nameNode.Value,
                        includeAttribute.Value);
                    continue;
                }

                yield return Path.Combine(
                    projectDir,
                    Path.GetDirectoryName(csprojFile),
                    "app");
            }
        }

        /// <summary>
        /// Creates a symbolic links at <paramref name="link"/> which points to <paramref name="source"/>
        /// <remarks>
        /// If <paramref name="link"/> points to an existing folder, that folder will be deleted!
        /// </remarks>
        /// </summary>
        /// <param name="link">The full name of the link</param>
        /// <param name="source">The full name of the source</param>
        private static void CreateLink(string link, string source)
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