import { browser, by, element } from 'protractor';

import { BrowserUtil } from './../utils';

export class AppPage extends BrowserUtil {
    public async mainNav(navText: string) {
        return await element(by.cssContainingText('.nav-text', navText));
    }

    public async commentaryschema() {
        return await element(by.xpath('//li[1]//a[1]//span[1]'));
    }

    public $alertCloseButtons() {
        return element.all(by.xpath('//div[contains(@class, \'alert\')]/button'));
    }

    public async selectContentMenuItem() {
        await this.selectMenuItem('Content');
    }

    public async selectSettingsMenuItem() {
        await this.selectMenuItem('Settings');
    }

    public async selectMenuItem(navText: string) {
        const navItem = await this.mainNav(navText);

        // Just wait a little bit for the animation to finish.
        await browser.sleep(1000);

        await this.browserScriptToClick(navItem);

    }

    public async selectCommentarySchema() {
        const navItem = await this.commentaryschema();

        // Just wait a little bit for the animation to finish.
        await browser.sleep(1000);

        await this.browserScriptToClick(navItem);
    }

    public async closeAlerts() {
        const alerts = this.$alertCloseButtons();
        alerts.each(async (alert) => {
            await this.waitForElementToBeVisibleAndClick(alert);
        });
    }
}