import { browser, by, element } from 'protractor';

import { BrowserUtil } from './../utils';

export class AppPage extends BrowserUtil {
    public $mainNav(navText: string) {
        return element(by.cssContainingText('.nav-text', navText));
    }

    public $commentaryschema() {
        return element(by.xpath('//li[1]//a[1]//span[1]'));
    }

    public $alertCloseButtons() {
        return element.all(by.xpath('//div[contains(@class, \'alert\')]/button'));
    }

    public async selectContentMenuItem() {
        await this.selectMenuItem('Content');
    }

    public async selectSettingsMenuItems() {
        await this.selectMenuItem('Settings');
    }

    public async selectMenuItem(navText: string) {
        const navItem = this.$mainNav(navText);

        // Just wait a little bit for the animation to finish.
        await browser.sleep(1000);

        await this.waitForElementToBeVisibleAndClick(navItem);
    }

    public async selectCommentarySchema() {
        const navItem = this.$commentaryschema();

        // Just wait a little bit for the animation to finish.
        await browser.sleep(1000);

        await this.waitForElementToBeVisibleAndClick(navItem);
    }

    public async closeAlerts() {
        const alerts = this.$alertCloseButtons();

        await alerts.click();
    }
}