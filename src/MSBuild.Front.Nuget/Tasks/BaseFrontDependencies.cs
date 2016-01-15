using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using MSBuild.Front.Nuget.IO;

namespace MSBuild.Front.Nuget.Tasks
{
    public abstract class BaseFrontDependencies : Task
    {
        protected readonly ILinkUtil _linkUtil;
        protected readonly IDirectoryUtil _directoryUtil;

        /// <summary>
        /// Should contains the absolute path to the project file (csproj) for which you want to include front end dependencies
        /// </summary>
        [Required]
        public string ProjectPath { get; set; }

        protected readonly LinkFolder[] LinkedFolders = new LinkFolder[5]
        {
            new LinkFolder() { Folder = "app", Type = LinkType.Symlink },
            new LinkFolder() { Folder = "build", Type = LinkType.Copy },
            new LinkFolder() { Folder = "tests\\unit-tests", Type = LinkType.Symlink },
            new LinkFolder() { Folder = "tests\\e2e-tests", Type = LinkType.Symlink },
            new LinkFolder() { Folder = "tests\\util", Type = LinkType.Symlink }
        };

        protected const string ExcludedFolderPattern = ".*node_modules.*";
        protected const string ExcludedFilePattern = ".*\\.gitignore.*";

        protected BaseFrontDependencies()
            : this(new LinkUtil(), new DirectoryUtil())
        {
        }

        protected BaseFrontDependencies(ILinkUtil linkUtil, IDirectoryUtil directoryUtil)
        {
            _linkUtil = linkUtil;
            _directoryUtil = directoryUtil;
        }

        protected enum LinkType
        {
            Copy = 0,
            Symlink = 1
        }

        protected class LinkFolder
        {
            public string Folder { get; set; }
            public LinkType Type { get; set; }
        }
    }
}
