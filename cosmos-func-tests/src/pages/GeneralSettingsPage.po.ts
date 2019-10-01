import { by, element } from 'protractor';
import { BrowserUtil } from '../utils';
let path = require('path');


export class GeneralSettingsPage extends BrowserUtil {

    public async labelField() {
        return await element(by.css('#label'));
    }

    public async descriptionField() {
        return await element(by.css('#description'));
    }

    public async uploadImageButton() {
        return await element(by.xpath('//span[contains(text(),\'Upload File\')]'));
    }

    public async imagePathInput() {
        return await element(by.xpath('//input[@type=\'file\']'));
    }

    public async imgSrc() {
        return await this.getWhenVisible(element(by.tagName('sqx-avatar')).element(by.tagName('img')));
    }

    public async saveContent() {
        return await this.waitForElementToBeVisibleAndClick(element(by.buttonText('Save')));
    }

    public async appsDropDown() {
        return await this.waitForElementToBeVisibleAndClick(element(by.id('app-name')));
    }

    public async selectAllApps() {
        return await this.waitForElementToBeVisibleAndClick(element(by.cssContainingText('.all-apps-text', 'All Apps')));
    }

    public async updateLabel(label: string) {
        await this.waitForElementToBePresentAndWrite(await this.labelField(), label);
    }

    public async updateDescription(desc: string) {
        await this.waitForElementToBePresentAndWrite(await this.descriptionField(), desc);
    }

    public async navigateToAppHomePage() {
        await this.appsDropDown();
        await this.selectAllApps();
    }


    public async uploadImage(imgPath: string) {

        let absolutePath = path.resolve(__dirname, imgPath);
        await this.imagePathInput().then((imageInput) => {
            imageInput.sendKeys(absolutePath);
        });
        await this.uploadImageButton();

    }

}