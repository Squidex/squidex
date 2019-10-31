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
    });

    beforeEach(async () => {
        // initializing page object classes before every test so they don't refer to stale elements
        homePage = new HomePage();
        loginPage = new LoginPage();
    });

    it('Verify logging into Cosmos with valid credentials', async () => {

        const welcomeText = await homePage.getWelcomeText();
        expect(welcomeText).toEqual(constants.loginTest.adminWelcomeMessage);

        const currentUrl = await homePage.getCurrentURL(`${browser.params.baseUrl}/app`);
        expect(currentUrl).toBe(true);
    });
});
