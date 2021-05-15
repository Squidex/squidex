Error.stackTraceLimit = Infinity;

require('core-js/proposals/reflect-metadata');

require('zone.js/dist/zone');
require('zone.js/dist/zone-testing');

const testing = require('@angular/core/testing');
const browser = require('@angular/platform-browser-dynamic/testing');

testing.getTestBed().initTestEnvironment(
    browser.BrowserDynamicTestingModule,
    browser.platformBrowserDynamicTesting(),
);

// Then we find all the tests.
const context = require.context('./../app', true, /\.spec\.ts$/);
// And load the modules.
context.keys().map(context);
