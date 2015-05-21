namespace MSBuild.Front.Nuget.IO
{
    public interface ILinkUtil
    {
        /// <summary>
        /// Creates a symbolic links at <paramref name="link"/> which points to <paramref name="source"/>
        /// <remarks>
        /// If <paramref name="link"/> points to an existing folder, that folder will be deleted!
        /// </remarks>
        /// </summary>
        /// <param name="link">The full name of the link</param>
        /// <param name="source">The full name of the source</param>
        void CreateLink(string link, string source);
    }
}