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

describe('VEGA-134 : Create UI for entering Regional Commentary', () => {
    let hasRunBefore = false;
    let appPage: AppPage;
    let browserPage: BrowserUtil;
    let contentPage: ContentPage;
    let homePage: HomePage;
    let loginPage: LoginPage;
    let searchPage: SearchPage;

    beforeAll(async () => {
        loginPage = new LoginPage();
        await loginPage.login(Users.find(u => u.name === 'vegaAdmin')!);
    });

    beforeEach(async () => {
        // initializing page objects before every test to see that no test is refering to stale elements.
        appPage = new AppPage();
        browserPage = new BrowserUtil();
        contentPage = new ContentPage();
        homePage = new HomePage();
        loginPage = new LoginPage();
        searchPage = new SearchPage();

        if (hasRunBefore) {
            // Go back to the home page and reset local store to get rid of all autosaved content.
            await homePage.navigateTo();
            await homePage.resetBrowserLocalStore();
        }

        // Then just reload to get the access token from the identity server.
        await homePage.navigateTo();

        await homePage.selectCommentaryApp('commentary');
        await searchPage.clickOnNewButton();
    });

    afterEach(async () => {
        await appPage.closeAlerts();

        hasRunBefore = true;
    });

    afterAll(async () => {
        await homePage.logout();
        // setting a timeout between logout and login of another spec for the test not to time out
        await browser.sleep(1000);
    });

    describe('VEGA-135 : Commentary creation UI with search & filter options for ref data', () => {
        it('Verify that user is allowed to search and select ref data from drop down for creating commentary (this test also covers commentary creation verification)', async () => {

            // Arrange
            await contentPage.selectDate(3);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.commentaryTest.commodityValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.commentaryTest.commentaryTypeValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.commentaryTest.regionValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.period, constants.commentaryTest.periodValue);

            // Act
            await contentPage.createCommentary(constants.commentaryTest.contentBody);

            // Assert
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

        it('Verify that user is allowed to search and select ref data with partial text from drop down for creating commentary (this test also covers commentary creation verification)', async () => {
            // Arrange
            await contentPage.selectDate(3);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.partialCommentaryTest.commodityValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.partialCommentaryTest.commentaryTypeValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.partialCommentaryTest.regionValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.period, constants.partialCommentaryTest.periodValue);

            // Act
            await contentPage.createCommentary(constants.partialCommentaryTest.contentBody);

            // Assert
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

        it('Verify user is able to edit the existing commentary', async () => {
            // Arrange
            await contentPage.selectDate(2);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.editCommentaryTest.commodityValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.editCommentaryTest.commentaryTypeValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.editCommentaryTest.regionValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.period, constants.editCommentaryTest.periodValue);
            await contentPage.createCommentary(constants.editCommentaryTest.contentBody);

            const alertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
            expect(alertMessage).toBe(constants.messages.commentaryCreationSuccessMessage);

            await appPage.closeAlerts();

            // Act
            await searchPage.selectContentByText(constants.editCommentaryTest.contentBody);

            await contentPage.selectDate(3);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.editCommentaryTest.modifiedCommodityValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.editCommentaryTest.modifiedCommentaryTypeValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.editCommentaryTest.modifiedRegionValue);
            await contentPage.createCommentary(constants.editCommentaryTest.modifiedContentBody);

            // Assert
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
    });

    describe('VEGA-333: Tag commentary blocks with the observed period', () => {
        it('Verify user is not allowed to create commentary if period field is marked \'required\' for a specific commentary type with no period value set', async () => {

            // Arrange
            await contentPage.selectDate(5);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.periodFieldInvalidValueTest.commodityValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.periodFieldInvalidValueTest.commentaryTypeValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.periodFieldInvalidValueTest.regionValue);
            await contentPage.writeCommentary(constants.periodFieldInvalidValueTest.contentBody);

            // Act
            await contentPage.saveContent();

            // Assert
            const alertMessage = await searchPage.getCommentaryCreationFailureMessageText();
            expect(alertMessage).toBe(constants.messages.observedPeriodNotSetErrorMessage);
        });

        it('Verify user is able to create commentary with period marked \' not required\' with no period value set', async () => {

            // Arrange
            await contentPage.selectDate(6);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.periodFieldSetAsNotRequiredTest.commodityValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.periodFieldSetAsNotRequiredTest.commentaryTypeValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.periodFieldSetAsNotRequiredTest.regionValue);
            await contentPage.writeCommentary(constants.periodFieldSetAsNotRequiredTest.contentBody);

            // Act
            await contentPage.saveContent();

            // Assert
            const alertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
            expect(alertMessage).toBeDefined();
        });

        it('Verify user is able to create commentary if period is marked \'required\' for a specific commentary type and period value set', async () => {

            // Arrange
            await contentPage.selectDate(5);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.periodFieldSetAsRequiredTest.commodityValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.periodFieldSetAsRequiredTest.commentaryTypeValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.periodFieldSetAsRequiredTest.regionValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.period, constants.periodFieldSetAsRequiredTest.periodValue);
            await contentPage.writeCommentary(constants.periodFieldSetAsRequiredTest.contentBody);

            // Act
            await contentPage.saveContent();

            // Assert
            const alertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
            expect(alertMessage).toBeDefined();
        });
    });

    describe('VEGA-243: Command validation middleware', () => {
        it('Verify user is not allowed to create duplicate commentaries with same ref data values for same date', async () => {

            // Arrange
            await contentPage.selectDate(3);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.duplicateCommentaryCreationTest.commodityValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.duplicateCommentaryCreationTest.commentaryTypeValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.duplicateCommentaryCreationTest.regionValue);

            // Act
            await contentPage.createCommentary(constants.duplicateCommentaryCreationTest.contentBody);

            await searchPage.clickOnNewButton();

            await contentPage.selectDate(3);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.duplicateCommentaryCreationTest.commodityValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.duplicateCommentaryCreationTest.commentaryTypeValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.duplicateCommentaryCreationTest.regionValue);
            await contentPage.createCommentary(constants.duplicateCommentaryCreationTest.contentBody);

            // Assert
            const message = contentPage.captureContentValidationMessage();
            expect<any>(message).toBe(constants.messages.validationFailureErrorMessage);
        });

        it('Verify user is not allowed to create commentary with invalid values', async () => {
            // Arrange
            await contentPage.selectDate(3);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.invalidRefDataTest.invalidRefDataValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.invalidRefDataTest.invalidRefDataValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.invalidRefDataTest.invalidRefDataValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.period, constants.invalidRefDataTest.invalidRefDataValue);

            // Act
            await contentPage.createCommentary(constants.commentaryTest.contentBody);

            // Assert
            const alertMessage = await searchPage.getCommentaryCreationFailureMessageText();
            expect(alertMessage).toBe(constants.messages.commentaryCreationFailureMessage);
        });

    });

    describe('VEGA-62: Autosave Commentary', () => {
        it('Verify commentary auto save pop-up functionality', async () => {

            // Arrange
            await contentPage.selectDate(4);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.commentaryTest.commodityValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.commentaryTest.commentaryTypeValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.commentaryTest.regionValue);
            await contentPage.writeCommentary(constants.commentaryTest.contentBody);

            // Act
            // The content is autosaved every 2seconds. Lets wait a little bit longer.
            await browser.sleep(4000);
            await browserPage.browserRefresh();

            // Assert
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

        it('Verify user is able to save the auto saved commentary by accepting auto-save pop-up', async () => {

            // Arrange
            await contentPage.selectDate(4);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.savingAutoSavedCommentaryTest.commodityValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.savingAutoSavedCommentaryTest.commentaryTypeValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.savingAutoSavedCommentaryTest.regionValue);
            await contentPage.writeCommentary(constants.savingAutoSavedCommentaryTest.contentBody);

            // Act
            // The content is autosaved every 2seconds. Lets wait a little bit longer.
            await browser.sleep(4000);
            await browserPage.browserRefresh();

            // Assert
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

        it('Verify that if a user quits without saving the auto saved commentary, saving changes pop-up warning appears', async () => {

            // Arrange
            await contentPage.selectDate(4);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.commentaryTest.commodityValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.commentaryTest.commentaryTypeValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.commentaryTest.regionValue);
            await contentPage.writeCommentary(constants.commentaryTest.contentBody);

            // Act
            // The content is autosaved every 2seconds. Lets wait a little bit longer.
            await browser.sleep(4000);
            await browserPage.browserRefresh();

            // Assert
            expect(contentPage.autoSavePopUp()).toBeTruthy();
            await contentPage.acceptAutoSave();

            await contentPage.navigateToContentsTable();

            const popUp = await contentPage.captureUnsavedChangesPopUpMessage();
            expect<any>(popUp).toContain(constants.messages.unsavedChangesPopUpMessage);
        });
    });
});
