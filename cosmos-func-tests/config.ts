import { Config, browser } from "protractor";
import { allure } from "jasmine-allure-reporter";

declare const allure: any;
export let config: Config = {
  //to auto start Selenium server every time before test through config, we can use the below command instead of the above one
  directConnect: true,
  framework: "jasmine2", // set to "jasmine" for using Jasmine framework
  capabilities: {
    maxInstances: 1,
    browserName: "chrome"
    //for running in headless mode
    // chromeOptions: {
    //   args: ["--headless", "--disable-gpu", "--window-size=800,600"]
    // }
  },

  //options for Jasmine
  jasmineNodeOpts: {
    showColors: true,
    //Jasmine assertions timeout
    defaultTimeoutInterval: 150000
  },

  specs: ["../JSFiles/specs/*.spec.js"],

  onPrepare: () => {
    // to work with non-angular pages. deprecated
    browser.ignoreSynchronization = true;
    // Use `jasmine-allure-reporter` as the spec result reporter
    var AllureReporter = require("jasmine-allure-reporter");
    jasmine.getEnv().addReporter(
      new AllureReporter({
        allureReport: {
          resultsDir: "allure-results"
        }
      })
    );
    let addScreenShots = new (function() {
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
    baseUrl: "https://vega.systest.cha.rbxd.ds/",
    expectedUrlAfterNavigation: "https://vega.systest.cha.rbxd.ds/app"
  },
  //protractor timeouts
  getPageTimeout: 30000,
  allScriptsTimeout: 30000,
  plugins: [],

  onComplete: () => {
    browser.close();
  }
};
