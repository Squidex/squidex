import { browser } from 'protractor';

import {
    constants,
    Users
} from './../../utils';

import {
    HomePage,
    LoginPage
} from './../../pages';

describe('User Login', () => {
    let homePage: HomePage;
    let loginPage: LoginPage;

    beforeAll(async () => {
        loginPage = new LoginPage();
        await loginPage.login(Users.find(u => u.name === 'vegaAdmin'));
    });

    afterAll(async () => {
        await homePage.logout();
        // setting a timeout between logout and login of another spec for the test not to time out
        await browser.sleep(1000);
    });

    beforeEach(async () => {
        homePage = new HomePage();
    });

    it('Verify logging into Cosmos with valid credentials', async () => {

        const welcomeText = await homePage.getWelcomeText();
        expect(welcomeText).toEqual(constants.loginTest.adminWelcomeMessage);

        const currentUrl = await homePage.getCurrentURL();
        expect(currentUrl).toBe(`${browser.params.baseUrl}/app`);
    });
});
