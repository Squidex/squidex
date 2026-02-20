import { getTestBed, TestBed } from "@angular/core/testing";
import { BrowserTestingModule, platformBrowserTesting } from "@angular/platform-browser/testing";
import { afterEach } from "vitest";

afterEach(() => {
    TestBed.resetTestingModule();
});

// Only initialize test environment if it hasn't been initialized yet
// This prevents errors when vitest runs tests in parallel (e.g., in CI)
try {
    getTestBed().initTestEnvironment(
        BrowserTestingModule,
        platformBrowserTesting(),
    );
} catch {
    // Test environment already initialized, ignore the error
}
