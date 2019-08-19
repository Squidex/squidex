import { ElementFinder } from 'protractor';
import {
    browser,
    by,
    element,
    protractor
} from 'protractor';

import { BrowserUtil } from '../utils/Browser.util';


const searchResult = element.all(by.xpath('//span[@class=\'truncate ng-star-inserted\']/b'));

export class ContentPage extends BrowserUtil {

    // Create Commentary after Navigating to Commentary Page under Content
    public async getSearchBar() {
        return await element(by.xpath('//input[@placeholder=\'Search\']'));
    }

    public async getContentTable() {
        return await element.all(
            by.xpath('//div[@class=\'control-dropdown-items\']/div/span')
        );
    }

    public async selectRefData() {
        return await element.all(
            by.css('.control-dropdown-item control-dropdown-item-selectable ng-star-inserted')
        );
    }

    public async getCommentaryEditor() {
        return await element(by.xpath('//iframe[@src=\'https://localhost:5000/editors/toastui/md-editor.html\']')).getWebElement();
    }

    public async getCommentaryEditorInput() {
        return await element(by.xpath('//div[@contenteditable=\'true\']'));
    }

    public async submitSelection() {
        return await element(by.buttonText('Link selected contents (1)')).click();
    }

    public async referencePlaceHolder(referenceName: string) {
        return await element(
            by.xpath(
                '//label[contains(text(), \'' + referenceName + '\')]/following-sibling::div/sqx-references-dropdown/sqx-dropdown/span/div/input[@class=\'form-control\']'
            )
        );
    }

    public async applyEditorToolBarOptions(option: string) {
        return await element(by.className(option));
    }

    public async navigateToContentsPage() {
        return await element(by.xpath('//h3[contains(text(),\'Content\')]'));
    }

    public async newButton() {
        return await element(by.className('btn btn-success'));
    }

    public async saveContent() {
        return await element(by.xpath('//button[text() = \' Save \']')).click();
    }

    public async navigateToCommentaryAppPage() {
        const commentaryApp = element(by.cssContainingText('.card-title', 'commentary'));
        const content = element(by.cssContainingText('.nav-text', 'Content'));
        const commentarySchema = element(by.xpath('//li[1]//a[1]//span[1]'));
        await this.waitForElementToBeVisibleAndClick(commentaryApp);
        await this.waitForElementToBeVisibleAndClick(content);
        await this.waitForElementToBeVisibleAndClick(commentarySchema);
        await this.waitForElementToBeVisibleAndClick(await this.newButton());
    }

    public async selectRandomReferences() {
        await this.scrollIntoViewAndClick(await this.referencePlaceHolder(' Commodity  '));
        await this.randomSelection();
        await this.scrollIntoViewAndClick(await this.referencePlaceHolder(' Commentary Type  '));
        await this.randomSelection();
        await this.scrollIntoViewAndClick(await this.referencePlaceHolder(' Region  '));
        await this.randomSelection();
    }

    public async selectContent(content: string) {
        await this.waitForElementToBePresentAndWrite(await this.getSearchBar(), content);
        if (searchResult.isPresent() && (await searchResult.getText()).indexOf(content) !== -1) {
            await browser.actions().sendKeys(protractor.Key.ARROW_DOWN).perform();
        }
        await browser.actions().sendKeys(protractor.Key.ENTER).perform();
    }

    public async randomSelection() {
        this.selectRefData().then(selection => {
            const randomItem = selection[Math.floor(Math.random() * selection.length)];
            randomItem.getWebElement().click();
            return;
        });
    }

    public async selectCommodity(commodity: string) {
        await this.scrollIntoViewAndClick(await this.referencePlaceHolder(' Commodity  '));
        await this.selectContent(commodity);
    }

    public async selectCommentaryType(commentaryType: string) {
        await this.scrollIntoViewAndClick(await this.referencePlaceHolder(' Commentary Type  '));
        await this.selectContent(commentaryType);
    }

    public async selectRegion(region: string) {
        await this.scrollIntoViewAndClick(await this.referencePlaceHolder(' Region  '));
        await this.selectContent(region);
    }

    public async getCommentary(contentEntryPlaceHolder: ElementFinder) {
        await browser.sleep(5000);
        const editorFrame = await this.getCommentaryEditor();
        try {
            await browser.switchTo().frame(editorFrame);
            return await contentEntryPlaceHolder.getText();
        } finally {
            await browser.switchTo().defaultContent();
            await browser.waitForAngular();
        }
    }

    public async writeCommentary(commentaryText: string) {
        const editorFrame = await this.getCommentaryEditor();
        try {
            await browser.switchTo().frame(editorFrame);
            const editor = await this.getCommentaryEditorInput();
            await this.waitForElementToBePresentAndWrite(editor, commentaryText);
        } finally {
            await browser.switchTo().defaultContent();
            await browser.waitForAngular();
        }
    }

    public async createCommentary(commentary: string) {
        await this.writeCommentary(commentary);
        await browser.sleep(5000);
        await this.saveContent();
    }

    public async commentaryEditorTest(commentary: string) {
        await this.navigateToContentsPage();
        await this.waitForElementToBeVisibleAndClick(await this.newButton());
        await this.selectRandomReferences();
        await this.writeCommentary(commentary);
        await this.selectAllContent();
    }
    public async createCommentaryWithBoldLetters(commentary: string) {
        await this.commentaryEditorTest(commentary);
        await this.waitForElementToBeVisibleAndClick(await this.applyEditorToolBarOptions('tui-bold tui-toolbar-icons'));
        await this.saveContent();
    }

    public async createCommentaryWithItalicFont(commentary: string) {
        await this.commentaryEditorTest(commentary);
        await this.waitForElementToBeVisibleAndClick(await this.applyEditorToolBarOptions('tui-italic tui-toolbar-icons'));
        await this.saveContent();
    }

    public async createBulletPointsCommentary(commentary: string) {
        await this.commentaryEditorTest(commentary);
        await this.waitForElementToBeVisibleAndClick(await this.applyEditorToolBarOptions('tui-ul tui-toolbar-icons'));
        await this.saveContent();
    }

    public async createNumberedCommentary(commentary: string) {
        await this.commentaryEditorTest(commentary);
        await this.waitForElementToBeVisibleAndClick(await this.applyEditorToolBarOptions('tui-ol tui-toolbar-icons'));
        await this.saveContent();
    }
}