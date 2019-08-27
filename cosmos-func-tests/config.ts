import { browser, Config } from 'protractor';

const allure = require('jasmine-allure-reporter');

class ScreenshotMaker implements jasmine.CustomReporter {
    public specDone(result: jasmine.CustomReporterResult) {
        if (result.status === 'failed') {
            browser
                .takeScreenshot()
                .then((png) => {
                    allure.createAttachment(
                        'Screenshot',
                        () => {
                            return new Buffer(png, 'base64');
                        },
                        'image/png'
                    )();
                })
                .catch((error: any) => console.log(error));
        }
    }
}

export let config: Config = {
    // to auto start Selenium server every time before test through config, we can use the below command instead of the above one
    directConnect: true,
    SELENIUM_PROMISE_MANAGER: false,
    // seleniumAddress: 'http://localhost:4444/wd/hub/',
    framework: 'jasmine2',
    capabilities: {
        maxInstances: 1,
        browserName: 'chrome'
        // for running in headless mode
        // chromeOptions: {
        //   args: ["--headless", "--disable-gpu", "--window-size=800,600"]
        // }
    },
    // Delaying for 3 sec before interacting with element and hightlighting it
    // highlightDelay: 3000,
    // Log file location
    // webDriverLogDir: 'logs',


    // options for Jasmine
    jasmineNodeOpts: {
        showColors: true,
        // Jasmine assertions timeout
        defaultTimeoutInterval: 150000
    },

    specs: ['../JSFiles/specs/commentary/*.spec.js'],

    onPrepare: () => {
        browser.manage().timeouts().implicitlyWait(5000);
        const AllureReporter = require('jasmine-allure-reporter');
        jasmine.getEnv().addReporter(
            new AllureReporter({
                allureReport: {
                    resultsDir: 'allure-results'
                }
            })
        );

        // generate a screen shot after each failed test
        jasmine.getEnv().addReporter(new ScreenshotMaker());

        browser.driver
            .manage()
            .window()
            .maximize();
    },
    params: {
        baseUrl: 'https://localhost:5000',
        expectedUrlAfterNavigation: 'https://localhost:5000/app'
    },
    // protractor timeouts
    getPageTimeout: 50000,
    allScriptsTimeout: 50000,
    plugins: [],

    onComplete: () => {
        browser.close();
    }
};
