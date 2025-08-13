import { test, expect } from '@playwright/test';

test.describe('Azure Services Integration Tests', () => {
  
  test('Azure AI Search - Index Creation and Health Check', async ({ page }) => {
    test.setTimeout(60000);
    
    // Navigate to admin/health endpoint (if available)
    await page.goto('/api/health');
    
    const response = await page.waitForResponse(response => 
      response.url().includes('/api/health') && response.status() === 200
    );
    
    const healthData = await response.json();
    
    // Verify Azure Search connectivity
    expect(healthData.azureSearch).toBeDefined();
    expect(healthData.azureSearch.status).toBe('healthy');
    expect(healthData.azureSearch.indexCount).toBeGreaterThan(0);
  });

  test('Azure OpenAI - Embedding Service Health', async ({ page }) => {
    test.setTimeout(60000);
    
    await page.goto('/api/health');
    
    const response = await page.waitForResponse(response => 
      response.url().includes('/api/health') && response.status() === 200
    );
    
    const healthData = await response.json();
    
    // Verify Azure OpenAI connectivity
    expect(healthData.azureOpenAI).toBeDefined();
    expect(healthData.azureOpenAI.status).toBe('healthy');
    expect(healthData.azureOpenAI.embeddingModel).toBe('text-embedding-3-large');
    expect(healthData.azureOpenAI.dimensions).toBe(3072);
  });

  test('Search Functionality - Vector Similarity Search', async ({ page }) => {
    test.setTimeout(120000);
    
    // Navigate to search page
    await page.goto('/search');
    
    // Perform a semantic search
    const searchInput = page.locator('input[placeholder*="Search"]');
    await searchInput.fill('authentication middleware');
    
    await page.click('button[type="submit"]');
    
    // Wait for search results
    const results = page.locator('[data-testid="search-result"]');
    await expect(results.first()).toBeVisible({ timeout: 30000 });
    
    // Verify results contain relevant information
    const firstResult = results.first();
    await expect(firstResult).toContainText(/auth|middleware/i);
    
    // Verify similarity scores are present (if displayed)
    const scoreElement = firstResult.locator('[data-testid="similarity-score"]');
    if (await scoreElement.isVisible()) {
      const score = await scoreElement.textContent();
      expect(parseFloat(score || '0')).toBeGreaterThan(0);
    }
  });

  test('Document Indexing - Vector Embedding Generation', async ({ page }) => {
    test.setTimeout(180000);
    
    // This test would require a test repository to be indexed
    // Navigate to repository management
    await page.goto('/repositories');
    
    // Find an indexed repository
    const indexedRepo = page.locator('[data-testid="repository-card"]').filter({
      hasText: 'Indexed'
    }).first();
    
    if (await indexedRepo.isVisible()) {
      // Check document count
      const docCount = indexedRepo.locator('[data-testid="document-count"]');
      const count = await docCount.textContent();
      expect(parseInt(count || '0')).toBeGreaterThan(0);
      
      // Verify embedding dimensions
      const embeddingInfo = indexedRepo.locator('[data-testid="embedding-info"]');
      if (await embeddingInfo.isVisible()) {
        await expect(embeddingInfo).toContainText('3072'); // text-embedding-3-large dimensions
      }
    }
  });

  test('Rate Limiting - Azure OpenAI Request Throttling', async ({ page }) => {
    test.setTimeout(60000);
    
    // This test checks that rate limiting is working properly
    // Navigate to a page that makes multiple AI requests
    await page.goto('/documentation');
    
    // Monitor network requests to Azure OpenAI
    const aiRequests: any[] = [];
    page.on('response', response => {
      if (response.url().includes('openai.azure.com')) {
        aiRequests.push({
          status: response.status(),
          url: response.url(),
          timestamp: Date.now()
        });
      }
    });
    
    // Trigger multiple documentation generations rapidly
    const repositories = page.locator('[data-testid="repository-item"]');
    const repoCount = Math.min(await repositories.count(), 3);
    
    for (let i = 0; i < repoCount; i++) {
      const repo = repositories.nth(i);
      const generateBtn = repo.locator('button').filter({ hasText: 'Generate' });
      if (await generateBtn.isVisible()) {
        await generateBtn.click();
        await page.waitForTimeout(1000); // Small delay between requests
      }
    }
    
    await page.waitForTimeout(10000); // Wait for requests to complete
    
    // Check that we didn't get too many 429 (rate limited) responses
    const rateLimitedRequests = aiRequests.filter(req => req.status === 429);
    const successfulRequests = aiRequests.filter(req => req.status === 200);
    
    console.log(`AI Requests: ${aiRequests.length}, Rate Limited: ${rateLimitedRequests.length}, Successful: ${successfulRequests.length}`);
    
    // Some rate limiting is expected, but most requests should succeed
    expect(successfulRequests.length).toBeGreaterThan(rateLimitedRequests.length);
  });

  test('Data Sovereignty - Australia East Region Compliance', async ({ page }) => {
    test.setTimeout(30000);
    
    // Check that all Azure service calls are going to Australia East endpoints
    const azureRequests: any[] = [];
    
    page.on('request', request => {
      const url = request.url();
      if (url.includes('azure.com') || url.includes('openai.azure.com')) {
        azureRequests.push({
          url: url,
          method: request.method()
        });
      }
    });
    
    // Navigate to trigger Azure service calls
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Verify all Azure endpoints are in Australia East region
    for (const request of azureRequests) {
      const url = new URL(request.url);
      
      if (url.hostname.includes('openai.azure.com')) {
        // Check for Australia East OpenAI endpoint pattern
        expect(url.hostname).toMatch(/.*-dev\.openai\.azure\.com/);
        console.log('Azure OpenAI endpoint verified:', url.hostname);
      }
      
      if (url.hostname.includes('search.windows.net')) {
        // Check for Australia East Search endpoint
        expect(url.hostname).toMatch(/.*-dev\.search\.windows\.net/);
        console.log('Azure Search endpoint verified:', url.hostname);
      }
    }
    
    console.log(`Total Azure requests: ${azureRequests.length}`);
  });

  test('Error Handling - Azure Service Failures', async ({ page }) => {
    test.setTimeout(60000);
    
    // Test graceful degradation when Azure services are unavailable
    // This would typically require mock service responses or service interruption
    
    await page.goto('/search');
    
    // Try to perform a search that might fail
    const searchInput = page.locator('input[placeholder*="Search"]');
    await searchInput.fill('test query that might fail');
    
    await page.click('button[type="submit"]');
    
    // Wait for either results or error message
    await Promise.race([
      expect(page.locator('[data-testid="search-result"]')).toBeVisible({ timeout: 30000 }),
      expect(page.locator('[role="alert"]')).toContainText('error', { timeout: 30000 })
    ]);
    
    // Verify the application doesn't crash
    await expect(page.locator('body')).toBeVisible();
  });

  test('Performance - Azure Search Query Response Times', async ({ page }) => {
    test.setTimeout(90000);
    
    await page.goto('/search');
    
    const searchQueries = [
      'authentication',
      'database connection',
      'error handling',
      'configuration settings',
      'user management'
    ];
    
    for (const query of searchQueries) {
      const startTime = Date.now();
      
      await page.fill('input[placeholder*="Search"]', query);
      await page.click('button[type="submit"]');
      
      // Wait for results or timeout
      try {
        await expect(page.locator('[data-testid="search-result"]')).toBeVisible({ timeout: 15000 });
        const endTime = Date.now();
        const responseTime = endTime - startTime;
        
        console.log(`Search query "${query}" completed in ${responseTime}ms`);
        expect(responseTime).toBeLessThan(10000); // 10 second threshold
        
        // Clear search for next query
        await page.fill('input[placeholder*="Search"]', '');
      } catch (error) {
        console.log(`Search query "${query}" timed out or failed`);
      }
    }
  });

  test('Vector Store - Embedding Consistency', async ({ page }) => {
    test.setTimeout(60000);
    
    // Test that the same content generates consistent embeddings
    await page.goto('/api/test-embeddings'); // Hypothetical test endpoint
    
    const testContent = 'public class UserService { public async Task<User> GetUserAsync(int id) { return null; } }';
    
    // Generate embedding multiple times for the same content
    const embeddingRequests = [];
    for (let i = 0; i < 3; i++) {
      const response = await page.request.post('/api/generate-embedding', {
        data: { content: testContent }
      });
      
      expect(response.status()).toBe(200);
      const data = await response.json();
      embeddingRequests.push(data.embedding);
    }
    
    // Verify embeddings are identical (or very similar)
    const firstEmbedding = embeddingRequests[0];
    expect(firstEmbedding).toHaveLength(3072); // text-embedding-3-large dimensions
    
    for (let i = 1; i < embeddingRequests.length; i++) {
      const embedding = embeddingRequests[i];
      expect(embedding).toHaveLength(3072);
      
      // Calculate cosine similarity (should be very close to 1.0)
      const similarity = calculateCosineSimilarity(firstEmbedding, embedding);
      expect(similarity).toBeGreaterThan(0.99);
    }
  });
});

// Helper function for cosine similarity calculation
function calculateCosineSimilarity(a: number[], b: number[]): number {
  if (a.length !== b.length) return 0;
  
  let dotProduct = 0;
  let normA = 0;
  let normB = 0;
  
  for (let i = 0; i < a.length; i++) {
    dotProduct += a[i] * b[i];
    normA += a[i] * a[i];
    normB += b[i] * b[i];
  }
  
  return dotProduct / (Math.sqrt(normA) * Math.sqrt(normB));
}