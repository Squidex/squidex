import { browser, by, element } from 'protractor';

import { ContentPage } from './ContentPage.po';

export class SearchPage extends ContentPage {
    public $contentsItem() {
        return element(by.xpath('//sqx-content-value/span'));
    }

    public $getRefDataList(index: number) {
        return element.all(by.xpath('//table[@class=\'table table-items table-fixed\']/tbody//td[' + index + ']/sqx-content-value/span'));
    }

    public $contentItems() {
        return element.all(by.xpath('//sqx-content-value/span'));
    }

    public $boldText() {
        return element(by.xpath('//div[@class=\'tui-editor-contents\']/div/b'));
    }

    public $italicText() {
        return element(by.xpath('//div[@class=\'tui-editor-contents\']/div/i'));
    }

    public async $bulletPointText() {
        return await element(by.xpath('//div[@class=\'tui-editor-contents\']/ul/li'));
    }

    public async $numberedText() {
        return await element(by.xpath('//div[@class=\'tui-editor-contents\']/ol/li'));
    }

    public async getCommentaryCreationSuccessMessageText() {
        return await this.waitForElementToBeVisibleAndGetText(element(by.xpath('//div[contains(@class, \'alert\')]/span')));
    }

    public async getCommentaryCreationFailureMessageText() {
        return await this.waitForElementToBeVisibleAndGetText(element(by.xpath('//div[contains(@class, \'alert\')]/span')));
    }

    public async getRefDataSelection(referenceName: string) {
        return await this.waitForElementToBeVisibleAndGetText(element(by.xpath(`//label[contains(text(),\'${referenceName}\')]/following-sibling::div/sqx-references-dropdown/sqx-dropdown/span/div[@class=\'selection\']/div/span`)));
    }

    public async selectContentByText(contentBody: string) {
        await this.waitForElementToBePresent(this.$contentsItem());

        const contents = await this.$contentItems();

        expect(contents.length).toBeGreaterThan(0);

        for (let content of contents) {
            const text = await content.getText();

            if (text.indexOf(contentBody) >= 0) {
                await this.waitForElementToBeVisibleAndClick(content);
                return;
            }
        }

        throw `No Element with contentBody ${contentBody} found`;
    }

    public async verifyCommentaryCreation() {
        await this.scrollIntoView(this.getCommentaryEditorFrame());
        return await browser.wait(this.getCommentary(await this.$commentaryInput()), 5 * 1000, 'div should be visible in 5 seconds');
    }

    public async verifyBoldCommentaryCreation() {
        await this.selectContentByText('Bold');
        await this.scrollIntoView(this.getCommentaryEditorFrame());
        return await this.getCommentary(this.$boldText());
    }

    public async verifyItalicCommentaryCreation() {
        await this.selectContentByText('Italic');
        await this.scrollIntoView(this.getCommentaryEditorFrame());
        return await this.getCommentary(this.$italicText());
    }

    public async verifyNumberedCommentaryCreation() {
        await this.selectContentByText('Numbered');
        await this.scrollIntoView(await this.getCommentaryEditorFrame());
        return await this.getCommentary(await this.$numberedText());
    }

    public async verifyBulletPointsCommentaryCreation() {
        await this.selectContentByText('Bullet');
        await this.scrollIntoView(this.getCommentaryEditorFrame());
        return await this.getCommentary(await this.$bulletPointText());
    }

    public async searchContentByRefData(commodity: string, commentaryType: string, region: string) {
        await this.waitForElementToBePresent(await this.$contentsItem());

        const commodities = await this.$getRefDataList(5);
        const commentaryTypes = await this.$getRefDataList(6);
        const regions = await this.$getRefDataList(8);

        expect(commodities.length).toBeGreaterThan(0);
        expect(commentaryTypes.length).toBeGreaterThan(0);
        expect(regions.length).toBeGreaterThan(0);

        for (let commodityValue of commodities) {
            const text = await commodityValue.getText();

            if (text.includes(commodity)) {

                const commodityIndex = commodities.indexOf(commodityValue);

                const commentaryTypetext = await commentaryTypes[commodityIndex].getText();

                if (commentaryTypetext.includes(commentaryType)) {

                    const regiontext = await regions[commodityIndex].getText();

                    if (regiontext.includes(region)) {

                        await this.waitForElementToBeVisibleAndClick(regions[commodityIndex]);
                        return;
                    }
                }
            }

            throw `No Element with ${commodity}, ${commentaryType}, ${region} found`;
        }
    }

    public async clickOnNewButton() {
        return this.waitForElementToBeVisibleAndClick(this.$newButton());
    }
}