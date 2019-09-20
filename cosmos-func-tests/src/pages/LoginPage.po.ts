import { browser, by, element } from 'protractor';

import {
    BrowserUtil
} from './../utils/';

export class LoginPage extends BrowserUtil {
    public $loginButton() {
        return element(by.id('submitButton'));
    }

    public $passwordInput() {
        return element(by.id('passwordInput'));
    }

    public $usernameInput() {
        return element(by.id('userNameInput'));
    }

    public async login(loginData: { username: string, password: string }) {
        await this.waitForAngularDisabledOnCurrentWindow();
        await this.navigateTo();

        await this.$usernameInput().sendKeys(loginData.username);
        await this.$passwordInput().sendKeys(loginData.password);

        await this.$loginButton().click();
    }

    public async navigateTo() {
        await browser.get(browser.params.baseUrl);
    }
}
