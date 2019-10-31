
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
        let moment = require('moment');
        const HtmlReporter = require('protractor-beautiful-reporter');
        let reporter = new HtmlReporter({
            baseDirectory: '_results-reports'
            , screenshotsSubfolder: '_images'
            , jsonsSubfolder: 'jsons'
            , takeScreenShotsOnlyForFailedSpecs: true
            , docTitle: 'Cosmos - Test Execution Report' + ' - ' + moment().format('MMMM Do YYYY, hh:mm:ss z')
            , docName: 'cosmos_test_report.html'
            , gatherBrowserLogs: false
            , preserveDirectory: false
            , clientDefaults: {
                showTotalDurationIn: 'header',
                totalDurationFormat: 'h:m:s',
                columnSettings: {
                    inlineScreenshots: true,
                    warningTime: 100000,
                    dangerTime: 250000
                }
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
            browserName: 'chrome',
            chromeOptions: {
                'args': ['disable-extensions', 'start-maximized']
            }
        },

        // options for Jasmine
        jasmineNodeOpts: {
            showColors: true,
            // Jasmine assertions timeout
            defaultTimeoutInterval: 150000
        },

        specs: ['./../_out/specs/**/*.spec.js'],

        plugins: [
            {
                // The module name
                package: 'protractor-image-comparison',
                // Some options, see the docs for more
                options: {
                    baselineFolder: '../_baseline',
                    screenshotPath: '../_images',
                    savePerInstance: true,
                    autoSaveBaseline: true,
                    debug: true
                }
            }
        ],

        onPrepare: async () => {
            console.log('Preparing');
            addHtmlReporter();
            try {

                if (options.setup) {
                    startMongoDb();
                    startSquidex();

                    await runDeployment(options.url);
                }

                browser.manage().timeouts().implicitlyWait(5000);
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