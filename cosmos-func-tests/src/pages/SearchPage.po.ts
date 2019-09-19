import { by, element } from 'protractor';

import { ContentPage } from './ContentPage.po';

export class SearchPage extends ContentPage {
    public async getContentsList() {
        return await element.all(by.xpath('//table[@class=\'table table-items table-fixed\']/tbody/tr[@ng-reflect-can-clone=\'true\']/td[7]/sqx-content-value/span')).getWebElements();
    }

    public async getBoldContentText() {
        return await element(by.xpath('//div[@class=\'tui-editor-contents\']/div/b'));
    }

    public async getItalicContentText() {
        return await element(by.xpath('//div[@class=\'tui-editor-contents\']/div/i'));
    }

    public async getBulletPointContentTest() {
        return await element(by.xpath('//div[@class=\'tui-editor-contents\']/div/ul/li'));
    }

    public async getNumberedContentText() {
        return await element(by.xpath('//div[@class=\'tui-editor-contents\']/div/ol/li'));
    }

    public async getCommentaryEditorInput() {
        return await element(by.xpath('//div[@contenteditable=\'true\']/div'));
    }

    public async getCommentaryCreationSuccessMessageText() {
        return await this.waitForElementToBeVisibleAndGetText(await element(by.xpath('//div[@class=\'alert alert-dismissible alert-info ng-trigger ng-trigger-fade ng-star-inserted\']/span')));
    }

    public async getCommentaryCreationFailureMessageText() {
        return await this.waitForElementToBeVisibleAndGetText(await element(by.xpath('//div[@class=\'alert alert-dismissible alert-danger ng-trigger ng-trigger-fade ng-star-inserted\']/span')));
    }

    public async verifyRefDataSelection(referenceName: string) {
        // tslint:disable-next-line: max-line-length
        return await this.waitForElementToBeVisibleAndGetText(await element(by.xpath('//label[contains(text(),\'' + referenceName + '\')]/following-sibling::div/sqx-references-dropdown/sqx-dropdown/span/div[@class=\'selection\']/div/span')));
    }

    public async selectContentByText(contentBody: string) {
        await this.getContentsList().then(async (contents) => {
            contents.filter(async (content) => {
                await content.getText().then(async (text) => {
                    return (text.indexOf(contentBody) !== -1);
                });
                await content.click();
            });
        });
    }

    public async verifyCommentaryCreation() {
        await this.commentaryEditor();
        return await this.getCommentary(await this.getCommentaryEditorInput());
    }

    public async verifyBoldCommentaryCreation() {
        await this.selectContentByText('Bold');
        await this.getCommentaryEditorFrame();
        return await this.getCommentary(await this.getBoldContentText());
    }


    public async verifyItalicCommentaryCreation() {
        await this.selectContentByText('Italic');
        await this.getCommentaryEditorFrame();
        return await this.getCommentary(await this.getItalicContentText());
    }


    public async verifyNumberedCommentaryCreation() {
        await this.selectContentByText('Numbered');
        await this.getCommentaryEditorFrame();
        return await this.getCommentary(await this.getNumberedContentText());
    }

    public async verifyBulletPointsCommentaryCreation() {
        await this.selectContentByText('Bullet');
        await this.getCommentaryEditorFrame();
        return await this.getCommentary(await this.getBulletPointContentTest());
    }
}