import { browser } from 'protractor';
import {
    constants,
    Users
} from '../../utils';

import {
    HomePage,
    LoginPage,
} from '../../pages';

describe('VEGA-358 : Re-direct to Landing Page From App View', () => {

    const homePage = new HomePage();
    const loginPage = new LoginPage();

    beforeAll(async () => {
        await loginPage.login(Users.find(u => u.name === 'vegaEditor')!);
    });

    afterAll(async () => {
        await homePage.logout();
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

})