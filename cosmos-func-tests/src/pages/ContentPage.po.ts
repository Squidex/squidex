import { browser, by, element, ElementFinder, protractor, WebElement } from 'protractor';
import { constants } from './../utils/constants';


import { BrowserUtil } from './../utils';

export class ContentPage extends BrowserUtil {
    // Create Commentary after Navigating to Commentary Page under Content
    public searchResult = element.all(by.xpath('//span[@class=\'truncate ng-star-inserted\']/b'));

    public calendar = element(by.xpath('//div[@class=\'input-group\']/input'));

    public getRefData = element.all(by.xpath('//div[@class=\'control-dropdown-items\']/div/span'));

    public async getCommentaryEditorFrame(): Promise<WebElement> {
        return await element(by.xpath('//iframe[@src=\'' + constants.refDataLocators.editorUrl + '\']')).getWebElement();
    }

    public async commentaryEditor() {
        return await this.waitForElementToBePresent(element(by.xpath('//iframe[@src=\'' + constants.refDataLocators.editorUrl + '\']')));
    }

    public async getSearchBar() {
        return await element(by.xpath('//input[@placeholder=\'Search\']'));
    }

    public async getContentTable() {
        return await element.all(
            by.xpath('//div[@class=\'control-dropdown-items\']/div/span')
        );
    }

    public async selectTodaysDate() {
        return await this.waitForElementToBeVisibleAndClick(await element(by.buttonText('Today')));
    }

    public async autoSavePopUp() {
        return await this.waitForElementToBePresent(element(by.xpath('//div[@class=\'modal-content\']')));
    }

    public async acceptAutoSave() {
        return await this.waitForElementToBeVisibleAndClick(await element(by.buttonText('Yes')));
    }

    public async getCommentaryEditorInput() {
        return await element(by.xpath('//div[@contenteditable=\'true\']'));
    }

    public async getReferencePlaceHolder(referenceName: string) {
        return await element(
            by.xpath(
                '//label[contains(text(), \'' + referenceName + '\')]/following-sibling::div/sqx-references-dropdown/sqx-dropdown/span/div/input[@class=\'form-control\']'
            )
        );
    }

    public async getEditorToolBarOptions(option: string) {
        return await element(by.className(option));
    }

    public async getCalender() {
        return await element(by.xpath('//div[@class=\'input-group\']/input'));
    }

    public async clickOnNewButton() {
        return await this.waitForElementToBeVisibleAndClick(await element(by.xpath('//button[@routerlink=\'new\']')));
    }

    public async saveContent() {
        return await this.waitForElementToBeVisibleAndClick(await element(by.xpath('//button[text() = \' Save \']')));
    }

    public async picktodaysDate() {
        return await element(by.buttonText('Today'));
    }

    public async captureContentValidationMessage() {
        return await this.waitForElementToBeVisibleAndGetText(await element(by.xpath('//div[@class=\'alert alert-dismissible alert-danger ng-trigger ng-trigger-fade ng-star-inserted\']/span')));
    }

    public async captureUnsavedChangesPopUpMessage() {
        return await this.waitForElementToBeVisibleAndGetText(element(by.xpath('//div[@class=\'modal-body \']')));
    }

    public async navigateToContentsTable() {
        return await this.waitForElementToBeVisibleAndClick(await element(by.xpath('//i[@class=\'icon-angle-left\']')));
    }

    public async createCommentaryWithoutSave(commentary: string) {
        await this.writeCommentary(commentary);
    }


    public async navigateToCommentaryAppPage() {
        const commentaryApp = element(by.cssContainingText('.card-title', 'commentary'));
        const content = element(by.cssContainingText('.nav-text', 'Content'));
        const commentarySchema = element(by.xpath('//li[1]//a[1]//span[1]'));
        await this.waitForElementToBeVisibleAndClick(commentaryApp);
        await this.waitForElementToBeVisibleAndClick(content);
        await this.waitForElementToBeVisibleAndClick(commentarySchema);
        await this.clickOnNewButton();
    }

    public async selectContentValue(content: string) {
        await this.waitForElementToBePresentAndWrite(await this.getSearchBar(), content);
        if (this.searchResult.isPresent() && (await this.searchResult.getText()).indexOf(content) !== -1) {
            await browser.actions().sendKeys(protractor.Key.ARROW_DOWN).perform();
        }
        await browser.actions().sendKeys(protractor.Key.ENTER).perform();
    }

    public async datePicker(addDays: number) {
        expect(await this.calendar);
        let today = new Date();
        let date = today.getDate() + addDays;
        let month = today.getMonth() + 1; // By default January counts as 0
        let year = today.getFullYear();
        return year + '-' + month + '-' + date;
    }

    public async selectDate(number: number) {
        await this.waitForElementToBeVisibleAndClick(this.calendar);
        await this.calendar.clear();
        await this.calendar.sendKeys(await this.datePicker(number));
    }

    public async selectContentFromDropDown(contentType: string, value: string) {
        await this.scrollIntoViewAndClick(await this.getReferencePlaceHolder(contentType));
        await this.selectContentValue(value);
    }

    public async getCommentary(contentEntryPlaceHolder: ElementFinder) {
        const editorFrame = await this.getCommentaryEditorFrame();
        try {
            await browser.switchTo().frame(editorFrame);
            return await contentEntryPlaceHolder.getText();
        } finally {
            await browser.switchTo().defaultContent();
            await browser.waitForAngular();
        }
    }

    public async writeCommentary(commentaryText: string) {
        const editorFrame = await this.getCommentaryEditorFrame();
        try {
            await browser.switchTo().frame(editorFrame);
            const editor = await this.getCommentaryEditorInput();
            await this.waitForElementToBePresentAndWrite(editor, commentaryText);
        } finally {
            await browser.switchTo().defaultContent();
            await browser.waitForAngular();
        }
    }

    public async randomValueSelection(number: number) {
        this.getRefData.count().then( (numberOfItems) => {
        return Math.floor(Math.random() * numberOfItems + number );
        }).then(async (randomNumber) => {
            await this.waitForElementToBeVisibleAndClick(this.getRefData.get(randomNumber));
        });
    }

    public async selectRandomReferences() {
        await this.selectDate(2);
        await this.scrollIntoViewAndClick(await this.getReferencePlaceHolder(constants.refDataLocators.commodity));
        await this.randomValueSelection(1);
        await this.scrollIntoViewAndClick(await this.getReferencePlaceHolder(constants.refDataLocators.commentaryType));
        await this.randomValueSelection(2);
        await this.scrollIntoViewAndClick(await this.getReferencePlaceHolder(constants.refDataLocators.region));
        await this.randomValueSelection(3);
    }

    public async createCommentary(commentary: string) {
        await this.writeCommentary(commentary);
        await this.saveContent();
    }

    public async commentaryEditorTest(commentary: string) {
        await this.selectRandomReferences();
        await this.writeCommentary(commentary);
        await this.selectAllContent();
    }

    public async createCommentaryAndApplyEditorOptions(commentary: string, editorToolBarOption: string) {
        const editorFrame = await this.scrollIntoView(await this.getCommentaryEditorFrame());
        await this.commentaryEditorTest(commentary);
        try {
            await browser.switchTo().frame(editorFrame);
            const editor = await this.getEditorToolBarOptions(editorToolBarOption);
            await this.waitForElementToBeVisibleAndClick(editor);
        } finally {
            await browser.switchTo().defaultContent();
            await browser.waitForAngular();
        }
        await this.saveContent();
    }
}