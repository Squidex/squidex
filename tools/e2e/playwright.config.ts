import path from 'path';
import { defineConfig, devices } from '@playwright/test';

export const TEMPORARY_PATH = path.join(__dirname, 'playwright/.temp');

export const STORAGE_STATE = path.join(__dirname, 'playwright/.auth/user.json');

/**
 * See https://playwright.dev/docs/test-configuration.
 */
export default defineConfig({
    testDir: './tests',
    /* Run tests in files in parallel */
    fullyParallel: true,
    /* Fail the build on CI if you accidentally left test.only in the source code. */
    forbidOnly: !!process.env.CI,
    /* Use a dedicated folder for snapshots. */
    snapshotDir: './snapshots',
    /* Retry on CI only */
    retries: process.env.CI ? 0 : 0,
    /* Opt out of parallel tests on CI. */
    workers: process.env.CI ? 1 : undefined,
    /* Reporter to use. See https://playwright.dev/docs/test-reporters */
    reporter: 'html',
    /* Shared settings for all the projects below. See https://playwright.dev/docs/api/class-testoptions. */
    use: {
        /* Base URL to use in actions like `await page.goto('/')`. */
        baseURL: process.env.BASE__URL || 'https://localhost:5001',

        /* Collect trace when retrying the failed test. See https://playwright.dev/docs/trace-viewer */
        trace: 'on-first-retry',

        /* Keep the test report small. See https://playwright.dev/docs/api/class-testoptions#test-options-screenshot */
        screenshot: 'only-on-failure',
    },

    /* Configure projects for major browsers */
    projects: [
        {
            name: 'login',
            testMatch: 'tests/given-login/_setup.ts',
        },

        {
            name: 'app',
            testMatch: 'tests/given-app/_setup.ts',
            dependencies: ['login'],
            use: {
                storageState: STORAGE_STATE,
            },
        },

        {
            name: 'schema',
            testMatch: 'tests/given-schema/_setup.ts',
            dependencies: ['app'],
            use: {
                storageState: STORAGE_STATE,
            },
        },

        {
            name: 'given login',
            testMatch: 'tests/given-login/*.spec.ts',
            dependencies: ['login'],
            use: {
                ...devices['Desktop Chrome'],
                storageState: STORAGE_STATE,
            },
        },

        {
            name: 'given app',
            testMatch: 'tests/given-app/*.spec.ts',
            dependencies: ['app'],
            use: {
                ...devices['Desktop Chrome'],
                storageState: STORAGE_STATE,
            },
        },

        {
            name: 'given schema',
            testMatch: 'tests/given-schema/*.spec.ts',
            dependencies: ['schema'],
            use: {
                ...devices['Desktop Chrome'],
                storageState: STORAGE_STATE,
            },
        },

        {
            name: 'logged out',
            testMatch: 'tests/*.spec.ts',
            use: {
                ...devices['Desktop Chrome'],
            },
        },
    ],
});
