import { test, expect, Page } from '@playwright/test';

const TEST_TIMEOUT = 120000; // 2 minutes for long operations

test.describe('Features 1-4: Complete Workflow Tests', () => {
  let page: Page;
  let repositoryId: string;
  const testRepository = {
    name: 'playwright-test-repo',
    url: 'https://github.com/microsoft/TypeScript',
    owner: 'microsoft',
    language: 'TypeScript',
    description: 'TypeScript test repository for E2E testing'
  };

  test.beforeAll(async ({ browser }) => {
    page = await browser.newPage();
    
    // Navigate to application
    await page.goto('/');
    
    // Wait for application to load
    await expect(page.locator('h1')).toBeVisible();
  });

  test.afterAll(async () => {
    // Cleanup: Remove test repository if it exists
    if (repositoryId) {
      try {
        await cleanupTestRepository(page, repositoryId);
      } catch (error) {
        console.log('Cleanup failed:', error);
      }
    }
    await page.close();
  });

  test('F01: Repository Connection - Add Repository Successfully', async () => {
    test.setTimeout(TEST_TIMEOUT);

    // Navigate to Add Repository page
    await page.goto('/repositories/add');
    await expect(page.locator('h1')).toContainText('Add Repository');

    // Fill out repository form
    await page.fill('input[name="name"]', testRepository.name);
    await page.fill('input[name="url"]', testRepository.url);
    await page.fill('input[name="owner"]', testRepository.owner);
    await page.selectOption('select[name="language"]', testRepository.language);
    await page.fill('textarea[name="description"]', testRepository.description);

    // Submit form
    await page.click('button[type="submit"]');

    // Wait for success feedback
    await expect(page.locator('[role="alert"]')).toContainText('Repository added successfully');
    
    // Capture repository ID from response or redirect
    await page.waitForURL(/\/repositories\/.*/, { timeout: 30000 });
    repositoryId = extractRepositoryIdFromUrl(page.url());
    
    expect(repositoryId).toBeTruthy();
    console.log('Repository created with ID:', repositoryId);
  });

  test('F02: Azure AI Search - Repository Indexing Process', async () => {
    test.setTimeout(180000); // 3 minutes for indexing
    
    if (!repositoryId) {
      test.skip('Repository ID not available - previous test failed');
    }

    // Navigate to repository page  
    await page.goto(`/repositories`);
    
    // Find the test repository in the list
    const repositoryCard = page.locator(`[data-testid="repository-card"]`).filter({
      hasText: testRepository.name
    });
    
    await expect(repositoryCard).toBeVisible();
    
    // Click index button
    await repositoryCard.locator('button').filter({ hasText: 'Index' }).click();
    
    // Wait for indexing to start
    await expect(page.locator('[role="alert"]')).toContainText('Indexing started');
    
    // Monitor indexing progress
    let indexingComplete = false;
    let attempts = 0;
    const maxAttempts = 36; // 3 minutes with 5-second intervals
    
    while (!indexingComplete && attempts < maxAttempts) {
      await page.waitForTimeout(5000); // Wait 5 seconds
      await page.reload();
      
      const statusElement = repositoryCard.locator('[data-testid="indexing-status"]');
      const status = await statusElement.textContent();
      
      console.log(`Indexing attempt ${attempts + 1}: Status = ${status}`);
      
      if (status?.includes('Completed') || status?.includes('Indexed')) {
        indexingComplete = true;
      } else if (status?.includes('Failed') || status?.includes('Error')) {
        throw new Error(`Indexing failed: ${status}`);
      }
      
      attempts++;
    }
    
    expect(indexingComplete).toBe(true);
    console.log('Repository indexing completed successfully');
  });

  test('F03: Documentation Generation - Generate and Verify', async () => {
    test.setTimeout(180000); // 3 minutes for doc generation
    
    if (!repositoryId) {
      test.skip('Repository ID not available - previous tests failed');
    }

    // Navigate to documentation page
    await page.goto('/documentation');
    
    // Find repository and click Generate Documentation
    const repositoryItem = page.locator('[data-testid="repository-item"]').filter({
      hasText: testRepository.name
    });
    
    await expect(repositoryItem).toBeVisible();
    await repositoryItem.locator('button').filter({ hasText: 'Generate' }).click();
    
    // Wait for generation to start
    await expect(page.locator('[role="alert"]')).toContainText('Documentation generation started');
    
    // Monitor generation progress
    let generationComplete = false;
    let attempts = 0;
    const maxAttempts = 36; // 3 minutes
    
    while (!generationComplete && attempts < maxAttempts) {
      await page.waitForTimeout(5000);
      await page.reload();
      
      const statusElement = repositoryItem.locator('[data-testid="doc-status"]');
      const status = await statusElement.textContent();
      
      console.log(`Generation attempt ${attempts + 1}: Status = ${status}`);
      
      if (status?.includes('Generated') || status?.includes('Completed')) {
        generationComplete = true;
      } else if (status?.includes('Failed') || status?.includes('Error')) {
        throw new Error(`Documentation generation failed: ${status}`);
      }
      
      attempts++;
    }
    
    expect(generationComplete).toBe(true);
    console.log('Documentation generation completed successfully');
  });

  test('F03: Documentation Viewing - Professional UI Experience', async () => {
    test.setTimeout(60000);
    
    if (!repositoryId) {
      test.skip('Repository ID not available - previous tests failed');
    }

    // Navigate to generated documentation
    await page.goto('/documentation');
    
    const repositoryItem = page.locator('[data-testid="repository-item"]').filter({
      hasText: testRepository.name
    });
    
    // Click View Documentation
    await repositoryItem.locator('button').filter({ hasText: 'View' }).click();
    
    // Verify documentation page loads
    await expect(page.locator('h1')).toContainText(testRepository.name);
    
    // Verify Table of Contents exists
    const toc = page.locator('[data-testid="documentation-toc"]');
    await expect(toc).toBeVisible();
    
    // Verify sections exist
    const sections = toc.locator('button[role="button"]');
    const sectionCount = await sections.count();
    expect(sectionCount).toBeGreaterThan(0);
    
    // Test section navigation
    const firstSection = sections.first();
    await firstSection.click();
    
    // Verify content area updates
    const contentArea = page.locator('[data-testid="documentation-content"]');
    await expect(contentArea).toBeVisible();
    await expect(contentArea.locator('h2')).toBeVisible();
    
    // Test All Sections view
    const allSectionsButton = toc.locator('button').filter({ hasText: 'All Sections' });
    if (await allSectionsButton.isVisible()) {
      await allSectionsButton.click();
      await expect(contentArea).toContainText('Overview');
    }
    
    // Verify frontend UX fields are populated
    const metadata = page.locator('[data-testid="doc-metadata"]');
    await expect(metadata).toContainText(/\d+ sections?/);
    await expect(metadata).toContainText(/\d+ min read/);
    
    console.log('Documentation viewing experience verified');
  });

  test('F04: Conversational Query Interface - Basic Query Flow', async () => {
    test.setTimeout(120000);
    
    if (!repositoryId) {
      test.skip('Repository ID not available - previous tests failed');
    }

    // Navigate to chat interface
    await page.goto('/chat');
    
    // Verify chat interface loads
    await expect(page.locator('h1')).toContainText('Chat');
    
    // Type a test query about the repository
    const chatInput = page.locator('textarea[placeholder*="Ask"]');
    await expect(chatInput).toBeVisible();
    
    const testQuery = `What is the purpose of the ${testRepository.name} repository?`;
    await chatInput.fill(testQuery);
    
    // Send query
    await page.click('button[type="submit"]');
    
    // Wait for response
    const chatMessages = page.locator('[data-testid="chat-message"]');
    await expect(chatMessages).toHaveCount(2, { timeout: 60000 }); // User + AI messages
    
    // Verify AI response contains relevant information
    const aiResponse = chatMessages.last();
    await expect(aiResponse).toContainText(testRepository.language);
    
    console.log('Conversational query interface verified');
  });

  test('Error Handling - Invalid Repository URL', async () => {
    // Test F01 error handling
    await page.goto('/repositories/add');
    
    await page.fill('input[name="name"]', 'invalid-repo');
    await page.fill('input[name="url"]', 'not-a-valid-url');
    await page.fill('input[name="owner"]', 'test');
    
    await page.click('button[type="submit"]');
    
    // Should show validation error
    await expect(page.locator('[role="alert"]')).toContainText('Invalid');
  });

  test('Performance - Page Load Times', async () => {
    const pages = [
      '/',
      '/repositories',
      '/documentation',
      '/chat',
      '/search'
    ];
    
    for (const pagePath of pages) {
      const start = Date.now();
      await page.goto(pagePath);
      await page.waitForLoadState('networkidle');
      const loadTime = Date.now() - start;
      
      console.log(`${pagePath} loaded in ${loadTime}ms`);
      expect(loadTime).toBeLessThan(5000); // 5 second threshold
    }
  });

  test('Accessibility - Basic WCAG Compliance', async () => {
    await page.goto('/');
    
    // Check for proper heading structure
    const h1 = page.locator('h1');
    await expect(h1).toBeVisible();
    
    // Check for proper button labels
    const buttons = page.locator('button');
    for (let i = 0; i < await buttons.count(); i++) {
      const button = buttons.nth(i);
      if (await button.isVisible()) {
        const text = await button.textContent();
        const ariaLabel = await button.getAttribute('aria-label');
        expect(text || ariaLabel).toBeTruthy();
      }
    }
    
    // Check color contrast (basic test)
    await expect(page.locator('body')).toHaveCSS('color', /rgb/);
  });

  test('Mobile Responsiveness - Key Breakpoints', async () => {
    const viewports = [
      { width: 375, height: 667, name: 'iPhone SE' },
      { width: 768, height: 1024, name: 'iPad' },
      { width: 1920, height: 1080, name: 'Desktop' }
    ];
    
    for (const viewport of viewports) {
      await page.setViewportSize(viewport);
      await page.goto('/');
      
      // Verify main navigation is accessible
      const nav = page.locator('nav');
      await expect(nav).toBeVisible();
      
      // Verify content doesn't overflow
      await expect(page.locator('body')).not.toHaveCSS('overflow-x', 'scroll');
      
      console.log(`${viewport.name} (${viewport.width}x${viewport.height}) - responsive check passed`);
    }
  });
});

// Helper functions
function extractRepositoryIdFromUrl(url: string): string {
  const match = url.match(/\/repositories\/([^\/\?]+)/);
  return match ? match[1] : '';
}

async function cleanupTestRepository(page: Page, repoId: string): Promise<void> {
  try {
    // Navigate to repository management
    await page.goto('/repositories');
    
    // Find and delete test repository
    const repositoryCard = page.locator(`[data-repository-id="${repoId}"]`);
    if (await repositoryCard.isVisible()) {
      await repositoryCard.locator('button[aria-label="Delete"]').click();
      await page.click('button[data-testid="confirm-delete"]');
      await expect(page.locator('[role="alert"]')).toContainText('deleted');
    }
  } catch (error) {
    console.log('Cleanup error:', error);
  }
}