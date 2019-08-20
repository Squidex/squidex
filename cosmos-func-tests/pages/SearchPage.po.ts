import { by, element } from 'protractor';
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

    public async getCommentaryCreationFailureMessageText() {
        return await element(by.xpath('//div[@class=\'alert alert-dismissible alert-danger ng-trigger ng-trigger-fade ng-star-inserted\']/span')).getText();
    }

    public async selectContentByText(contentBody: string) {
        await this.getContentsList().then(async (contents) => {
            await contents.filter(async (content) => {
                await content.getText().then(async (text) => {
                    return (await text.indexOf(contentBody) !== -1);
                });
                await content.click();
            });
        });
    }

    public async verifyCommentaryCreation() {
        this.selectContentByText('content');
        return await this.getCommentary(await this.getCommentaryEditorInput());
    }

    public async verifyBoldCommentaryCreation() {
        this.selectContentByText('Bold');
        return await this.getCommentary(await this.getBoldContentText());
    }


    public async verifyItalicCommentaryCreation() {
        this.selectContentByText('Italic');
        return await this.getCommentary(await this.getItalicContentText());
    }


    public async verifyNumberedCommentaryCreation() {
        this.selectContentByText('Numbered');
        return await this.getCommentary(await this.getItalicContentText());
    }

    public async verifyBulletPointsCommentaryCreation() {
        this.selectContentByText('Bullet');
        return await this.getCommentary(await this.getItalicContentText());
    }
}