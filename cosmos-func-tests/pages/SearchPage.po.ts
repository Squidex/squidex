import {  by, element } from 'protractor';
import { ContentPage } from './ContentPage.po';

export class SearchPage extends ContentPage {

    public async getContentsList() {
        return await element.all(by.xpath('//table[@class=\'table table-items table-fixed\']/tbody/tr[@ng-reflect-can-clone=\'true\']/td[7]/sqx-content-value/span')).getWebElements();
    }

    public async getBoldContentText() {
        return await element(by.tagName('b'));
    }

    public async getItalicContentText() {
        return await element(by.tagName('i'));
    }

    public async getBulletPointContentTest() {
        return await element(by.tagName('ul')).element(by.tagName('li'));
    }

    public async getNumberedContentText() {
        return await element(by.tagName('ol')).element(by.tagName('li'));
    }

    public async getCommentaryEditorInput() {
        return await element(by.xpath('//div[@contenteditable=\'true\']'));
    }

    public async getCommentaryCreationSuccessMessageText() {
        return await element(by.xpath('//div[@class=\'alert alert-dismissible alert-info ng-trigger ng-trigger-fade ng-star-inserted\']/span')).getText();
    }

    public async verifyCommentaryCreation() {
        let value = null;
        this.getContent('content');
        return value = await this.getCommentary(await this.getCommentaryEditorInput());
    }

    public async getContent(contentBody: string) {
        await this.getContentsList().then(async (contents) => {
            await contents.filter(async (content) => {
                await content.getText().then(async (text) => {
                    return (await text.indexOf(contentBody) !== -1);
                });
                await content.click();
            });
        });
    }


    public async verifyBoldCommentaryCreation() {
        let value = null;
        this.getContent('Bold');
        return value = await this.getCommentary(await this.getBoldContentText());
    }


    public async verifyItalicCommentaryCreation() {
        let value = null;
        this.getContent('Italic');
        return value = await this.getCommentary(await this.getItalicContentText());
    }


    public async verifyNumberedCommentaryCreation() {
        let value = null;
        this.getContent('Numbered');
        return value = await this.getCommentary(await this.getItalicContentText());
    }

    public async verifyBulletPointsCommentaryCreation() {
        let value = null;
        this.getContent('Bullet');
        return value = await this.getCommentary(await this.getItalicContentText());
    }
}