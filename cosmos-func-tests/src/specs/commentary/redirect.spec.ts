import { browser } from 'protractor';
import {
    BrowserUtil,
    constants,
    Users
} from '../../utils';

import {
    ContentPage,
    HomePage,
    LoginPage,
    SearchPage
} from '../../pages';


describe('VEGA-358  : Re-direct to Landing Page From App View', () => {

    let homePage: HomePage;
    let loginPage: LoginPage;
    let contentPage: ContentPage;
    let browserPage: BrowserUtil;
    let searchPage: SearchPage;

    beforeAll(async () => {
        loginPage = new LoginPage();
        await loginPage.login(Users.find(u => u.name === 'vegaAdmin')!);
    });

    beforeEach(async () => {
        // initializing page object classes before every test so they don't refer to stale elements
        browserPage = new BrowserUtil();
        contentPage = new ContentPage();
        homePage = new HomePage();
        loginPage = new LoginPage();
        searchPage = new SearchPage();

        await homePage.navigateTo();
        await homePage.selectCommentaryApp('commentary');
    });

    afterAll(async () => {
        await homePage.logout();
        // setting a timeout between logout and login of another spec for the test not to time out
        await browser.sleep(1000);
    });

    describe('VEGA-358  : Re-direct to Landing Page From App View', () => {
        it('Verify user is redirected to commentary list page by default when commentaries app is opened', async () => {

            // Assert
            const currentUrl = await browserPage.getCurrentURL(constants.commentaryRedirectTest.commentaryUrl);
            expect(currentUrl).toBe(true);
        });

    });

    describe('VEGA-363 : Re-assign the function of the close (x) button when in commentary content view', () => {

        it('Verify user is redirected to commentary list page by default when content creation window is closed', async () => {

            // Act
            // open an existing commentary
            await searchPage.selectContentByText('Bold');
            // close the commentary
            await contentPage.navigateToContentsTable();

            // Assert
            const currentUrl = await browserPage.getCurrentURL(constants.commentaryRedirectTest.commentaryUrl);
            expect(currentUrl).toBe(true);
        });
    });

});