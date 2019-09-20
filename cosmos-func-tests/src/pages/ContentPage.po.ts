import { browser, by, element, ElementFinder, protractor } from 'protractor';
import { constants } from './../utils/constants';


import { BrowserUtil } from './../utils';

export class ContentPage extends BrowserUtil {
    // Create Commentary after Navigating to Commentary Page under Content
    public searchResult = element.all(by.xpath('//span[@class=\'truncate ng-star-inserted\']/b'));

    public calendar = element(by.xpath('//div[@class=\'input-group\']/input'));

    public getRefData = element.all(by.xpath('//div[@class=\'control-dropdown-items\']/div/span'));

    public getCommentaryEditorFrame() {
        return element(by.xpath(`//iframe[@src=\'${constants.refDataLocators.editorUrl}\']`));
    }

    public commentaryEditor() {
        return this.getWhenVisible(element(by.xpath(`//iframe[@src=\'${constants.refDataLocators.editorUrl}\']`)));
    }

    public getSearchBar() {
        return element(by.xpath('//input[@placeholder=\'Search\']'));
    }

    public getContentTable() {
        return element.all(by.xpath('//div[@class=\'control-dropdown-items\']/div/span'));
    }

    public async selectTodaysDate() {
        return await this.waitForElementToBeVisibleAndClick(await element(by.buttonText('Today')));
    }

    public async autoSavePopUp() {
        return await this.getWhenVisible(element(by.xpath('//div[@class=\'modal-content\']')));
    }

    public async acceptAutoSave() {
        return await this.waitForElementToBeVisibleAndClick(await element(by.buttonText('Yes')));
    }

    public $commentaryInput() {
        return element(by.xpath('//div[@contenteditable=\'true\']'));
    }

    public getReferencePlaceHolder(referenceName: string) {
        return element(by.xpath(`//label[contains(text(), \'${referenceName}\')]/following-sibling::div/sqx-references-dropdown/sqx-dropdown/span/div/input[@class=\'form-control\']`));
    }

    public getEditorToolBarOptions(option: string) {
        return element(by.className(option));
    }

    public async getCalender() {
        return await element(by.xpath('//div[@class=\'input-group\']/input'));
    }

    public async saveContent() {
        await this.waitForElementToBeVisibleAndClick(await element(by.xpath('//button[text() = \' Save \']')));

        // Just wait a little bit for the animation to finish.
        await browser.sleep(1000);
    }

    public $newButton() {
        return element(by.xpath('//button[@routerlink=\'new\']'));
    }

    public $dateLabel() {
        return element(by.xpath('//label[contains(text(), \' Created For Date  \')]'));
    }

    public $dateTodayButton() {
        return element(by.buttonText('Today'));
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

    public async selectContentValue(content: string) {
        await this.waitForElementToBePresentAndWrite(await this.getSearchBar(), content);
        if (this.searchResult.isPresent() && (await this.searchResult.getText()).indexOf(content) !== -1) {
            await browser.actions().sendKeys(protractor.Key.ARROW_DOWN).perform();
        }
        await browser.actions().sendKeys(protractor.Key.ENTER).perform();
    }

    public getDateFromNow(addDays: number) {
        const today = new Date();

        return `${today.getFullYear()}-${today.getMonth() + 1}-${today.getDate() + addDays}`;
    }

    public async selectDate(number: number) {
        await this.waitForElementToBeVisibleAndClick(this.calendar);
        await this.calendar.clear();
        await this.calendar.sendKeys(this.getDateFromNow(number));

        this.$dateLabel().click();
    }

    public async selectContentFromDropDown(contentType: string, value: string) {
        await this.scrollIntoViewAndClick(this.getReferencePlaceHolder(contentType));

        await this.selectContentValue(value);
    }

    public async getCommentary(contentEntryPlaceHolder: ElementFinder) {
        const editorFrame = await this.getCommentaryEditorFrame().getWebElement();
        try {
            await browser.switchTo().frame(editorFrame);
            return await contentEntryPlaceHolder.getText();
        } finally {
            await browser.switchTo().defaultContent();
            await browser.waitForAngular();
        }
    }

    public async writeCommentary(commentaryText: string) {
        const editorFrame = await this.getCommentaryEditorFrame().getWebElement();
        try {
            await browser.switchTo().frame(editorFrame);
            const editor = await this.$commentaryInput();
            await this.waitForElementToBePresentAndWrite(editor, commentaryText);
        } finally {
            await browser.switchTo().defaultContent();
            await browser.waitForAngular();
        }
    }

    public async randomValueSelection(number: number) {
        const itemsCount = await this.getRefData.count();
        const itemIndex = Math.min(5, Math.floor(Math.random() * itemsCount + number));

        const selected = await this.getRefData.get(itemIndex);

        await this.scrollIntoViewAndClick(selected);
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
        await this.scrollIntoView(this.getCommentaryEditorFrame());

        const editorFrame = await this.getCommentaryEditorFrame().getWebElement();
        await this.commentaryEditorTest(commentary);
        try {
            await browser.switchTo().frame(editorFrame);
            const button = this.getEditorToolBarOptions(editorToolBarOption);
            await this.waitForElementToBeVisibleAndClick(button);
        } finally {
            await browser.switchTo().defaultContent();
            await browser.waitForAngular();
        }
        await this.saveContent();
    }
}