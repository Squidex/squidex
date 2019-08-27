import { config } from '../../config';
import { ILoginData } from '../../data/ILoginData';
import { ContentPage } from '../../pages/ContentPage.po';
import { HomePage } from '../../pages/HomePage.po';
import { LoginPage } from '../../pages/LoginPage.po';
import { SearchPage } from '../../pages/SearchPage.po';
import { BrowserUtil } from '../../utils/Browser.util';
import constants from '../../utils/constants';


describe('Create Commentary', () => {
  const loginData = require('../../../data/login.json');
  const authors: ILoginData[] = loginData.authors;
  let loginPage: LoginPage;
  let homePage: HomePage;
  let browserPage: BrowserUtil;
  let contentPage: ContentPage;
  let searchPage: SearchPage;
  const using = require('jasmine-data-provider');

  beforeAll(async () => {
    loginPage = new LoginPage();
    homePage = new HomePage();
    browserPage = new BrowserUtil();
    contentPage = new ContentPage();
    searchPage = new SearchPage();
    await loginPage.login(
      authors.find(obj => {
        return obj.name === 'vegaEditor';
      })
    );
  });

  afterAll(() => {
    homePage.userLogout();
  });

  beforeEach(async () => {
    await loginPage.navigateToApp();
    await contentPage.navigateToCommentaryAppPage();
  });


  it('Login with Vega Editor credentials', async () => {
    await expect(browserPage.getCurrentURL()).toBe(
      config.params.expectedUrlAfterNavigation
    );
    const text = await homePage.userNameDisplay();
    await expect(text).toEqual(constants.editorWelcomeMessage);
  });

  using([{ commodityValue: constants.partialCommodityText, commentaryTypeValue: constants.partialCommentaryTypeText, regionValue: constants.partialRegionText },
  { commodityValue: constants.commodity, commentaryTypeValue: constants.commentaryType, regionValue: constants.region }], (data: any) => {
    it('should allow the user to search and filter ref data with partial text (or) full text and bring the matching results', async () => {
      await contentPage.selectDate(3);
      await contentPage.selectContentFromDropDown(constants.contentCommodity, data.commodityValue);
      await contentPage.selectContentFromDropDown(constants.contentCommentaryType, data.commentaryTypeValue);
      await contentPage.selectContentFromDropDown(constants.contentRegion, data.regionValue);
      await contentPage.createCommentary(constants.contentBody);
      const alertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
      await expect(alertMessage).toBe(constants.alertSuccessMessage);
      const commentaryText = await searchPage.verifyCommentaryCreation();
      await expect(commentaryText).toBe(constants.contentBody);
      const commodityValue = await searchPage.verifyRefDataSelection(constants.contentCommodity);
      await expect(commodityValue).toBe(data.commodityValue);
      const commentaryTypeValue = await searchPage.verifyRefDataSelection(constants.contentCommentaryType);
      await expect(commentaryTypeValue).toBe(data.commentaryTypeValue);
      const regionValue = await searchPage.verifyRefDataSelection(constants.contentRegion);
      await expect(regionValue).toBe(data.regionValue);
    });
  });

  it('should allow the user to edit the existing commentary and verify values', async () => {
    await contentPage.selectDate(2);
    await contentPage.selectContentFromDropDown(constants.contentCommodity, constants.commodity);
    await contentPage.selectContentFromDropDown(constants.contentCommentaryType, constants.commentaryType);
    await contentPage.selectContentFromDropDown(constants.contentRegion, constants.region);
    await contentPage.createCommentary(constants.commentaryContentText);
    await searchPage.selectContentByText(constants.commentaryContentText);
    await contentPage.selectDate(3);
    await contentPage.selectContentFromDropDown(constants.contentCommodity, constants.modifiedCommodity);
    await contentPage.selectContentFromDropDown(constants.contentCommentaryType, constants.modifiedCommentaryType);
    await contentPage.selectContentFromDropDown(constants.contentRegion, constants.modifiedRegion);
    await contentPage.createCommentary(constants.editContentTestText);
    await searchPage.selectContentByText(constants.editContentTestText);
    const alertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
    await expect(alertMessage).toBe(constants.alertSuccessMessage);
    const commentaryText = await searchPage.verifyCommentaryCreation();
    await expect(commentaryText).toBe(constants.contentBody);
    const commodityValue = await searchPage.verifyRefDataSelection(constants.contentCommodity);
    await expect(commodityValue).toBe(constants.modifiedCommentaryType);
    const commentaryTypeValue = await searchPage.verifyRefDataSelection(constants.contentCommentaryType);
    await expect(commentaryTypeValue).toBe(constants.modifiedCommentaryType);
    const regionValue = await searchPage.verifyRefDataSelection(constants.contentRegion);
    await expect(regionValue).toBe(constants.modifiedRegion);
  });

  it('should throw error for duplicate commentaries with same ref data', async () => {
    await contentPage.selectDate(3);
    await contentPage.selectContentFromDropDown(constants.contentCommodity, constants.duplicateTestCommodity);
    await contentPage.selectContentFromDropDown(constants.contentCommentaryType, constants.duplicateTestCommentaryType);
    await contentPage.selectContentFromDropDown(constants.contentRegion, constants.duplicateTestRegion);
    await contentPage.createCommentary(constants.duplicateTestContentBody);
    await contentPage.clickOnNewButton();
    await contentPage.selectDate(3);
    await contentPage.selectContentFromDropDown(constants.contentCommodity, constants.duplicateTestCommodity);
    await contentPage.selectContentFromDropDown(constants.contentCommentaryType, constants.duplicateTestCommentaryType);
    await contentPage.selectContentFromDropDown(constants.contentRegion, constants.duplicateTestRegion);
    await contentPage.createCommentary(constants.duplicateTestContentBody);
    const message = contentPage.captureContentValidationMessage();
    await expect<any>(message).toBe(constants.validationErrorMessage);
  });

  it('should throw error for invalid ref data', async () => {
    await contentPage.selectDate(3);
    await contentPage.selectContentFromDropDown(constants.contentCommodity, constants.invalidRefDataText);
    await contentPage.selectContentFromDropDown(constants.contentCommentaryType, constants.invalidRefDataText);
    await contentPage.selectContentFromDropDown(constants.contentRegion, constants.invalidRefDataText);
    await contentPage.createCommentary(constants.contentBody);
    const alertMessage = await searchPage.getCommentaryCreationFailureMessageText();
    await expect(alertMessage).toBe(constants.failureMessage);
  });

  it('should support Bold text', async () => {
    await contentPage.createCommentaryAndApplyEditorOptions(constants.boldCommentary, constants.editorOptionsBold);
    const commentaryText = await searchPage.verifyBoldCommentaryCreation();
    await expect(commentaryText).toBe(constants.boldCommentary);
  });

  it('should support Italic text', async () => {
    await contentPage.createCommentaryAndApplyEditorOptions(constants.boldCommentary, constants.editorOptionsItalic);
    const commentaryText = await searchPage.verifyItalicCommentaryCreation();
    await expect(commentaryText).toBe(constants.italicCommentary);
  });

  it('should support Numbered list', async () => {
    await contentPage.createCommentaryAndApplyEditorOptions(constants.boldCommentary, constants.editorOptionsNumberedList);
    const commentaryText = await searchPage.verifyNumberedCommentaryCreation();
    await expect(commentaryText).toBe(constants.italicCommentary);
  });

  it('should support Bulleted list', async () => {
    await contentPage.createCommentaryAndApplyEditorOptions(constants.boldCommentary, constants.editorOptionsBulletPointList);
    const commentaryText = await searchPage.verifyBulletPointsCommentaryCreation();
    await expect(commentaryText).toBe(constants.italicCommentary);
  });
});
