import { ILoginData } from "../../data/ILoginData";
import { browser, element, ExpectedConditions, by, Browser } from "protractor";
import { LoginPage } from "../../pages/LoginPage.po";
import { BrowserUtil } from "../../utils/Browser.util";
import { HomePage } from "../../pages/HomePage.po";
import { config } from "../../config";
import constants from "../../utils/constants";
import { CreateContent } from "../../pages/CreateContent.po";

describe("Create Commentary", () => {
  let loginData = require("../../../data/login.json");
  var authors: Array<ILoginData> = loginData.authors;
  let loginPg: LoginPage;
  let homePg: HomePage;
  let browserPg: BrowserUtil;
  let createContentPg: CreateContent;

  beforeEach(() => {
    loginPg = new LoginPage();
    homePg = new HomePage();
    browserPg = new BrowserUtil();
    createContentPg = new CreateContent();
  });

  // afterAll(() => {
  //   homePg.userLogout();
  // });
  //  describe("Login To Squidex", () => {

  it("Login with Vega Editor credentials", () => {
    loginPg.navigateTo();
    loginPg.loginButton().then(() => {
      browserPg.waitForAngularDisabledOnCurrentWindow();
      browserPg.switchToChildWindow();
      loginPg.login(
        authors.find(function(obj) {
          return obj.name === "vegaEditor";
        })
      );
    });
    browserPg.waitForAngularEnabledOnCurrentWindow().then(async () => {
      browserPg.switchToParentWindow();
      browser.sleep(1000);
      // loginPg.skipTour();
      browser.sleep(1000);
      await expect(browserPg.getCurrentURL()).toBe(
        config.params.expectedUrlAfterNavigation
      );
      await expect(homePg.userNameDisplay().getText()).toBe(
        constants.editorWelcomeMessage
      );
    });
  });

  it("should create valid commentary", () => {
    createContentPg.NavigateToContentPage();
    browser.sleep(3000);
    createContentPg.SelectCommodity(constants.commodity);
    createContentPg.SelectCommentaryType(constants.commentaryType);
    createContentPg.SelectRegion(constants.region);
    createContentPg.AddCommentary();
  });
});
