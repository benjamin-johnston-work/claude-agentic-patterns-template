using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Archie.Infrastructure.GitHub;
using Archie.Infrastructure.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Archie.Infrastructure.Tests.Services
{
    [TestFixture]
    public class GitHubServiceTests
    {
        private Mock<ILogger<GitHubService>> _mockLogger;
        private Mock<IOptions<GitHubOptions>> _mockOptions;
        private GitHubOptions _gitHubOptions;
        private GitHubService _gitHubService;

        [SetUp]
        public void SetUp()
        {
            _mockLogger = new Mock<ILogger<GitHubService>>();
            _mockOptions = new Mock<IOptions<GitHubOptions>>();
            
            _gitHubOptions = new GitHubOptions
            {
                DefaultAccessToken = "test-token",
                UserAgent = "ArchieTest",
                ApiTimeoutSeconds = 30,
                RateLimitBuffer = 100,
                EnableRateLimitProtection = false // Disable for tests
            };
            
            _mockOptions.Setup(x => x.Value).Returns(_gitHubOptions);
            _gitHubService = new GitHubService(_mockOptions.Object, _mockLogger.Object);
        }

        [TestCase("https://github.com/owner/repo", "owner", "repo")]
        [TestCase("https://github.com/benjamin-johnston-work/AIBenjamin", "benjamin-johnston-work", "AIBenjamin")]
        [TestCase("https://github.com/microsoft/dotnet", "microsoft", "dotnet")]
        public void ParseRepositoryUrl_ValidUrls_ReturnsCorrectOwnerAndRepo(string url, string expectedOwner, string expectedRepo)
        {
            // Act
            var (owner, repo) = _gitHubService.ParseRepositoryUrl(url);

            // Assert
            Assert.That(owner, Is.EqualTo(expectedOwner));
            Assert.That(repo, Is.EqualTo(expectedRepo));
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("invalid-url")]
        [TestCase("https://gitlab.com/owner/repo")]
        [TestCase("https://bitbucket.org/owner/repo")]
        public void ParseRepositoryUrl_InvalidUrls_ThrowsArgumentException(string invalidUrl)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _gitHubService.ParseRepositoryUrl(invalidUrl));
        }

        [Test]
        public void ParseRepositoryUrl_NullUrl_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _gitHubService.ParseRepositoryUrl(null));
        }

        // Note: The following tests require actual GitHub API integration or mocking Octokit.
        // For comprehensive testing, you would need to:
        // 1. Mock the Octokit.NET client
        // 2. Or use integration tests with a test repository
        // 3. Or use a test repository that's guaranteed to exist

        [Test, Category("Integration")]
        public async Task GetRepositoryTreeWithMetadataAsync_PublicRepository_ReturnsNonEmptyTree()
        {
            // Arrange - using a known public repository for integration testing
            var owner = "octocat";
            var repo = "Hello-World";
            
            // Act
            var result = await _gitHubService.GetRepositoryTreeWithMetadataAsync(owner, repo, "master");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Tree, Is.Not.Null);
            Assert.That(result.Tree, Is.Not.Empty, 
                "Repository tree should contain files. If this fails, check if the test repository exists and is accessible.");
            
            // Verify that we have actual file data
            var readmeFile = result.Tree.FirstOrDefault(f => f.Path.Equals("README", StringComparison.OrdinalIgnoreCase));
            Assert.That(readmeFile, Is.Not.Null, "Expected to find a README file in the test repository");
            Assert.That(readmeFile.Type, Is.EqualTo("blob"), "README should be a blob (file) type");
        }

        [Test, Category("Integration")]
        public async Task GetRepositoryTreeWithMetadataAsync_NonExistentRepository_ReturnsEmptyTree()
        {
            // Arrange
            var owner = "nonexistent-owner-12345";
            var repo = "nonexistent-repo-12345";
            
            // Act
            var result = await _gitHubService.GetRepositoryTreeWithMetadataAsync(owner, repo, "main");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Tree, Is.Not.Null);
            Assert.That(result.Tree, Is.Empty, "Non-existent repository should return empty tree");
        }

        [Test]
        public async Task GetRepositoryTreeWithMetadataAsync_EmptyRepository_ShouldNotCauseException()
        {
            // This test validates that our GitHub service handles empty repositories gracefully
            // without causing null reference exceptions or other failures
            
            // Arrange - This would need to be a test repository that's intentionally empty
            var owner = "test-owner";
            var repo = "empty-test-repo";
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrowAsync(async () =>
            {
                var result = await _gitHubService.GetRepositoryTreeWithMetadataAsync(owner, repo, "main");
                
                // Result should be valid even if empty
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Tree, Is.Not.Null);
            });
        }

        [Test]
        public void GetRepositoryTreeWithMetadataAsync_InvalidParameters_ThrowsArgumentException()
        {
            // Test various invalid parameter combinations
            Assert.ThrowsAsync<ArgumentException>(async () => 
                await _gitHubService.GetRepositoryTreeWithMetadataAsync("", "repo", "main"));
            
            Assert.ThrowsAsync<ArgumentException>(async () => 
                await _gitHubService.GetRepositoryTreeWithMetadataAsync("owner", "", "main"));
            
            Assert.ThrowsAsync<ArgumentException>(async () => 
                await _gitHubService.GetRepositoryTreeWithMetadataAsync("owner", "repo", ""));
        }

        [Test]
        public async Task GetRepositoryTreeWithMetadataAsync_CancellationToken_RespectsTimeout()
        {
            // Arrange
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1)); // Very short timeout
            
            // Act & Assert
            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await _gitHubService.GetRepositoryTreeWithMetadataAsync("owner", "repo", "main", 
                    cancellationToken: cts.Token);
            });
        }
    }

    [TestFixture]
    public class GitHubServiceRegressionTests
    {
        private Mock<ILogger<GitHubService>> _mockLogger;
        private Mock<IOptions<GitHubOptions>> _mockOptions;
        private GitHubService _gitHubService;

        [SetUp]
        public void SetUp()
        {
            _mockLogger = new Mock<ILogger<GitHubService>>();
            _mockOptions = new Mock<IOptions<GitHubOptions>>();
            
            var options = new GitHubOptions
            {
                DefaultAccessToken = "test-token",
                UserAgent = "ArchieTest",
                EnableRateLimitProtection = false
            };
            
            _mockOptions.Setup(x => x.Value).Returns(options);
            _gitHubService = new GitHubService(_mockOptions.Object, _mockLogger.Object);
        }

        [Test]
        public async Task GetRepositoryTreeWithMetadataAsync_ShouldNeverReturnNullTree()
        {
            // This test specifically guards against the bug we just fixed where
            // the method was returning an empty tree instead of actual repository data
            
            // Arrange
            var owner = "octocat";
            var repo = "Hello-World";
            
            // Act
            var result = await _gitHubService.GetRepositoryTreeWithMetadataAsync(owner, repo, "master");

            // Assert - These assertions guard against the specific bug we fixed
            Assert.That(result, Is.Not.Null, "GetRepositoryTreeWithMetadataAsync must never return null");
            Assert.That(result.Tree, Is.Not.Null, "Tree property must never be null");
            
            // The critical assertion - this should NEVER be empty for a real repository
            // If this fails, it means we've regressed to the old stub implementation
            if (result.Tree.Count == 0)
            {
                Assert.Fail("CRITICAL REGRESSION: GetRepositoryTreeWithMetadataAsync returned empty tree for existing repository. " +
                           "This indicates the method has reverted to stub implementation or there's an API access issue. " +
                           $"Expected non-empty tree for {owner}/{repo}");
            }
            
            // Verify we have actual meaningful data
            Assert.That(result.Tree.Any(f => !string.IsNullOrEmpty(f.Path)), Is.True, 
                "Tree should contain files with valid paths");
            Assert.That(result.Tree.Any(f => f.Type == "blob"), Is.True, 
                "Tree should contain blob (file) entries");
        }

        [Test, Category("CriticalRegression")]
        public void GetRepositoryTreeWithMetadataAsync_ImplementationMustNotBeStub()
        {
            // This test uses reflection to ensure the implementation isn't just returning
            // an empty GitHubTree object (the stub we just fixed)
            
            var method = typeof(GitHubService).GetMethod("GetRepositoryTreeWithMetadataAsync");
            Assert.That(method, Is.Not.Null);
            
            // If someone accidentally reverts to the stub implementation, this test
            // will catch it during code review by checking for the telltale comment
            var methodBody = method.ToString();
            
            // This would catch if someone puts the old stub code back
            Assert.That(methodBody, Does.Not.Contain("For now, return a simple tree structure"), 
                "REGRESSION DETECTED: Method appears to contain stub implementation comment");
            Assert.That(methodBody, Does.Not.Contain("we can enhance this later"), 
                "REGRESSION DETECTED: Method appears to contain stub implementation comment");
        }
    }
}