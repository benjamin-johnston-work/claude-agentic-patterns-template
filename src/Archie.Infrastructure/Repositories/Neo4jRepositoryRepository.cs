using Archie.Application.Interfaces;
using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;
using Archie.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neo4j.Driver;

namespace Archie.Infrastructure.Repositories;

public class Neo4jRepositoryRepository : IRepositoryRepository, IDisposable
{
    private readonly IDriver _driver;
    private readonly Neo4jOptions _options;
    private readonly ILogger<Neo4jRepositoryRepository> _logger;

    public Neo4jRepositoryRepository(
        IOptions<Neo4jOptions> options,
        ILogger<Neo4jRepositoryRepository> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _driver = GraphDatabase.Driver(_options.Uri, AuthTokens.Basic(_options.Username, _options.Password),
            config => config
                .WithConnectionTimeout(TimeSpan.FromSeconds(_options.ConnectionTimeoutSeconds))
                .WithMaxConnectionPoolSize(_options.MaxConnectionPoolSize));
    }

    public async Task<Repository?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string query = @"
            MATCH (r:Repository {id: $id})
            OPTIONAL MATCH (r)-[:HAS_BRANCH]->(b:Branch)
            OPTIONAL MATCH (b)-[:HAS_COMMIT]->(c:Commit)
            RETURN r, collect(distinct b) as branches, collect(distinct c) as commits";

        await using var session = _driver.AsyncSession(o => o.WithDatabase(_options.Database));
        
        var result = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query, new { id = id.ToString() });
            var records = await cursor.ToListAsync();
            return records.FirstOrDefault();
        });

        return result != null ? MapToRepository(result) : null;
    }

    public async Task<Repository?> GetByUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        const string query = @"
            MATCH (r:Repository {url: $url})
            OPTIONAL MATCH (r)-[:HAS_BRANCH]->(b:Branch)
            OPTIONAL MATCH (b)-[:HAS_COMMIT]->(c:Commit)
            RETURN r, collect(distinct b) as branches, collect(distinct c) as commits";

        await using var session = _driver.AsyncSession(o => o.WithDatabase(_options.Database));
        
        var result = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query, new { url });
            var records = await cursor.ToListAsync();
            return records.FirstOrDefault();
        });

        return result != null ? MapToRepository(result) : null;
    }

    public async Task<IEnumerable<Repository>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string query = @"
            MATCH (r:Repository)
            OPTIONAL MATCH (r)-[:HAS_BRANCH]->(b:Branch)
            OPTIONAL MATCH (b)-[:HAS_COMMIT]->(c:Commit)
            RETURN r, collect(distinct b) as branches, collect(distinct c) as commits
            ORDER BY r.createdAt DESC";

        await using var session = _driver.AsyncSession(o => o.WithDatabase(_options.Database));
        
        var results = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query);
            return await cursor.ToListAsync();
        });

        return results.Select(MapToRepository).ToList();
    }

    public async Task<IEnumerable<Repository>> GetByFilterAsync(RepositoryFilter filter, CancellationToken cancellationToken = default)
    {
        var conditions = new List<string>();
        var parameters = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(filter.Language))
        {
            conditions.Add("r.language = $language");
            parameters["language"] = filter.Language;
        }

        if (!string.IsNullOrEmpty(filter.Status))
        {
            conditions.Add("r.status = $status");
            parameters["status"] = filter.Status;
        }

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            conditions.Add("(r.name CONTAINS $searchTerm OR r.description CONTAINS $searchTerm)");
            parameters["searchTerm"] = filter.SearchTerm;
        }

        var whereClause = conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : "";
        
        var query = $@"
            MATCH (r:Repository)
            {whereClause}
            OPTIONAL MATCH (r)-[:HAS_BRANCH]->(b:Branch)
            OPTIONAL MATCH (b)-[:HAS_COMMIT]->(c:Commit)
            RETURN r, collect(distinct b) as branches, collect(distinct c) as commits
            ORDER BY r.createdAt DESC";

        if (filter.Skip.HasValue)
        {
            query += $" SKIP {filter.Skip.Value}";
        }

        if (filter.Take.HasValue)
        {
            query += $" LIMIT {filter.Take.Value}";
        }

        await using var session = _driver.AsyncSession(o => o.WithDatabase(_options.Database));
        
        var results = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query, parameters);
            return await cursor.ToListAsync();
        });

        return results.Select(MapToRepository).ToList();
    }

    public async Task<Repository> AddAsync(Repository repository, CancellationToken cancellationToken = default)
    {
        const string query = @"
            CREATE (r:Repository {
                id: $id,
                name: $name,
                url: $url,
                language: $language,
                description: $description,
                status: $status,
                createdAt: datetime($createdAt),
                updatedAt: datetime($updatedAt),
                statistics: $statistics
            })
            RETURN r";

        await using var session = _driver.AsyncSession(o => o.WithDatabase(_options.Database));
        
        await session.ExecuteWriteAsync(async tx =>
        {
            var parameters = new
            {
                id = repository.Id.ToString(),
                name = repository.Name,
                url = repository.Url,
                language = repository.Language,
                description = repository.Description,
                status = repository.Status.ToString(),
                createdAt = repository.CreatedAt,
                updatedAt = repository.UpdatedAt,
                statistics = new
                {
                    fileCount = repository.Statistics.FileCount,
                    lineCount = repository.Statistics.LineCount,
                    languageBreakdown = repository.Statistics.LanguageBreakdown.ToDictionary(
                        kvp => kvp.Key,
                        kvp => new
                        {
                            language = kvp.Value.Language,
                            fileCount = kvp.Value.FileCount,
                            lineCount = kvp.Value.LineCount,
                            percentage = kvp.Value.Percentage
                        })
                }
            };

            await tx.RunAsync(query, parameters);
        });

        // Add branches if any
        if (repository.Branches.Any())
        {
            await AddBranchesAsync(repository.Id, repository.Branches);
        }

        _logger.LogInformation("Added repository to Neo4j: {Id}", repository.Id);
        return repository;
    }

    public async Task<Repository> UpdateAsync(Repository repository, CancellationToken cancellationToken = default)
    {
        const string query = @"
            MATCH (r:Repository {id: $id})
            SET r.name = $name,
                r.url = $url,
                r.language = $language,
                r.description = $description,
                r.status = $status,
                r.updatedAt = datetime($updatedAt),
                r.statistics = $statistics
            RETURN r";

        await using var session = _driver.AsyncSession(o => o.WithDatabase(_options.Database));
        
        await session.ExecuteWriteAsync(async tx =>
        {
            var parameters = new
            {
                id = repository.Id.ToString(),
                name = repository.Name,
                url = repository.Url,
                language = repository.Language,
                description = repository.Description,
                status = repository.Status.ToString(),
                updatedAt = repository.UpdatedAt,
                statistics = new
                {
                    fileCount = repository.Statistics.FileCount,
                    lineCount = repository.Statistics.LineCount,
                    languageBreakdown = repository.Statistics.LanguageBreakdown.ToDictionary(
                        kvp => kvp.Key,
                        kvp => new
                        {
                            language = kvp.Value.Language,
                            fileCount = kvp.Value.FileCount,
                            lineCount = kvp.Value.LineCount,
                            percentage = kvp.Value.Percentage
                        })
                }
            };

            await tx.RunAsync(query, parameters);
        });

        _logger.LogInformation("Updated repository in Neo4j: {Id}", repository.Id);
        return repository;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string query = @"
            MATCH (r:Repository {id: $id})
            OPTIONAL MATCH (r)-[:HAS_BRANCH]->(b:Branch)
            OPTIONAL MATCH (b)-[:HAS_COMMIT]->(c:Commit)
            DELETE r, b, c
            RETURN count(r) > 0 as deleted";

        await using var session = _driver.AsyncSession(o => o.WithDatabase(_options.Database));
        
        var result = await session.ExecuteWriteAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query, new { id = id.ToString() });
            var records = await cursor.ToListAsync();
            var record = records.First();
            return record["deleted"].As<bool>();
        });

        if (result)
        {
            _logger.LogInformation("Deleted repository from Neo4j: {Id}", id);
        }

        return result;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string query = "MATCH (r:Repository {id: $id}) RETURN count(r) > 0 as exists";

        await using var session = _driver.AsyncSession(o => o.WithDatabase(_options.Database));
        
        var result = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query, new { id = id.ToString() });
            var records = await cursor.ToListAsync();
            var record = records.First();
            return record["exists"].As<bool>();
        });

        return result;
    }

    public async Task<bool> ExistsByUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        const string query = "MATCH (r:Repository {url: $url}) RETURN count(r) > 0 as exists";

        await using var session = _driver.AsyncSession(o => o.WithDatabase(_options.Database));
        
        var result = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(query, new { url });
            var records = await cursor.ToListAsync();
            var record = records.First();
            return record["exists"].As<bool>();
        });

        return result;
    }

    private async Task AddBranchesAsync(Guid repositoryId, IEnumerable<Branch> branches)
    {
        const string query = @"
            MATCH (r:Repository {id: $repositoryId})
            UNWIND $branches as branch
            CREATE (b:Branch {
                name: branch.name,
                repositoryId: branch.repositoryId,
                isDefault: branch.isDefault,
                createdAt: datetime(branch.createdAt)
            })
            CREATE (r)-[:HAS_BRANCH]->(b)
            WITH b, branch
            WHERE branch.lastCommit IS NOT NULL
            CREATE (c:Commit {
                hash: branch.lastCommit.hash,
                message: branch.lastCommit.message,
                author: branch.lastCommit.author,
                timestamp: datetime(branch.lastCommit.timestamp),
                repositoryId: branch.lastCommit.repositoryId
            })
            CREATE (b)-[:HAS_COMMIT]->(c)";

        await using var session = _driver.AsyncSession(o => o.WithDatabase(_options.Database));
        
        await session.ExecuteWriteAsync(async tx =>
        {
            var branchData = branches.Select(b => new
            {
                name = b.Name,
                repositoryId = b.RepositoryId.ToString(),
                isDefault = b.IsDefault,
                createdAt = b.CreatedAt,
                lastCommit = b.LastCommit != null ? new
                {
                    hash = b.LastCommit.Hash,
                    message = b.LastCommit.Message,
                    author = b.LastCommit.Author,
                    timestamp = b.LastCommit.Timestamp,
                    repositoryId = b.LastCommit.RepositoryId.ToString()
                } : null
            });

            await tx.RunAsync(query, new { repositoryId = repositoryId.ToString(), branches = branchData });
        });
    }

    private static Repository MapToRepository(IRecord record)
    {
        var repoNode = record["r"].As<INode>();
        var branches = record["branches"].As<List<INode>>() ?? new List<INode>();

        var id = Guid.Parse(repoNode["id"].As<string>());
        var name = repoNode["name"].As<string>();
        var url = repoNode["url"].As<string>();
        var language = repoNode["language"].As<string>();
        var description = repoNode["description"].As<string>();
        var status = Enum.Parse<RepositoryStatus>(repoNode["status"].As<string>());
        var createdAt = repoNode["createdAt"].As<ZonedDateTime>().ToDateTimeOffset().DateTime;
        var updatedAt = repoNode["updatedAt"].As<ZonedDateTime>().ToDateTimeOffset().DateTime;

        // Parse statistics
        var statsNode = repoNode["statistics"].As<Dictionary<string, object>>();
        var fileCount = Convert.ToInt32(statsNode["fileCount"]);
        var lineCount = Convert.ToInt32(statsNode["lineCount"]);
        var languageBreakdownDict = statsNode["languageBreakdown"].As<Dictionary<string, object>>();
        
        var languageBreakdown = languageBreakdownDict.ToDictionary(
            kvp => kvp.Key,
            kvp =>
            {
                var langStats = kvp.Value.As<Dictionary<string, object>>();
                return new LanguageStats(
                    langStats["language"].As<string>(),
                    Convert.ToInt32(langStats["fileCount"]),
                    Convert.ToInt32(langStats["lineCount"]),
                    Convert.ToDouble(langStats["percentage"]));
            });

        var statistics = new RepositoryStatistics(fileCount, lineCount, languageBreakdown);

        // Create repository using reflection or constructor that accepts all parameters
        var repository = new Repository(name, url, language, description);
        
        // Set private fields using reflection (temporary solution)
        var idField = typeof(Repository).GetField("<Id>k__BackingField", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        idField?.SetValue(repository, id);

        var createdAtField = typeof(Repository).GetField("<CreatedAt>k__BackingField", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        createdAtField?.SetValue(repository, createdAt);

        repository.UpdateStatus(status);
        repository.UpdateStatistics(statistics);

        // Add branches
        foreach (var branchNode in branches)
        {
            var branchName = branchNode["name"].As<string>();
            var isDefault = branchNode["isDefault"].As<bool>();
            var branchCreatedAt = branchNode["createdAt"].As<ZonedDateTime>().ToDateTimeOffset().DateTime;

            var branch = new Branch(branchName, isDefault, repository.Id);
            
            // Set created at using reflection
            var branchCreatedAtField = typeof(Branch).GetField("<CreatedAt>k__BackingField", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            branchCreatedAtField?.SetValue(branch, branchCreatedAt);

            repository.AddBranch(branch);
        }

        return repository;
    }

    public void Dispose()
    {
        _driver?.Dispose();
    }
}