import { browser, protractor } from "protractor";

var origFn = browser.driver.controlFlow().execute;
browser.driver.controlFlow().execute = function () {
var args = arguments;
// queue 100ms wait
origFn.call(browser.driver.controlFlow(), function () {
return protractor.promise.delayed(500);   // here we can adjust the execution speed
});
return origFn.apply(browser.driver.controlFlow(), args);
};