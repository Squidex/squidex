import moment from 'moment';
import { browser, by, element, ElementFinder, protractor } from 'protractor';
import { constants } from './../utils/constants';

let casual = require('casual');

import { BrowserUtil } from './../utils';

export class ContentPage extends BrowserUtil {

    // Create Commentary after Navigating to Commentary Page under Content
    public searchResult = element.all(by.xpath('//span[@class=\'truncate ng-star-inserted\']/b'));

    public getRefData = element.all(by.xpath('//div[@class=\'control-dropdown-items\']/div/span'));

    public calendar() {
        return element(by.xpath('//input[contains(@class, \'form-date-only\')]'));
    }

    public getCommentaryEditorFrame() {
        return element(by.xpath(`//iframe[@src=\'${constants.refDataLocators.editorUrl}\']`));
    }

    public getSearchBar() {
        return element(by.xpath('//input[@placeholder=\'Search\']'));
    }

    public getContentTable() {
        return element.all(by.xpath('//div[@class=\'control-dropdown-items\']/div/span'));
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

    public $getFooter() {
        return element(by.id('footer'));
    }

    public async captureContentValidationMessage() {
        return await this.waitForElementToBeVisibleAndGetText(await element(by.xpath('//div[@class=\'alert alert-dismissible alert-danger ng-trigger ng-trigger-fade ng-star-inserted\']/span')));
    }

    public async captureUnsavedChangesPopUpMessage() {
        return await this.waitForElementToBeVisibleAndGetText(element(by.xpath('//div[@class=\'modal-body \']')));
    }

    public async navigateToContentsTable() {
        return await this.waitForElementToBeVisibleAndClick(element.all(by.css('.icon-close')).get(1));
    }

    public async createCommentaryWithoutSave(commentary: string) {
        await this.writeCommentary(commentary);
    }

    public async selectContentValue(content: string) {
        await browser.wait(this.waitForElementToBePresentAndWrite(await this.getSearchBar(), content), 5 * 1000, 'not able to search within 5 seconds');

        if (this.searchResult.isPresent() && (await this.searchResult.getText()).indexOf(content) !== -1) {
            await browser.actions().sendKeys(protractor.Key.ARROW_DOWN).perform();
        }

        await browser.actions().sendKeys(protractor.Key.ENTER).perform();
    }

    public getDateFromNow(days: number) {
        const formatedDate = moment().add(days, 'days').format('YYYY-MM-DD');

        return formatedDate;
    }

    public async selectDate(days: number) {
        await this.waitForElementToBePresentAndWrite(this.calendar(), this.getDateFromNow(days));
        await this.mouseMoveAndClick(this.$dateLabel());
    }

    public async selectContentFromDropDown(contentType: string, value: string) {
        await this.scrollIntoViewAndClick(this.getReferencePlaceHolder(contentType));

        await this.selectContentValue(value);
    }

    public async getCommentary(contentEntryPlaceHolder: ElementFinder) {
        return await this.forToastUI(async () => {
            return await browser.wait(this.waitForElementToBePresentAndGetText(contentEntryPlaceHolder), 5 * 1000, 'element not visible');
        });
    }

    public async getCommentaryFooter() {
        return await this.forToastUI(async () => {
            const footer = await this.$getFooter();

            return await this.scrollIntoViewAndGetTextAndInvokeBrowser(footer);
        });
    }

    public async writeCommentary(commentaryText: string) {
        return await this.forToastUI(async () => {
            const editor = await this.$commentaryInput();

            await this.waitForElementToBePresentAndWrite(editor, commentaryText);
        });
    }

    public async writeCommentaryForToastUiTests(commentaryText: string, selectAll = false) {
        return await this.forToastUI(async () => {
            const editor = await this.$commentaryInput();

            await this.waitForElementToBePresentAndAppend(editor, commentaryText);

            if (selectAll) {
                this.selectAllContent();
            }
        });
    }

    public async appendCommentary(commentaryText: string) {
        return await this.forToastUI(async () => {
            const editor = await this.$commentaryInput();

            await this.waitForElementToBePresentAndAppendText(editor, commentaryText);
        });
    }

    public async clickToastUIButton(editorToolBarOption: string) {
        return await this.forToastUI(async () => {
            const button = this.getEditorToolBarOptions(editorToolBarOption);

            await browser.wait(this.browserScriptToClick(button), 5 * 1000, 'not able to search within 5 seconds');
        });
    }

    public async randomValueSelection() {
        const size = this.getRefData.length;
        const refData = await this.getRefData;
        const validRefData = refData.slice(1, size);

        const item = casual.random_element(validRefData);
        await this.scrollIntoViewAndClick(item);
    }

    public async selectRandomReferences(date: number) {
        await this.selectDate(date);

        await this.waitForElementToBeVisibleAndClick(await this.getReferencePlaceHolder(constants.refDataLocators.commodity));
        await this.randomValueSelection();

        await this.waitForElementToBeVisibleAndClick(await this.getReferencePlaceHolder(constants.refDataLocators.commentaryType));
        await this.randomValueSelection();

        await this.waitForElementToBeVisibleAndClick(await this.getReferencePlaceHolder(constants.refDataLocators.region));
        await this.randomValueSelection();

        await this.waitForElementToBeVisibleAndClick(await this.getReferencePlaceHolder(constants.refDataLocators.period));
        await this.randomValueSelection();
    }

    public async createCommentary(commentary: string) {
        await this.writeCommentary(commentary);

        await this.saveContent();
    }

    public async createCommentaryAndApplyEditorOptions(commentary: string, editorToolBarOption: string, date: number) {
        await this.selectRandomReferences(date);

        await this.writeCommentaryForToastUiTests(commentary, true);

        await browser.wait(this.clickToastUIButton(editorToolBarOption), 5 * 1000, 'option should be applied within 5 seconds');

        await this.saveContent();
    }

    private async forToastUI<T>(action: () => Promise<T>) {
        let result: T;

        const editorFrame = await this.getCommentaryEditorFrame().getWebElement();
        try {
            await browser.switchTo().frame(editorFrame);

            result = await action();
        } finally {
            await browser.switchTo().defaultContent();
            await browser.waitForAngular();
        }

        return result;
    }
}