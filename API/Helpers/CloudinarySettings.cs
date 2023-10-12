namespace API.Helpers;

public class CloudinarySettings
{
    // Property name should exactly match the key names in the appsettings.json file.
    // Once done, add the Cloudinary settings in the ApplicationServiceExtensions.cs file.
    public string CloudName { get; set; }
    public string ApiKey { get; set; }
    public string ApiSecret { get; set; }
}
