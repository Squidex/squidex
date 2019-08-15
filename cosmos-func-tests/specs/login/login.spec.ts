import { browser } from 'protractor';
import { config } from '../../config';
import { ILoginData } from '../../data/ILoginData';
import { CreateContent } from '../../pages/CreateContent.po';
import { HomePage } from '../../pages/HomePage.po';
import { LoginPage } from '../../pages/LoginPage.po';
import SearchContent from '../../pages/SearchContent.po';
import { BrowserUtil } from '../../utils/Browser.util';
import constants from '../../utils/constants';

describe('Create Commentary', () => {
    const loginData = require('../../../data/login.json');
    const authors: ILoginData[] = loginData.authors;
    let loginPg: LoginPage;
    let homePg: HomePage;
    let browserPg: BrowserUtil;
    let createContentPg: CreateContent;
    let searchContentPg: SearchContent;

    beforeEach(async () => {
        loginPg = new LoginPage();
        homePg = new HomePage();
        browserPg = new BrowserUtil();
        createContentPg = new CreateContent();
        searchContentPg = new SearchContent();
        await loginPg.login(
            authors.find(obj => {
                return obj.name === 'vegaEditor';
            })
        );
        await browser.sleep(3000);
    });

    afterAll(() => {
        homePg.userLogout();
    });

    xit('Login with Vega Editor credentials', async () => {
        await expect(browserPg.getCurrentURL()).toBe(
            config.params.expectedUrlAfterNavigation
        );
        const text = await homePg.userNameDisplay();
        await expect(text).toEqual(constants.editorWelcomeMessage);
    });

    it('should create valid commentary', async () => {
        await createContentPg.navigateToCommentaryAppPage();
        // await createContentPg.selectCommodity(constants.commodity);
        // await createContentPg.selectCommentaryType(constants.commentaryType);
        // await createContentPg.selectRegion(constants.region);
        await createContentPg.createCommentary(constants.contentBody);
        const commentaryText = await searchContentPg.verifyCommentaryCreation();
        await expect(commentaryText).toBe(constants.contentBody);
    });

    xit('Commentary should support bold text', () => {
        createContentPg.createCommentaryWithBoldLetters(constants.boldCommentary);
        const commentaryText = searchContentPg.verifyBoldCommentaryCreation();
        expect(commentaryText).toBe(constants.boldCommentary);
    });

    xit('Commentary should support Italic text', () => {
        createContentPg.createCommentaryWithItalicFont(constants.italicCommentary);
        const commentaryText = searchContentPg.verifyItalicCommentaryCreation();
        expect(commentaryText).toBe(constants.italicCommentary);
    });

    xit('Commentary should support numbered list', () => {
        createContentPg.createNumberedCommentary(constants.numberedList);
        const commentaryText = searchContentPg.verifyNumberedCommentaryCreation();
        expect(commentaryText).toBe(constants.italicCommentary);
    });

    xit('Commentary should support bulleted list', () => {
        createContentPg.createBulletPointsCommentary(constants.bulletPoints);
        const commentaryText = searchContentPg.verifyBulletPointsCommentaryCreation();
        expect(commentaryText).toBe(constants.italicCommentary);
    });
});
