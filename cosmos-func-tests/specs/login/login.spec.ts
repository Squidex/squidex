import { config } from '../../config';
import { ILoginData } from '../../data/ILoginData';
import { ContentPage } from '../../pages/ContentPage.po';
import { HomePage } from '../../pages/HomePage.po';
import { LoginPage } from '../../pages/LoginPage.po';
import { SearchPage }from '../../pages/SearchPage.po';
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

  it('Login with Vega Editor credentials', async () => {
    await expect(browserPage.getCurrentURL()).toBe(
      config.params.expectedUrlAfterNavigation
    );
    const text = await homePage.userNameDisplay();
    await expect(text).toEqual(constants.editorWelcomeMessage);
  });

  it('should allow the user to search and filter ref data with partial text', async () => {
    await contentPage.navigateToCommentaryAppPage();
    await contentPage.selectCommodity(constants.commodity);
    await contentPage.selectCommentaryType(constants.commentaryType);
    await contentPage.selectRegion(constants.region);
    await contentPage.createCommentary(constants.contentBody);
    const alertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
    await expect(alertMessage).toBe(constants.alertSuccessMessage);
    const commentaryText = await searchPage.verifyCommentaryCreation();
    await expect(commentaryText).toBe(constants.contentBody);
  });

  it('should create commentary with valid ref data and print success message alert', async () => {
    await contentPage.navigateToCommentaryAppPage();
    await contentPage.selectCommodity(constants.commodity);
    await contentPage.selectCommentaryType(constants.commentaryType);
    await contentPage.selectRegion(constants.region);
    await contentPage.createCommentary(constants.contentBody);
    const alertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
    await expect(alertMessage).toBe(constants.alertSuccessMessage);
    const commentaryText = await searchPage.verifyCommentaryCreation();
    await expect(commentaryText).toBe(constants.contentBody);
  });

  xit('should throw error for invalid ref data', async () => {
    await contentPage.navigateToCommentaryAppPage();
    await contentPage.selectCommodity(constants.commodity);
    await contentPage.selectCommentaryType(constants.commentaryType);
    await contentPage.selectRegion(constants.region);
    await contentPage.createCommentary(constants.contentBody);
    const alertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
    await expect(alertMessage).toBe(constants.alertSuccessMessage);
    const commentaryText = await searchPage.verifyCommentaryCreation();
    await expect(commentaryText).toBe(constants.contentBody);
  });

  xit('should support Bold text', async () => {
    contentPage.createCommentaryWithBoldLetters(constants.boldCommentary);
    const commentaryText = await searchPage.verifyBoldCommentaryCreation();
    await expect(commentaryText).toBe(constants.boldCommentary);
  });

  xit('should support Italic text', async () => {
    contentPage.createCommentaryWithItalicFont(constants.italicCommentary);
    const commentaryText = await searchPage.verifyItalicCommentaryCreation();
    await expect(commentaryText).toBe(constants.italicCommentary);
  });

  xit('should support Numbered list', async () => {
    contentPage.createNumberedCommentary(constants.numberedList);
    const commentaryText = await searchPage.verifyNumberedCommentaryCreation();
    await expect(commentaryText).toBe(constants.italicCommentary);
  });

  xit('should support Bulleted list', async () => {
    contentPage.createBulletPointsCommentary(constants.bulletPoints);
    const commentaryText = await searchPage.verifyBulletPointsCommentaryCreation();
    await expect(commentaryText).toBe(constants.italicCommentary);
  });
});
