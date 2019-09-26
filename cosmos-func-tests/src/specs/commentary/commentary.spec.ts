import { browser } from 'protractor';
import {
    BrowserUtil,
    constants,
    Users
} from './../../utils';

import {
    AppPage,
    ContentPage,
    HomePage,
    LoginPage,
    SearchPage
} from './../../pages';

describe('Create Commentary', () => {
    let hasRunBefore = false;

    const appPage = new AppPage();
    const browserPage = new BrowserUtil();
    const contentPage = new ContentPage();
    const homePage = new HomePage();
    const loginPage = new LoginPage();
    const searchPage = new SearchPage();

    beforeAll(async () => {
        await loginPage.login(Users.find(u => u.name === 'vegaEditor')!);
    });

    beforeEach(async () => {
        if (hasRunBefore) {
            // Go back to the home page and reset local store to get rid of all autosaved content.
            await homePage.navigateTo();
            await homePage.resetBrowserLocalStore();
        }

        // Then just reload to get the access token from the identity server.
        await homePage.navigateTo();

        await homePage.selectCommentaryApp('Commentaries');
        await appPage.selectContentMenuItem();
        await appPage.selectCommentarySchema();
        await searchPage.clickOnNewButton();
    });

    afterEach(async () => {
        await appPage.closeAlerts();

        hasRunBefore = true;
    });

    afterAll(async () => {
        await homePage.logout();
    });

    it('should allow the user to search and filter ref data with text and bring the matching results', async () => {
        await contentPage.selectDate(3);
        await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.commentaryTest.commodityValue);
        await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.commentaryTest.commentaryTypeValue);
        await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.commentaryTest.regionValue);
        await contentPage.createCommentary(constants.commentaryTest.contentBody);

        const alertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
        expect(alertMessage).toBe(constants.messages.commentaryCreationSuccessMessage);

        await searchPage.selectContentByText(constants.commentaryTest.contentBody);

        const commodityValue = await searchPage.getRefDataSelection(constants.refDataLocators.commodity);
        expect(commodityValue).toBe(constants.commentaryTest.commodityValue);

        const commentaryTypeValue = await searchPage.getRefDataSelection(constants.refDataLocators.commentaryType);
        expect(commentaryTypeValue).toBe(constants.commentaryTest.commentaryTypeValue);

        const regionValue = await searchPage.getRefDataSelection(constants.refDataLocators.region);
        expect(regionValue).toBe(constants.commentaryTest.regionValue);

        const commentaryText = await searchPage.verifyCommentaryCreation();
        expect(commentaryText).toBe(constants.commentaryTest.contentBody);
    });

    it('should allow the user to search and filter ref data with partial text and bring the matching results', async () => {
        await contentPage.selectDate(3);
        await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.partialCommentaryTest.commodityValue);
        await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.partialCommentaryTest.commentaryTypeValue);
        await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.partialCommentaryTest.regionValue);
        await contentPage.selectContentFromDropDown(constants.refDataLocators.period, constants.partialCommentaryTest.periodValue);
        await contentPage.createCommentary(constants.partialCommentaryTest.contentBody);

        const alertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
        expect(alertMessage).toBe(constants.messages.commentaryCreationSuccessMessage);

        await searchPage.selectContentByText(constants.partialCommentaryTest.contentBody);

        const commodityValue = await searchPage.getRefDataSelection(constants.refDataLocators.commodity);
        expect(commodityValue).toBe(constants.partialCommentaryTest.commodityValueFilteredByPartialText);

        const commentaryTypeValue = await searchPage.getRefDataSelection(constants.refDataLocators.commentaryType);
        expect(commentaryTypeValue).toBe(constants.partialCommentaryTest.commentaryTypeValueFilteredByPartialText);

        const regionValue = await searchPage.getRefDataSelection(constants.refDataLocators.region);
        expect(regionValue).toBe(constants.partialCommentaryTest.regionValueFilteredByPartialText);

        const commentaryText = await searchPage.verifyCommentaryCreation();
        expect(commentaryText).toBe(constants.partialCommentaryTest.contentBody);
    });

    it('should allow the user to edit the existing commentary and verify values', async () => {
        await contentPage.selectDate(2);
        await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.editCommentaryTest.commodityValue);
        await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.editCommentaryTest.commentaryTypeValue);
        await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.editCommentaryTest.regionValue);
        await contentPage.createCommentary(constants.editCommentaryTest.contentBody);

        const alertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
        expect(alertMessage).toBe(constants.messages.commentaryCreationSuccessMessage);

        await appPage.closeAlerts();

        await searchPage.selectContentByText(constants.editCommentaryTest.contentBody);

        await contentPage.selectDate(3);
        await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.editCommentaryTest.modifiedCommodityValue);
        await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.editCommentaryTest.modifiedCommentaryTypeValue);
        await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.editCommentaryTest.modifiedRegionValue);
        await contentPage.createCommentary(constants.editCommentaryTest.modifiedContentBody);

        const editAlertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
        expect(editAlertMessage).toBe(constants.messages.commentaryEditSuccessMessage);

        await contentPage.navigateToContentsTable();
        await searchPage.selectContentByText(constants.editCommentaryTest.modifiedContentBody);

        const commodityValue = await searchPage.getRefDataSelection(constants.refDataLocators.commodity);
        expect(commodityValue).toBe(constants.editCommentaryTest.modifiedCommodityValue);

        const commentaryTypeValue = await searchPage.getRefDataSelection(constants.refDataLocators.commentaryType);
        expect(commentaryTypeValue).toBe(constants.editCommentaryTest.modifiedCommentaryTypeValue);

        const regionValue = await searchPage.getRefDataSelection(constants.refDataLocators.region);
        expect(regionValue).toBe(constants.editCommentaryTest.modifiedRegionValue);

        const commentaryText = await searchPage.verifyCommentaryCreation();
        expect(commentaryText).toBe(constants.editCommentaryTest.modifiedContentBody);
    });

    describe('VEGA-333: Tag commentary blocks with the observed period', () => {
        it('should not allow to create commentary if period is required by commentary type', async () => {
            const testValues = {
                body: 'ShortText',
                commentaryTypeWithRequiredPeriod: 'Charts Commentary, 200, Yes',
                commodity: 'Propylene',
                region: 'South East Asia & Pacific'
            };

            // Arrange
            await contentPage.selectDate(5);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, testValues.commodity);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, testValues.commentaryTypeWithRequiredPeriod);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, testValues.region);
            await contentPage.writeCommentary(testValues.body);

            // Act
            await contentPage.saveContent();

            // Assert
            const alertMessage = await searchPage.getCommentaryCreationFailureMessageText();
            expect(alertMessage).toBe('Failed to save commentary: Period is required.');
        });

        it('should allow to create commentary if period is not required and no period set.', async () => {
            const testValues = {
                body: 'ShortText',
                commentaryTypeWithoutRequiredPeriod: 'Overview',
                commodity: 'Propylene',
                region: 'South East Asia & Pacific'
            };

            // Arrange
            await contentPage.selectDate(6);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, testValues.commodity);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, testValues.commentaryTypeWithoutRequiredPeriod);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, testValues.region);
            await contentPage.writeCommentary(testValues.body);

            // Act
            await contentPage.saveContent();

            // Assert
            const alertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
            expect(alertMessage).toBeDefined();
        });

        it('should allow to create commentary if period is required by commentary type and period is set', async () => {
            const testValues = {
                body: 'ShortText',
                commentaryTypeWithRequiredPeriod: 'Charts Commentary, 200, Yes',
                commodity: 'Propylene',
                period: 'Settlement',
                region: 'South East Asia & Pacific'
            };

            // Arrange
            await contentPage.selectDate(7);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, testValues.commodity);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, testValues.commentaryTypeWithRequiredPeriod);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, testValues.region);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.period, testValues.period);
            await contentPage.writeCommentary(testValues.body);

            // Act
            await contentPage.saveContent();

            // Assert
            const alertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
            expect(alertMessage).toBeDefined();
        });
    });

    it('should throw error for duplicate commentaries with same ref data', async () => {
        await contentPage.selectDate(3);
        await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.duplicateCommentaryCreationTest.commodityValue);
        await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.duplicateCommentaryCreationTest.commentaryTypeValue);
        await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.duplicateCommentaryCreationTest.regionValue);
        await contentPage.createCommentary(constants.duplicateCommentaryCreationTest.contentBody);

        await searchPage.clickOnNewButton();

        await contentPage.selectDate(3);
        await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.duplicateCommentaryCreationTest.commodityValue);
        await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.duplicateCommentaryCreationTest.commentaryTypeValue);
        await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.duplicateCommentaryCreationTest.regionValue);
        await contentPage.createCommentary(constants.duplicateCommentaryCreationTest.contentBody);

        const message = contentPage.captureContentValidationMessage();
        expect<any>(message).toBe(constants.messages.validationFailureErrorMessage);
    });

    describe('VEGA-62: Autosaving', () => {
        it('should auto save commentary', async () => {
            await contentPage.selectDate(4);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.commentaryTest.commodityValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.commentaryTest.commentaryTypeValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.commentaryTest.regionValue);
            await contentPage.writeCommentary(constants.commentaryTest.contentBody);

            // The content is autosaved every 2seconds. Lets wait a little bit longer.
            await browser.sleep(5000);
            await browserPage.browserRefresh();

            expect(await contentPage.autoSavePopUp()).not.toBeNull();
            await contentPage.acceptAutoSave();

            const commodityValue = await searchPage.getRefDataSelection(constants.refDataLocators.commodity);
            expect(commodityValue).toBe(constants.commentaryTest.commodityValue);

            const commentaryTypeValue = await searchPage.getRefDataSelection(constants.refDataLocators.commentaryType);
            expect(commentaryTypeValue).toBe(constants.commentaryTest.commentaryTypeValue);

            const regionValue = await searchPage.getRefDataSelection(constants.refDataLocators.region);
            expect(regionValue).toBe(constants.commentaryTest.regionValue);

            const commentaryText = await searchPage.verifyCommentaryCreation();
            expect(commentaryText).toBe(constants.commentaryTest.contentBody);
        });

        it('should save the auto saved commentary', async () => {
            await contentPage.selectDate(4);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.savingAutoSavedCommentaryTest.commodityValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.savingAutoSavedCommentaryTest.commentaryTypeValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.savingAutoSavedCommentaryTest.regionValue);
            await contentPage.writeCommentary(constants.savingAutoSavedCommentaryTest.contentBody);

            // The content is autosaved every 2seconds. Lets wait a little bit longer.
            await browser.sleep(5000);
            await browserPage.browserRefresh();

            expect(await contentPage.autoSavePopUp()).not.toBeNull();
            await contentPage.acceptAutoSave();

            const commodityValue = await searchPage.getRefDataSelection(constants.refDataLocators.commodity);
            expect(commodityValue).toBe(constants.savingAutoSavedCommentaryTest.commodityValue);

            const commentaryTypeValue = await searchPage.getRefDataSelection(constants.refDataLocators.commentaryType);
            expect(commentaryTypeValue).toBe(constants.savingAutoSavedCommentaryTest.commentaryTypeValue);

            const regionValue = await searchPage.getRefDataSelection(constants.refDataLocators.region);
            expect(regionValue).toBe(constants.savingAutoSavedCommentaryTest.regionValue);

            const commentaryText = await searchPage.verifyCommentaryCreation();
            expect(commentaryText).toBe(constants.savingAutoSavedCommentaryTest.contentBody);

            await contentPage.saveContent();
            await searchPage.selectContentByText(constants.savingAutoSavedCommentaryTest.contentBody);

            const commodityAfterSave = await searchPage.getRefDataSelection(constants.refDataLocators.commodity);
            expect(commodityAfterSave).toBe(constants.savingAutoSavedCommentaryTest.commodityValue);

            const commentaryTypeAfterSave = await searchPage.getRefDataSelection(constants.refDataLocators.commentaryType);
            expect(commentaryTypeAfterSave).toBe(constants.savingAutoSavedCommentaryTest.commentaryTypeValue);

            const regionValueAfterSave = await searchPage.getRefDataSelection(constants.refDataLocators.region);
            expect(regionValueAfterSave).toBe(constants.savingAutoSavedCommentaryTest.regionValue);

            const commentaryAfterSave = await searchPage.verifyCommentaryCreation();
            expect(commentaryAfterSave).toBe(constants.savingAutoSavedCommentaryTest.contentBody);
        });

        it('quit without saving the auto saved commentary and capture the pop-up', async () => {
            await contentPage.selectDate(4);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.commentaryTest.commodityValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.commentaryTest.commentaryTypeValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.commentaryTest.regionValue);
            await contentPage.writeCommentary(constants.commentaryTest.contentBody);

            // The content is autosaved every 2seconds. Lets wait a little bit longer.
            await browser.sleep(5000);
            await browserPage.browserRefresh();

            expect(contentPage.autoSavePopUp()).toBeTruthy();
            await contentPage.acceptAutoSave();

            await contentPage.navigateToContentsTable();

            const popUp = await contentPage.captureUnsavedChangesPopUpMessage();
            expect<any>(popUp).toContain(constants.messages.unsavedChangesPopUpMessage);
        });
    });

    it('should throw error for invalid ref data', async () => {
        await contentPage.selectDate(3);
        await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.invalidRefDataTest.invalidRefDataValue);
        await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.invalidRefDataTest.invalidRefDataValue);
        await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.invalidRefDataTest.invalidRefDataValue);
        await contentPage.selectContentFromDropDown(constants.refDataLocators.period, constants.invalidRefDataTest.invalidRefDataValue);
        await contentPage.createCommentary(constants.commentaryTest.contentBody);

        const alertMessage = await searchPage.getCommentaryCreationFailureMessageText();
        expect(alertMessage).toBe(constants.messages.commentaryCretaionFailureMessage);
    });

    it('should support Bold text', async () => {
        await contentPage.createCommentaryAndApplyEditorOptions(constants.tuiEditorOptionsTest.boldCommentaryContentBody, constants.refDataLocators.editorOptionsBold);
        const commentaryText = await searchPage.verifyBoldCommentaryCreation();
        expect(commentaryText).toBe(constants.tuiEditorOptionsTest.boldCommentaryContentBody);
    });

    it('should support Italic text', async () => {
        await contentPage.createCommentaryAndApplyEditorOptions(constants.tuiEditorOptionsTest.italicCommentaryContentBody, constants.refDataLocators.editorOptionsItalic);
        const commentaryText = await searchPage.verifyItalicCommentaryCreation();
        expect(commentaryText).toBe(constants.tuiEditorOptionsTest.italicCommentaryContentBody);
    });

    it('should support Numbered list', async () => {
        await contentPage.createCommentaryAndApplyEditorOptions(constants.tuiEditorOptionsTest.numberedListContentBody, constants.refDataLocators.editorOptionsNumberedList);
        const commentaryText = await searchPage.verifyNumberedCommentaryCreation();
        expect(commentaryText).toBe(constants.tuiEditorOptionsTest.numberedListContentBody);
    });

    it('should support Bulleted list', async () => {
        await contentPage.createCommentaryAndApplyEditorOptions(constants.tuiEditorOptionsTest.bulletPointsContentBody, constants.refDataLocators.editorOptionsBulletPointList);
        const commentaryText = await searchPage.verifyBulletPointsCommentaryCreation();
        expect(commentaryText).toBe(constants.tuiEditorOptionsTest.bulletPointsContentBody);
    });
});
