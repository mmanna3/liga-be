namespace Api._Config;

public class DeepLinksOptions
{
    public string AppleTeamId { get; set; } = "";
    public string IosBundleId { get; set; } = "";
    public string AndroidPackage { get; set; } = "";
    public string[] AndroidSha256Fingerprints { get; set; } = [];
    public string AppStoreUrl { get; set; } = "";
    public string PlayStoreUrl { get; set; } = "";

    public bool EstaConfigurado =>
        !string.IsNullOrWhiteSpace(AppleTeamId)
        && !string.IsNullOrWhiteSpace(IosBundleId)
        && !string.IsNullOrWhiteSpace(AndroidPackage)
        && AndroidSha256Fingerprints.Length > 0
        && !string.IsNullOrWhiteSpace(AppStoreUrl)
        && !string.IsNullOrWhiteSpace(PlayStoreUrl);
}
