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
    const homePage = new HomePage();
    const loginPage = new LoginPage();

    beforeAll(async () => {
        await loginPage.login(Users.find(u => u.name === 'vegaAdmin'));
    });

    afterAll(async () => {
        await homePage.logout();
    });

    it('Verify logging into Cosmos with valid credentials', async () => {

        const welcomeText = await homePage.getWelcomeText();
        expect(welcomeText).toEqual(constants.loginTest.adminWelcomeMessage);

        const currentUrl = await homePage.getCurrentURL();
        expect(currentUrl).toBe(`${browser.params.baseUrl}/app`);
    });
});
