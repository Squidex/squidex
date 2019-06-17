import { ILoginData } from "../../data/ILoginData";
import { browser, element, ExpectedConditions, by, Browser } from "protractor";
import { LoginPage } from "../../pages/LoginPage.po";
import { BrowserUtil } from "../../utils/Browser.util";
import { HomePage } from "../../pages/HomePage.po";
import { config } from "../../config";
import constants from "../../utils/constants";

describe("Login To Squidex", () => {
  let loginData = require("../../../data/login.json");
  var authors: Array<ILoginData> = loginData.authors;
  let loginPg: LoginPage;
  let homePg: HomePage;
  let browserPg: BrowserUtil;

  beforeEach(() => {
    loginPg = new LoginPage();
    homePg = new HomePage();
    browserPg = new BrowserUtil();
  });

  afterEach(() => {
    homePg.userLogout();
  });


  
  it("Login with Vega Admin credentials", () => {
    loginPg.navigateTo();
    loginPg.loginButton().then(() => {
      browserPg.waitForAngularDisabledOnCurrentWindow();
      browserPg.switchToChildWindow();
      loginPg.login(
        authors.find(function(obj) {
          return obj.name === "vegaAdmin";
        })
      );
    });
    browserPg.waitForAngularEnabledOnCurrentWindow().then(async () => {
      browserPg.switchToParentWindow();
      browser.sleep(1000);
      await loginPg.skipTour();
      browser.sleep(1000);
      await expect(browserPg.getCurrentURL()).toBe(
        config.params.expectedUrlAfterNavigation
      );
      await expect(homePg.userNameDisplay().getText()).toBe(
        constants.editorWelcomeMessage
      );
    });
  });
});
