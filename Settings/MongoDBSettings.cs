namespace CVGeneratorAPI.Settings;

/// <summary>
/// Configuration settings for connecting to MongoDB.
/// This class is bound to the <c>MongoDBSettings</c> section in <c>appsettings.json</c>.
/// Provides database and collection names used by the application.
/// </summary>
public class MongoDBSettings
{
    /// <summary>
    /// The connection string for the MongoDB instance.
    /// Typically includes host, port, and authentication details.
    /// </summary>
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// The name of the MongoDB database used by the application.
    /// </summary>
    public string DatabaseName { get; set; } = null!;

    /// <summary>
    /// The default collection name, if one is needed for general use.
    /// </summary>
    public string CollectionName { get; set; } = null!;

    /// <summary>
    /// The name of the collection storing user documents.
    /// Default value: <c>"Users"</c>.
    /// </summary>
    public string UsersCollectionName { get; set; } = "Users";

    /// <summary>
    /// The name of the collection storing CV documents.
    /// Default value: <c>"CVs"</c>.
    /// </summary>
    public string CVsCollectionName { get; set; } = "CVs";
    /// <summary>
    /// The name of the collection storing CV template documents.
    /// Default value: <c>"cv_templates"</c>.
    /// </summary>
    public string TemplatesCollectionName { get; set; } = "cv_templates";
}