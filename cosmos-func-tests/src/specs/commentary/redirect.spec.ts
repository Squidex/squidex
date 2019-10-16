import { browser } from 'protractor';
import {
    constants,
    Users
} from '../../utils';

import {
    HomePage,
    LoginPage
} from '../../pages';


describe('VEGA-358 : Re-direct to Landing Page From App View', () => {

    let homePage: HomePage;
    let loginPage: LoginPage;

    beforeAll(async () => {
        loginPage = new LoginPage();
        await loginPage.login(Users.find(u => u.name === 'vegaEditor')!);
    });

    beforeEach(async () => {
        homePage = new HomePage();
    });

    afterAll(async () => {
        await homePage.logout();
        // setting a timeout between logout and login of another spec for the test not to time out
        await browser.sleep(1000);
    });

    it('should redirect to commentary list page by default when commentaries app is opened', async () => {

        // Act
        await homePage.selectCommentaryApp('commentary');

        // Assert
        // waiting for animation to finish
        await browser.sleep(1000);

        const currentUrl = await browser.getCurrentUrl();
        expect(currentUrl).toBe(constants.commentaryRedirectTest.commentaryUrl);
    });

});