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

        await this.waitForElementToBePresentClearAndWrite(this.$usernameInput(), loginData.username);
        await this.waitForElementToBePresentClearAndWrite(this.$passwordInput(), loginData.password);

        await this.waitForElementToBeVisibleAndClick(this.$loginButton());
    }

    public async navigateTo() {
        await browser.get(browser.params.baseUrl);
    }
}
