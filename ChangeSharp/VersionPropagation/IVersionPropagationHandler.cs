namespace ChangeSharp.VersionPropagation;

public interface IVersionPropagationHandler
{
    bool CanHandle(VersionTargetConfig target);
    /// <summary>
    /// Returns null on success, or a warning message if the target was skipped.
    /// </summary>
    string? UpdateVersion(string basePath, VersionTargetConfig target, string nextVersion);
}
