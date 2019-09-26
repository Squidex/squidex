import { browser, Config } from 'protractor';

import {
    runDeployment,
    startMongoDb,
    startSquidex,
    stopMongoDB,
    stopSquidex
} from './setup';

export function buildConfig(options: { url: string, setup: boolean }): Config {

    function addHtmlReporter() {
        const HtmlReporter = require('protractor-beautiful-reporter');
        let reporter = new HtmlReporter({
            baseDirectory: '_results-reports'
            , screenshotsSubfolder: '_images'
            , jsonsSubfolder: 'jsons'
            , excludeSkippedSpecs: true
            , takeScreenShotsOnlyForFailedSpecs: true
            , docTitle: 'Cosmos Test Suite Last Run Report'
            , docName: 'lastrunresults.html'
            , gatherBrowserLogs: false
            , preserveDirectory: false
            , clientDefaults: {
                showTotalDurationIn: 'header',
                totalDurationFormat: 'hms'
            }
         });
         jasmine.getEnv().addReporter(reporter.getJasmine2Reporter());
    }

    let isCleanup = false;

    function cleanup() {
        if (isCleanup) {
            return;
        }

        console.log('Cleaning');

        browser.close();

        if (options.setup) {
            stopSquidex();
            stopMongoDB();
        }

        console.log('Cleaned');
    }

    process.on('exit', () => {
        cleanup();
    });

    return {
        // to auto start Selenium server every time before test through config, we can use the below command instead of the above one
        directConnect: true,
        // https://www.protractortest.org/#/async-await
        SELENIUM_PROMISE_MANAGER: false,
        // seleniumAddress: 'http://localhost:4444/wd/hub/',
        framework: 'jasmine2',

        capabilities: {
            maxInstances: 1,
            browserName: 'chrome'
        },

        // options for Jasmine
        jasmineNodeOpts: {
            showColors: true,
            // Jasmine assertions timeout
            defaultTimeoutInterval: 150000
        },

        specs: ['./../_out/specs/**/*.spec.js'],

        onPrepare: async () => {
            console.log('Preparing');
            const protractorImageComparison = require('protractor-image-comparison');
            browser. protractorImageComparison = new protractorImageComparison(
                {
                    autoSaveBaseline: true,
                    baselineFolder: '../baseline/',
                    screenshotPath: '../images',
                    debug: true,
                    disableCSSAnimation: true,
                    nativeWebScreenshot: true
                }
            );
            addHtmlReporter();
            try {

                if (options.setup) {
                    startMongoDb();
                    startSquidex();

                    await runDeployment(options.url);
                }

                browser.manage().timeouts().implicitlyWait(5000);
                browser.driver
                    .manage()
                    .window()
                    .maximize();
            } catch (ex) {
                browser.close();

                if (options.setup) {
                    stopSquidex();
                    stopMongoDB();
                }
                cleanup();

                throw ex;
            }

            console.log('Prepared');
        },
        params: {
            baseUrl: options.url
        },

        // When navigating to a new page using browser.get, Protractor waits for the page to be loaded and the new URL to appear before continuing.
        getPageTimeout: 50000,

        // Before performing any action, Protractor waits until there are no pending asynchronous tasks in your Angular application.
        allScriptsTimeout: 50000,

        onCleanup: () => {
            browser.close();

            if (options.setup) {
                stopSquidex();
                stopMongoDB();
            }
            cleanup();
        }
    };
}