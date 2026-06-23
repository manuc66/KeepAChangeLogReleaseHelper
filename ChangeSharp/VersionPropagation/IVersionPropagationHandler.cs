namespace ChangeSharp.VersionPropagation;

public interface IVersionPropagationHandler
{
    bool CanHandle(VersionTargetConfig target);
    void UpdateVersion(string basePath, VersionTargetConfig target, string nextVersion);
}
