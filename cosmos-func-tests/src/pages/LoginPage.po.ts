import { browser, by, element } from 'protractor';

import {
    BrowserUtil
} from './../utils/';

import { config } from '../config';

/**
 * Class representing login page.
 * Login window which opens after clicking on Login button on Squidex base page
 */
export class LoginPage extends BrowserUtil {
    /**
     * signs in using specified username/password in login.json file.
     */
    public async login(loginData: { username: string, password: string }) {
        await this.waitForAngularDisabledOnCurrentWindow();
        await this.navigateTo();
        const usernameInput = element(by.id('userNameInput'));
        const passwordInput = element(by.id('passwordInput'));
        const submitButton = element(by.id('submitButton'));
        await usernameInput.sendKeys(loginData.username);
        await passwordInput.sendKeys(loginData.password);
        await submitButton.click();
    }


    // navigating to Squidex base page
    public async navigateTo() {
        await browser.get(config.params.baseUrl);
    }

    public async navigateToApp() {
        await browser.get(`${config.params.baseUrl}/app`);
    }
}
