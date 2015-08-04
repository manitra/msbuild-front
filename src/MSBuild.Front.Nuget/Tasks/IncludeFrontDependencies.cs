using System.Linq;

namespace MSBuild.Front.Nuget.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Xml.Linq;

    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    using MSBuild.Front.Nuget.IO;

    /// <summary>
    /// For each dependencies of a given project, this task creates a symlink to its front-end modules folder right inside the target project 'app' folder
    /// </summary>
    public class IncludeFrontDependencies : Task
    {
        private readonly ILinkUtil _linkUtil;
        private readonly IDirectoryUtil _directoryUtil;

        public IncludeFrontDependencies()
            : this(new LinkUtil(), new DirectoryUtil())
        {
        }

        public IncludeFrontDependencies(ILinkUtil linkUtil, IDirectoryUtil directoryUtil)
        {
            _linkUtil = linkUtil;
            _directoryUtil = directoryUtil;
        }

        private enum LinkType
        {
            Copy = 0,
            Symlink = 1
        }

        private class LinkFolder
        {
            public string Folder { get; set; }
            public LinkType Type { get; set; }
        }

        /// <summary>
        /// Should contains the absolute path to the project file (csproj) for which you want to include front end dependencies
        /// </summary>
        [Required]
        public string ProjectPath { get; set; }

        public string Configuration { get; set; }

        private readonly LinkFolder[] LinkedFolders = new LinkFolder[5]
        {
            new LinkFolder() { Folder = "app", Type = LinkType.Symlink },
            new LinkFolder() { Folder = "build", Type = LinkType.Copy },
            new LinkFolder() { Folder = "tests\\unit-tests", Type = LinkType.Symlink },
            new LinkFolder() { Folder = "tests\\e2e-tests", Type = LinkType.Symlink },
            new LinkFolder() { Folder = "tests\\util", Type = LinkType.Symlink }
        };

        private const string ExcludedFolderPattern = ".*node_modules.*";

        public override bool Execute()
        {
            var referencedDirs = GetReferencedDirs(ProjectPath).ToList();

            foreach (var linkFolder in LinkedFolders)
            {
                var destDir = Path.Combine(Path.GetDirectoryName(ProjectPath), linkFolder.Folder);
                var ignoreFile = Path.Combine(destDir, ".gitignore");

                if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
                File.WriteAllText(ignoreFile, ".gitignore" + Environment.NewLine);

                // Note that we don't handle recursion here because
                // the nested dependancies have been built already
                // which means their respective dependancies
                // will be in their own app folder
                foreach (var referencedDir in referencedDirs)
                {
                    var sourceDir = Path.Combine(referencedDir, linkFolder.Folder);
                    // just means that there is no folder in the referenced project
                    // which means it's not a MSBuild.Front-ready project
                    if (!Directory.Exists(sourceDir))
                    {
                        Log.LogMessage(MessageImportance.Low, "Skipping '{0}' because it doesn't exist", sourceDir);
                        continue;
                    }

                    foreach (var item in Directory.GetFileSystemEntries(sourceDir, "*", SearchOption.TopDirectoryOnly))
                    {
                        var shortName = Path.GetFileName(item);
                        var desItem = Path.Combine(destDir, shortName);
                        if (linkFolder.Type == LinkType.Copy || string.Equals(Configuration, "release", StringComparison.OrdinalIgnoreCase))
                        {
                            if (Directory.Exists(item))
                            {
                                _directoryUtil.DirectoryCopy(item, desItem, ExcludedFolderPattern, true);
                            }
                            else
                            {
                                File.Copy(item, desItem, true);
                            }
                        }
                        else if (linkFolder.Type == LinkType.Symlink)
                        {
                            _linkUtil.CreateLink(
                                desItem,
                                item,
                                Directory.Exists(item) ? SymbolicLinkType.Directory : SymbolicLinkType.File
                                );
                        }
                        File.AppendAllText(ignoreFile, shortName + Environment.NewLine);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Get the list of the absolute path of all dependencies of the given csproj file
        /// </summary>
        /// <param name="projectFile">The full path of a csproj file</param>
        /// <returns></returns>
        private IEnumerable<string> GetReferencedDirs(string projectFile)
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

                yield return Path.GetFullPath(
                    Path.Combine(
                    projectDir,
                    Path.GetDirectoryName(csprojFile)));
            }
        }
    }
}