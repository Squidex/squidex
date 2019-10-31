import { browser, by, element } from 'protractor';

import { BrowserUtil } from './../utils';

export class HomePage extends BrowserUtil {

    public $getProfileDropdown() {
        return element(by.css('.user'));
    }

    public $getWelcomeElement() {
        return element(by.className('apps-title'));
    }

    public $getProfileIcon() {
        return element(by.className('user-picture'));
    }

    public $getLogoutButton() {
        return element(by.xpath('//a[contains(text(),\'Logout\')]'));
    }

    public $getImage() {
        return element(by.tagName('img')).get(1);
    }

    public async getAppName(appName: string) {
        return await element(by.cssContainingText('.card-title', appName));
    }

    public async getWelcomeText() {
        return await this.waitForElementToBeVisibleAndGetText(this.$getWelcomeElement());
    }

    public async getDescription() {
        return await this.waitForElementToBeVisibleAndGetText(element.all(by.css('.card-text')).get(1));
    }

    public async getAppNameAfterChange() {
        return await this.waitForElementToBeVisibleAndGetText(element(by.css('.card-title')));
    }

    public async selectCommentaryApp(appName: string) {
        return await this.selectApp(appName);
    }

    public async selectApp(appName: string) {
        const card = await this.getAppName(appName);

        await this.waitForElementToBeVisibleAndClick(card);
    }

    public async navigateTo() {
        await browser.get(`${browser.params.baseUrl}/app`);
    }

    public async logout() {
        await this.waitForElementToBeVisibleAndClick(this.$getProfileDropdown());
        await this.waitForElementToBeVisibleAndClick(this.$getLogoutButton());
    }

}
