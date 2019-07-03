import { Config, browser } from "protractor";
import { allure } from "jasmine-allure-reporter";

declare const allure: any;
export let config: Config = {
  //to auto start Selenium server every time before test through config, we can use the below command instead of the above one
  //directConnect: true,
  seleniumAddress: "http://localhost:4444/wd/hub/",
  framework: "jasmine2",
  capabilities: {
    maxInstances: 1,
    browserName: "chrome",
    //for running in headless mode
    chromeOptions: {
      args: ["--headless", "--disable-gpu", "--window-size=800,600"]
    }
  },

  //options for Jasmine
  jasmineNodeOpts: {
    showColors: true,
    //Jasmine assertions timeout
    defaultTimeoutInterval: 150000
  },

  specs: ["../JSFiles/specs/login/*.spec.js"],

  onPrepare: () => {
    const AllureReporter = require("jasmine-allure-reporter");
    jasmine.getEnv().addReporter(
      new AllureReporter({
        allureReport: {
          resultsDir: "allure-results"
        }
      })
    );
    const addScreenShots = new (function() {
      this.specDone = function(result) {
        if (result.status === "failed") {
          browser
            .takeScreenshot()
            .then(function(png) {
              allure.createAttachment(
                "Screenshot",
                function() {
                  return new Buffer(png, "base64");
                },
                "image/png"
              )();
            })
            .catch((error: any) => console.log(error));
        }
      };
    })();
    //generate a screen shot after each failed test
    jasmine.getEnv().addReporter(addScreenShots);

    browser.driver
      .manage()
      .window()
      .maximize();
  },
  params: {
    baseUrl: "https://localhost:5000",
    expectedUrlAfterNavigation: "https://localhost:5000/app"
  },
  //protractor timeouts
  getPageTimeout: 50000,
  allScriptsTimeout: 50000,
  plugins: [],

  onComplete: () => {
    browser.close();
  }
};
