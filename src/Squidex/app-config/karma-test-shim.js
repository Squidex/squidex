Error.stackTraceLimit = Infinity;

require('core-js/proposals/reflect-metadata');

require('zone.js/dist/zone');
require('zone.js/dist/long-stack-trace-zone');
require('zone.js/dist/proxy');
require('zone.js/dist/sync-test');
require('zone.js/dist/jasmine-patch');
require('zone.js/dist/async-test');
require('zone.js/dist/fake-async-test');

var testing = require('@angular/core/testing');
var browser = require('@angular/platform-browser-dynamic/testing');

testing.TestBed.initTestEnvironment(
    browser.BrowserDynamicTestingModule,
    browser.platformBrowserDynamicTesting()
);

var testContext = require.context('../app', true, /\.spec\.ts/);

/**
 * Get all the files, for each file, call the context function
 * that will require the file and load it up here. Context will
 * loop and require those spec files here.
 */
function requireAll(requireContext) {
    return requireContext.keys().map(requireContext);
}

/**
 * Requires and returns all modules that match.
 */
var modules = requireAll(testContext);