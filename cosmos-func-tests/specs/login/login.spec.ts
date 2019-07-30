import { ILoginData } from "../../data/ILoginData";
import { browser, element, ExpectedConditions, by, Browser } from "protractor";
import { LoginPage } from "../../pages/LoginPage.po";
import { BrowserUtil } from "../../utils/Browser.util";
import { HomePage } from "../../pages/HomePage.po";
import { config } from "../../config";
import constants from "../../utils/constants";
import { CreateContent } from "../../pages/CreateContent.po";
import SearchContent from "../../pages/SearchContent.po";
import { AssertionError } from "assert";

describe("Create Commentary", () => {
  let loginData = require("../../../data/login.json");
  var authors: Array<ILoginData> = loginData.authors;
  let loginPg: LoginPage;
  let homePg: HomePage;
  let browserPg: BrowserUtil;
  let createContentPg: CreateContent;
  let searchContentPg: SearchContent;

  beforeEach(() => {
    loginPg = new LoginPage();
    homePg = new HomePage();
    browserPg = new BrowserUtil();
    createContentPg = new CreateContent();
    searchContentPg = new SearchContent();
  });

  // afterAll(() => {
  //   homePg.userLogout();
  // });

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
      await expect(homePg.userNameDisplay().getText().toString()).toBe(constants.editorWelcomeMessage);
    });
  });

  it("should create valid commentary", async () => {
    createContentPg.navigateToContentPage();
    createContentPg.selectCommodity(constants.commodity);
    createContentPg.selectCommentaryType(constants.commentaryType);
    createContentPg.selectRegion(constants.region);
    createContentPg.addCommentary(constants.contentBody);
    const commentaryText = searchContentPg.verifyCommentaryCreation().toString();
    expect(commentaryText).toBe(constants.contentBody);
  });

it("Commentary should support bold text", ()=>{
 createContentPg.createCommentaryWithBoldLetters(constants.boldCommentary);
 const commentaryText = searchContentPg.verifyBoldCommentaryCreation();
 expect(commentaryText).toBe(constants.boldCommentary);

})

it("Commentary should support Italic text", ()=>{
  createContentPg.createCommentaryWithItalicFont(constants.italicCommentary);
  const commentaryText = searchContentPg.verifyItalicCommentaryCreation();
  expect(commentaryText).toBe(constants.italicCommentary);
})

it("Commentary should support numbered list", ()=>{
  createContentPg.createNumberedCommentary(constants.numberedList);
  const commentaryText = searchContentPg.verifyNumberedCommentaryCreation();
  expect(commentaryText).toBe(constants.italicCommentary);

})

it("Commentary should support bulleted list", ()=>{
  createContentPg.createBulletPointsCommentary(constants.bulletPoints);
  const commentaryText = searchContentPg.verifyBulletPointsCommentaryCreation();
  expect(commentaryText).toBe(constants.italicCommentary);
})

});