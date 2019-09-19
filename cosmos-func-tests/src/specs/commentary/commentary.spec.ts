import { browser } from 'protractor';
import {
  BrowserUtil,
  constants,
  Users
} from './../../utils';

import {
  ContentPage,
  HomePage,
  LoginPage,
  SearchPage
} from './../../pages';

describe('Create Commentary', () => {
  const authors = Users;
  let loginPage: LoginPage;
  let homePage: HomePage;
  let browserPage: BrowserUtil;
  let contentPage: ContentPage;
  let searchPage: SearchPage;

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

  it('should allow the user to search and filter ref data with text and bring the matching results', async () => {

      await contentPage.selectDate(3);
      await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.commentaryTest.commodityValue);
      await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.commentaryTest.commentaryTypeValue);
      await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.commentaryTest.regionValue);
      await contentPage.createCommentary(constants.commentaryTest.contentBody);

      const alertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
      expect(alertMessage).toBe(constants.messages.commentaryCreationSuccessMessage);

      await searchPage.selectContentByText(constants.commentaryTest.contentBody);

      const commodityValue = await searchPage.verifyRefDataSelection(constants.refDataLocators.commodity);
      expect(commodityValue).toBe(constants.commentaryTest.commodityValue);

      const commentaryTypeValue = await searchPage.verifyRefDataSelection(constants.refDataLocators.commentaryType);
      expect(commentaryTypeValue).toBe(constants.commentaryTest.commentaryTypeValue);

      const regionValue = await searchPage.verifyRefDataSelection(constants.refDataLocators.region);
      expect(regionValue).toBe(constants.commentaryTest.regionValue);

      const commentaryText = await searchPage.verifyCommentaryCreation();
      expect(commentaryText).toBe(constants.commentaryTest.contentBody);

    });

  it('should allow the user to search and filter ref data with partial text and bring the matching results', async () => {

      await contentPage.selectDate(3);
      await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.partialCommentaryTest.commodityValue);
      await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.partialCommentaryTest.commentaryTypeValue);
      await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.partialCommentaryTest.regionValue);
      await contentPage.createCommentary(constants.partialCommentaryTest.contentBody);

      const alertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
      expect(alertMessage).toBe(constants.messages.commentaryCreationSuccessMessage);

      await searchPage.selectContentByText(constants.partialCommentaryTest.contentBody);

      const commodityValue = await searchPage.verifyRefDataSelection(constants.refDataLocators.commodity);
      expect(commodityValue).toBe(constants.partialCommentaryTest.commodityValueFilteredByPartialText);

      const commentaryTypeValue = await searchPage.verifyRefDataSelection(constants.refDataLocators.commentaryType);
      expect(commentaryTypeValue).toBe(constants.partialCommentaryTest.commentaryTypeValueFilteredByPartialText);

      const regionValue = await searchPage.verifyRefDataSelection(constants.refDataLocators.region);
      expect(regionValue).toBe(constants.partialCommentaryTest.regionValueFilteredByPartialText);

      const commentaryText = await searchPage.verifyCommentaryCreation();
      expect(commentaryText).toBe(constants.partialCommentaryTest.contentBody);

    });


  it('should allow the user to edit the existing commentary and verify values', async () => {

    await contentPage.selectDate(2);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.editCommentaryTest.commodityValue);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.editCommentaryTest.commentaryTypeValue);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.editCommentaryTest.regionValue);
    await contentPage.createCommentary(constants.editCommentaryTest.contentBody);

    const alertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
    expect(alertMessage).toBe(constants.messages.commentaryCreationSuccessMessage);

    await searchPage.selectContentByText(constants.editCommentaryTest.contentBody);

    await contentPage.selectDate(3);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.editCommentaryTest.modifiedCommodityValue);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.editCommentaryTest.modifiedCommentaryTypeValue);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.editCommentaryTest.modifiedRegionValue);
    await contentPage.createCommentary(constants.editCommentaryTest.modifiedContentBody);

    const editAlertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
    expect(editAlertMessage).toBe(constants.messages.commentaryEditSuccessMessage);

    await contentPage.navigateToContentsTable();
    await searchPage.selectContentByText(constants.editCommentaryTest.modifiedContentBody);

    const commodityValue = await searchPage.verifyRefDataSelection(constants.refDataLocators.commodity);
    expect(commodityValue).toBe(constants.editCommentaryTest.modifiedCommodityValue);

    const commentaryTypeValue = await searchPage.verifyRefDataSelection(constants.refDataLocators.commentaryType);
    expect(commentaryTypeValue).toBe(constants.editCommentaryTest.modifiedCommentaryTypeValue);

    const regionValue = await searchPage.verifyRefDataSelection(constants.refDataLocators.region);
    expect(regionValue).toBe(constants.editCommentaryTest.modifiedRegionValue);

    const commentaryText = await searchPage.verifyCommentaryCreation();
    expect(commentaryText).toBe(constants.editCommentaryTest.modifiedContentBody);

  });

  it('should throw error for duplicate commentaries with same ref data', async () => {

    await contentPage.selectDate(3);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.duplicateCommentaryCreationTest.commodityValue);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.duplicateCommentaryCreationTest.commentaryTypeValue);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.duplicateCommentaryCreationTest.regionValue);
    await contentPage.createCommentary(constants.duplicateCommentaryCreationTest.contentBody);

    await contentPage.clickOnNewButton();

    await contentPage.selectDate(3);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.duplicateCommentaryCreationTest.commodityValue);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.duplicateCommentaryCreationTest.commentaryTypeValue);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.duplicateCommentaryCreationTest.regionValue);
    await contentPage.createCommentary(constants.duplicateCommentaryCreationTest.contentBody);

    const message = contentPage.captureContentValidationMessage();
    expect<any>(message).toBe(constants.messages.validationFailureErrorMessage);

  });

  it('should auto save commentary', async () => {

    await contentPage.selectDate(4);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.commentaryTest.commodityValue);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.commentaryTest.commentaryTypeValue);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.commentaryTest.regionValue);
    await contentPage.createCommentary(constants.commentaryTest.contentBody);

    // this browser.sleep ensures saving after writing commentary, which will enable auto save
    // without this sleep statement, test is failing as auto save is not storing values within one second and not returning them
    await browser.sleep(2000);
    await browserPage.browserRefresh();

    expect(await contentPage.autoSavePopUp()).not.toBeNull();
    await contentPage.acceptAutoSave();

    const commodityValue = await searchPage.verifyRefDataSelection(constants.refDataLocators.commodity);
    expect(commodityValue).toBe(constants.commentaryTest.commodityValue);

    const commentaryTypeValue = await searchPage.verifyRefDataSelection(constants.refDataLocators.commentaryType);
    expect(commentaryTypeValue).toBe(constants.commentaryTest.commentaryTypeValue);

    const regionValue = await searchPage.verifyRefDataSelection(constants.refDataLocators.region);
    expect(regionValue).toBe(constants.commentaryTest.regionValue);

    const commentaryText = await searchPage.verifyCommentaryCreation();
    expect(commentaryText).toBe(constants.commentaryTest.contentBody);

  });


  it('should save the auto saved commentary', async () => {

    await contentPage.selectDate(4);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.savingAutoSavedCommentaryTest.commodityValue);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.savingAutoSavedCommentaryTest.commentaryTypeValue);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.savingAutoSavedCommentaryTest.regionValue);
    await contentPage.createCommentary(constants.savingAutoSavedCommentaryTest.contentBody);

    // this browser.sleep ensures saving after writing commentary, which will enable auto save
    // without this sleep statement, test is failing as auto save is not storing values within one second and not returning them
    await browser.sleep(1000);
    await browserPage.browserRefresh();

    expect(await contentPage.autoSavePopUp()).not.toBeNull();
    await contentPage.acceptAutoSave();

    const commodityValue = await searchPage.verifyRefDataSelection(constants.refDataLocators.commodity);
    expect(commodityValue).toBe(constants.savingAutoSavedCommentaryTest.commodityValue);

    const commentaryTypeValue = await searchPage.verifyRefDataSelection(constants.refDataLocators.commentaryType);
    expect(commentaryTypeValue).toBe(constants.savingAutoSavedCommentaryTest.commentaryTypeValue);

    const regionValue = await searchPage.verifyRefDataSelection(constants.refDataLocators.region);
    expect(regionValue).toBe(constants.savingAutoSavedCommentaryTest.regionValue);

    const commentaryText = await searchPage.verifyCommentaryCreation();
    expect(commentaryText).toBe(constants.savingAutoSavedCommentaryTest.contentBody);

    await contentPage.saveContent();
    await searchPage.selectContentByText(constants.savingAutoSavedCommentaryTest.contentBody);

    const commodityAfterSave = await searchPage.verifyRefDataSelection(constants.refDataLocators.commodity);
    expect(commodityAfterSave).toBe(constants.savingAutoSavedCommentaryTest.commodityValue);

    const commentaryTypeAfterSave = await searchPage.verifyRefDataSelection(constants.refDataLocators.commentaryType);
    expect(commentaryTypeAfterSave).toBe(constants.savingAutoSavedCommentaryTest.commentaryTypeValue);

    const regionValueAfterSave = await searchPage.verifyRefDataSelection(constants.refDataLocators.region);
    expect(regionValueAfterSave).toBe(constants.savingAutoSavedCommentaryTest.regionValue);

    const commentaryAfterSave = await searchPage.verifyCommentaryCreation();
    expect(commentaryAfterSave).toBe(constants.savingAutoSavedCommentaryTest.contentBody);

  });

  it('quit without saving the auto saved commentary and capture the pop-up', async () => {

    await contentPage.selectDate(4);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.commentaryTest.commodityValue);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.commentaryTest.commentaryTypeValue);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.commentaryTest.regionValue);
    await contentPage.createCommentary(constants.commentaryTest.contentBody);

    // this browser.sleep ensures saving after writing commentary, which will enable auto save
    // without this sleep statement, test is failing as auto save is not storing values within one second and not returning them
    await browser.sleep(1000);
    await browserPage.browserRefresh();

    expect(contentPage.autoSavePopUp()).toBeTruthy();
    await contentPage.acceptAutoSave();

    await contentPage.navigateToContentsTable();

    const popUp = await contentPage.captureUnsavedChangesPopUpMessage();
    expect<any>(popUp).toBe(constants.messages.unsavedChangesPopUpMessage);

  });


  it('should throw error for invalid ref data', async () => {
    await contentPage.selectDate(3);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.invalidRefDataTest.invalidRefDataValue);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.invalidRefDataTest.invalidRefDataValue);
    await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.invalidRefDataTest.invalidRefDataValue);
    await contentPage.createCommentary(constants.commentaryTest.contentBody);

    const alertMessage = await searchPage.getCommentaryCreationFailureMessageText();
    expect(alertMessage).toBe(constants.messages.commentaryCretaionFailureMessage);

  });

  it('should support Bold text', async () => {
    await contentPage.createCommentaryAndApplyEditorOptions(constants.tuiEditorOptionsTest.boldCommentaryContentBody, constants.refDataLocators.editorOptionsBold);
    const commentaryText = await searchPage.verifyBoldCommentaryCreation();
    expect(commentaryText).toBe(constants.tuiEditorOptionsTest.boldCommentaryContentBody);

  });

  it('should support Italic text', async () => {
    await contentPage.createCommentaryAndApplyEditorOptions(constants.tuiEditorOptionsTest.italicCommentaryContentBody, constants.refDataLocators.editorOptionsItalic);
    const commentaryText = await searchPage.verifyItalicCommentaryCreation();
    expect(commentaryText).toBe(constants.tuiEditorOptionsTest.italicCommentaryContentBody);
  });

  it('should support Numbered list', async () => {
    await contentPage.createCommentaryAndApplyEditorOptions(constants.tuiEditorOptionsTest.numberedListContentBody, constants.refDataLocators.editorOptionsNumberedList);
    const commentaryText = await searchPage.verifyNumberedCommentaryCreation();
    expect(commentaryText).toBe(constants.tuiEditorOptionsTest.numberedListContentBody);

  });

  it('should support Bulleted list', async () => {
    await contentPage.createCommentaryAndApplyEditorOptions(constants.tuiEditorOptionsTest.bulletPointsContentBody, constants.refDataLocators.editorOptionsBulletPointList);
    const commentaryText = await searchPage.verifyBulletPointsCommentaryCreation();
    expect(commentaryText).toBe(constants.tuiEditorOptionsTest.bulletPointsContentBody);

  });

});
