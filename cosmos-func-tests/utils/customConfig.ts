// TODO: Remove?

/*import { browser, protractor } from "protractor";

const origFn = browser.driver.controlFlow().execute;
browser.driver.controlFlow().execute = function () {
const args = arguments;
// queue 100ms wait
origFn.call(browser.driver.controlFlow(), function () {
return protractor.promise.delayed(500);   // here we can adjust the execution speed
});
return origFn.apply(browser.driver.controlFlow(), args);
};
*/