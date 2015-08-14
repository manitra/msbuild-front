using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using MSBuild.Front.Nuget.IO;

namespace MSBuild.Front.Nuget.Tasks
{
    public class CleanFrontDependencies : BaseFrontDependencies
    {
        public CleanFrontDependencies()
            : base(new LinkUtil(), new DirectoryUtil())
        {
        }

        public override bool Execute()
        {
            var allProjectAssets = GetProjectAssets(ProjectPath).ToList();
            var projectDir = Path.GetDirectoryName(ProjectPath);

            foreach (var linkFolder in LinkedFolders)
            {
                var destDir = Path.Combine(projectDir, linkFolder.Folder);

                //get all child entries
                foreach (var item in Directory.GetFileSystemEntries(destDir, "*", SearchOption.TopDirectoryOnly))
                {
                    if (!allProjectAssets.Any(asset => asset.Contains(item)))
                    {
                        if (Directory.Exists(item))
                        {
                            Directory.Delete(item, System.IO.File.GetAttributes(item).HasFlag(FileAttributes.ReparsePoint) == false);
                        }
                        if (File.Exists(item))
                            File.Delete(item);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Get the list of the absolute path of all assets of a project
        /// </summary>
        /// <param name="projectFile">The full path of a csproj file</param>
        /// <returns></returns>
        private IEnumerable<string> GetProjectAssets(string projectFile)
        {
            var projectContent = XDocument.Load(projectFile);
            var projectDir = Path.GetDirectoryName(projectFile);

            foreach (var asset in projectContent.Descendants()
                                                .Where(x => x.Name.LocalName == "Folder"
                                                        || x.Name.LocalName == "Content"
                                                        || x.Name.LocalName == "None"))
            {
                var includeAttribute = asset.Attribute("Include");

                if (includeAttribute == null)
                {
                    Log.LogWarning("The asset doesn't have an 'Include' attribute. MSBuild.Front will ignore this asset");
                    continue;
                }

                var assetPath = Path.Combine(projectDir, includeAttribute.Value);
                if (!File.Exists(assetPath) && !Directory.Exists(assetPath))
                {
                    Log.LogWarning(
                        "[WARN] The asset '{0}' is not found locally.",
                        includeAttribute.Value);
                    continue;
                }

                yield return Path.GetFullPath(assetPath);
            }
        }
    }
}
