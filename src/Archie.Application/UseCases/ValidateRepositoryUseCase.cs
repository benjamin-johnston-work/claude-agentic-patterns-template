using Archie.Application.Common;
using Archie.Application.DTOs;
using Archie.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Archie.Application.UseCases;

public class ValidateRepositoryUseCase
{
    private readonly IGitRepositoryService _gitRepositoryService;
    private readonly ILogger<ValidateRepositoryUseCase> _logger;

    public ValidateRepositoryUseCase(
        IGitRepositoryService gitRepositoryService,
        ILogger<ValidateRepositoryUseCase> logger)
    {
        _gitRepositoryService = gitRepositoryService ?? throw new ArgumentNullException(nameof(gitRepositoryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ValidateRepositoryResult>> ExecuteAsync(
        ValidateRepositoryInput input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating repository: {Url}", input.Url);

            // First validate URL format and basic accessibility
            var isValid = await _gitRepositoryService.ValidateRepositoryAccessAsync(
                input.Url, 
                input.AccessToken, 
                cancellationToken);

            if (!isValid)
            {
                return Result<ValidateRepositoryResult>.Failure("Repository is not accessible or does not exist");
            }

            // Get detailed repository information
            var repository = await _gitRepositoryService.ConnectRepositoryAsync(
                input.Url, 
                input.AccessToken, 
                cancellationToken);

            // Get available branches
            var branches = await _gitRepositoryService.GetBranchesAsync(
                input.Url, 
                input.AccessToken, 
                cancellationToken);

            var result = new ValidateRepositoryResult
            {
                IsValid = true,
                Repository = new RepositoryInfo
                {
                    Name = repository.Name,
                    Description = repository.Description,
                    Language = repository.Language,
                    IsPrivate = false, // This would need to be determined from GitHub API response
                    Branches = branches.Select(b => b.Name).ToArray()
                }
            };

            _logger.LogInformation("Repository validation successful: {Name}", result.Repository.Name);
            return Result<ValidateRepositoryResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Repository validation failed: {Url}", input.Url);
            return Result<ValidateRepositoryResult>.Failure(ex.Message);
        }
    }
}