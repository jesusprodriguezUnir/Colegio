import { test, expect } from '@playwright/test';

test.describe('Schedule Management', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/schedules');
  });

  test('should load the schedules page correctly', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Editor de Horarios' })).toBeVisible();
    await expect(page.getByText('Visualiza y ajusta la planificación del centro')).toBeVisible();
  });

  test('should display the timetable grid when a classroom is selected', async ({ page }) => {
    // Assuming there are classrooms seeded
    await page.locator('select').first().selectOption({ index: 0 });
    
    // Check if the grid appears
    await expect(page.locator('table')).toBeVisible();
    await expect(page.getByText('Lunes')).toBeVisible();
    await expect(page.getByText('Viernes')).toBeVisible();
  });

  test('should be able to generate schedules', async ({ page }) => {
    await page.locator('select').first().selectOption({ index: 0 });
    
    const generateButton = page.getByRole('button', { name: /Generar/i });
    if (await generateButton.isVisible()) {
        await generateButton.click();
        // Wait for loading to finish (the RefreshCw icon appears during loading)
        await expect(page.locator('.animate-spin')).not.toBeVisible({ timeout: 20000 });
    }
  });
});
