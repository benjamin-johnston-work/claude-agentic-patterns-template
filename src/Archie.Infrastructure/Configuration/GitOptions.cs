namespace Archie.Infrastructure.Configuration;

public class GitOptions
{
    public const string SectionName = "Git";

    public string TempDirectory { get; set; } = Path.GetTempPath();
    public int CloneTimeoutMinutes { get; set; } = 10;
    public int MaxConcurrentClones { get; set; } = 5;
    public bool CleanupAfterAnalysis { get; set; } = true;
}

public class Neo4jOptions
{
    public const string SectionName = "Neo4j";

    public string Uri { get; set; } = "bolt://localhost:7687";
    public string Username { get; set; } = "neo4j";
    public string Password { get; set; } = "password";
    public string Database { get; set; } = "neo4j";
    public int ConnectionTimeoutSeconds { get; set; } = 30;
    public int MaxConnectionPoolSize { get; set; } = 100;
}