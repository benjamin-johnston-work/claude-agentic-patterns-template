using Archie.Application.DTOs;
using Archie.Application.Interfaces;
using Archie.Application.UseCases;
using Archie.Domain.Entities;
using Archie.Domain.Events;
using Microsoft.Extensions.Logging;
using Moq;

namespace Archie.Application.Tests.UseCases;

[TestFixture]
public class AddRepositoryUseCaseTests
{
    private Mock<IRepositoryRepository> _mockRepository;
    private Mock<IGitRepositoryService> _mockGitService;
    private Mock<IEventPublisher> _mockEventPublisher;
    private Mock<ILogger<AddRepositoryUseCase>> _mockLogger;
    private AddRepositoryUseCase _useCase;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IRepositoryRepository>();
        _mockGitService = new Mock<IGitRepositoryService>();
        _mockEventPublisher = new Mock<IEventPublisher>();
        _mockLogger = new Mock<ILogger<AddRepositoryUseCase>>();
        
        _useCase = new AddRepositoryUseCase(
            _mockRepository.Object,
            _mockGitService.Object,
            _mockEventPublisher.Object,
            _mockLogger.Object);
    }

    [Test]
    public async Task ExecuteAsync_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var input = new AddRepositoryInput { Url = "https://github.com/user/repo.git" };
        var clonedRepository = new Repository("TestRepo", input.Url, "C#", "Test repository");
        
        _mockRepository.Setup(r => r.GetByUrlAsync(input.Url, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Repository?)null);
        
        _mockGitService.Setup(g => g.ValidateRepositoryAccessAsync(input.Url, input.AccessToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        _mockGitService.Setup(g => g.ConnectRepositoryAsync(input.Url, input.AccessToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clonedRepository);
        
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Repository>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Repository r, CancellationToken _) => r);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.Url, Is.EqualTo(input.Url));
        
        _mockEventPublisher.Verify(e => e.PublishAsync(It.IsAny<RepositoryAddedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_RepositoryAlreadyExists_ReturnsFailure()
    {
        // Arrange
        var input = new AddRepositoryInput { Url = "https://github.com/user/repo.git" };
        var existingRepository = new Repository("ExistingRepo", input.Url, "user", "C#");
        
        _mockRepository.Setup(r => r.GetByUrlAsync(input.Url, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRepository);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Repository already exists"));
        
        _mockGitService.Verify(g => g.ValidateRepositoryAccessAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockEventPublisher.Verify(e => e.PublishAsync(It.IsAny<RepositoryAddedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_InvalidRepositoryAccess_ReturnsFailure()
    {
        // Arrange
        var input = new AddRepositoryInput { Url = "https://github.com/user/private-repo.git" };
        
        _mockRepository.Setup(r => r.GetByUrlAsync(input.Url, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Repository?)null);
        
        _mockGitService.Setup(g => g.ValidateRepositoryAccessAsync(input.Url, input.AccessToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Repository is not accessible or does not exist"));
        
        _mockGitService.Verify(g => g.ConnectRepositoryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockEventPublisher.Verify(e => e.PublishAsync(It.IsAny<RepositoryAddedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_GitServiceThrowsException_ReturnsFailure()
    {
        // Arrange
        var input = new AddRepositoryInput { Url = "https://github.com/user/repo.git" };
        
        _mockRepository.Setup(r => r.GetByUrlAsync(input.Url, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Repository?)null);
        
        _mockGitService.Setup(g => g.ValidateRepositoryAccessAsync(input.Url, input.AccessToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        _mockGitService.Setup(g => g.ConnectRepositoryAsync(input.Url, input.AccessToken, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Clone failed"));

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("STEP 2 FAILED - GitHub connection error:"));
        
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Repository>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockEventPublisher.Verify(e => e.PublishAsync(It.IsAny<RepositoryAddedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}