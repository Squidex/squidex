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
        await loginPage.login(Users.find(u => u.name === 'vegaEditor'));
    });

    afterAll(async () => {
        await homePage.logout();
    });

    it('Login with Vega Editor credentials', async () => {
        const welcomeText = await homePage.getWelcomeText();
        expect(welcomeText).toEqual(constants.loginTest.editorWelcomeMessage);

        const currentUrl = await homePage.getCurrentURL();
        expect(currentUrl).toBe(`${browser.params.baseUrl}/app`);
    });
});
