import {
    constants,
    Users
} from '../../utils';

import {
    AppPage,
    ContentPage,
    HomePage,
    LoginPage,
    SearchPage
} from '../../pages';
let randomWords = require('random-words');


describe('VEGA-30 : ToastUI Editor Tests', () => {
    let hasRunBefore = false;
    const using = require('jasmine-data-provider');
    const appPage = new AppPage();
    const contentPage = new ContentPage();
    const homePage = new HomePage();
    const loginPage = new LoginPage();
    const searchPage = new SearchPage();

    beforeAll(async () => {
        await loginPage.login(Users.find(u => u.name === 'vegaAdmin')!);
    });

    beforeEach(async () => {
        if (hasRunBefore) {
            // Go back to the home page and reset local store to get rid of all autosaved content.
            await homePage.navigateTo();
            await homePage.resetBrowserLocalStore();
        }

        // Then just reload to get the access token from the identity server.
        await homePage.navigateTo();
        await homePage.selectCommentaryApp('commentary');
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

    describe('VEGA-30: Editor Options for creating commentary', () => {
        it('Verify that commentary editor supports Bold text', async () => {
            await contentPage.createCommentaryAndApplyEditorOptions(constants.tuiEditorOptionsTest.boldCommentaryContentBody, constants.refDataLocators.editorOptionsBold, 2);
            const commentaryText = await searchPage.verifyBoldCommentaryCreation();
            expect(commentaryText).toBe(constants.tuiEditorOptionsTest.boldCommentaryContentBody);
        });

        it('Verify that commentary editor supports Italic text', async () => {
            await contentPage.createCommentaryAndApplyEditorOptions(constants.tuiEditorOptionsTest.italicCommentaryContentBody, constants.refDataLocators.editorOptionsItalic, 3);
            const commentaryText = await searchPage.verifyItalicCommentaryCreation();
            expect(commentaryText).toBe(constants.tuiEditorOptionsTest.italicCommentaryContentBody);
        });

        it('Verify that commentary editor supports Numbered list', async () => {
            await contentPage.createCommentaryAndApplyEditorOptions(constants.tuiEditorOptionsTest.numberedListContentBody, constants.refDataLocators.editorOptionsNumberedList, 4);
            const commentaryText = await searchPage.verifyNumberedCommentaryCreation();
            expect(commentaryText).toBe(constants.tuiEditorOptionsTest.numberedListContentBody);
        });

        it('Verify that commentary editor supports Bulleted list', async () => {
            await contentPage.createCommentaryAndApplyEditorOptions(constants.tuiEditorOptionsTest.bulletPointsContentBody, constants.refDataLocators.editorOptionsBulletPointList, 5);
            const commentaryText = await searchPage.verifyBulletPointsCommentaryCreation();
            expect(commentaryText).toBe(constants.tuiEditorOptionsTest.bulletPointsContentBody);
        });
    });

    describe('VEGA-31 & VEGA-322 : Character Count Validation - Frontend and Backend', () => {
        using([{ commodityValue: 'Propylene', commentaryTypeValue: 'Overview', regionValue: 'North America', periodValue: 'Settlement' },
            { commodityValue: 'Toluene', commentaryTypeValue: 'Outlook', regionValue: 'Europe', periodValue: 'Settlement' }], (data: any) => {
        it('Verify user is able to create Outlook and Overview commentaries with input upto 100 words', async () => {
            const phrase = randomWords({exactly: 100, join: ' '});
            const input = phrase.substr(0, 800);

            // Arrange
            await contentPage.selectDate(2);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, data.commodityValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, data.commentaryTypeValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, data.regionValue);
            await contentPage.writeCommentary(input);

            // Act
            await contentPage.saveContent();

            // Assert
            const alertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
            expect(alertMessage).toBe(constants.messages.commentaryCreationSuccessMessage);

            await appPage.closeAlerts();

            await searchPage.searchContentByRefData(data.commodityValue, data.commentaryTypeValue, data.regionValue);
            const charCountOnTextEditor = await searchPage.verifyCommentaryCreation();
            const countOnEditor = charCountOnTextEditor.length.toString();
            const getFooterText = await contentPage.getCommentaryFooter();
            const num = getFooterText.replace(/^\D+/g, '');
            const getCountFromFooterText = num.substring(0, 3);

            expect(countOnEditor).toBe(getCountFromFooterText);
        });
        });

        it('Verify user is not allowed to create commentary if the input exceeds specified character count', async () => {
            const phrase = randomWords({exactly: 150, join: ' '});
            const input = phrase.substr(0, 850);

            // Arrange
            await contentPage.selectDate(4);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.characterCountFailureTest.commodityValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.characterCountFailureTest.commentaryTypeValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.characterCountFailureTest.regionValue);
            await contentPage.writeCommentary(input);

            // Act
            await contentPage.saveContent();

            // Assert
            const alertMessage = await searchPage.getCommentaryCreationFailureMessageText();
            expect(alertMessage).toBe(constants.messages.characterCountLimitErrorMessage);

            const charCountOnTextEditor = await searchPage.verifyCommentaryCreation();
            const countOnEditor = charCountOnTextEditor.length;
            const getFooterText = await contentPage.getCommentaryFooter();
            const num = getFooterText.replace(/^\D+/g, '');
            const getCountFromFooterText = Number(num.substring(0, 3));

            expect(countOnEditor).toBe(getCountFromFooterText);
        });

        it('Verify user is able to create commentaries other than Overview and Outlook with input upto 250 words', async () => {
            const phrase = randomWords({exactly: 250, join: ' '});
            const input = phrase.substr(0, 1800);

            // Arrange
            await contentPage.selectDate(5);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.characterCountWithWordLimitTest.commodityValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.characterCountWithWordLimitTest.commentaryTypeValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.characterCountWithWordLimitTest.regionValue);
            await contentPage.writeCommentary(input);

            // Act
            await contentPage.saveContent();

            // Assert
            const alertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
            expect(alertMessage).toBe(constants.messages.commentaryCreationSuccessMessage);

            await appPage.closeAlerts();

            await searchPage.searchContentByRefData(constants.characterCountWithWordLimitTest.commodityValue, constants.characterCountWithWordLimitTest.commentaryTypeValue, constants.characterCountWithWordLimitTest.regionValue);
            const charCountOnTextEditor = await searchPage.verifyCommentaryCreation();
            const countOnEditor = charCountOnTextEditor.length.toString();
            const getFooterText = await contentPage.getCommentaryFooter();
            const num = getFooterText.replace(/^\D+/g, '');
            const getCountFromFooterText = num.substring(0, 4);

            expect(countOnEditor).toBe(getCountFromFooterText);
        });

        it('Verify user is able to create commentary when character count limit is not set', async () => {
            const phrase = randomWords({exactly: 500, join: ' '});

            // Arrange
            await contentPage.selectDate(3);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.characterCountWithNoLimitTest.commodityValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.characterCountWithNoLimitTest.commentaryTypeValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.characterCountWithNoLimitTest.regionValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.period, constants.characterCountWithNoLimitTest.periodValue);
            await contentPage.writeCommentary(phrase);

            // Act
            await contentPage.saveContent();

            // Assert
            const alertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
            expect(alertMessage).toBe(constants.messages.commentaryCreationSuccessMessage);

            await appPage.closeAlerts();

            await searchPage.searchContentByRefData(constants.characterCountWithNoLimitTest.commodityValue, constants.characterCountWithNoLimitTest.commentaryTypeValue, constants.characterCountWithNoLimitTest.regionValue);
            const charCountOnTextEditor = await searchPage.verifyCommentaryCreation();
            const countOnEditor = charCountOnTextEditor.length.toString();
            const getFooterText = await contentPage.getCommentaryFooter();
            const num = getFooterText.replace(/^\D+/g, '');

            expect(countOnEditor).toBe(num);
        });

        it('Verify user is not allowed to exceed the word limit while editing existing commentary', async () => {
            const phrase = randomWords({exactly: 150, join: ' '});
            const input = phrase.substr(0, 750);
            const moreText = randomWords({exactly: 15, join: ' '});

            // Arrange
            await contentPage.selectDate(1);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.characterCountFailureForEditComTest.commodityValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.characterCountFailureForEditComTest.commentaryTypeValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.characterCountFailureForEditComTest.regionValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.period, constants.characterCountFailureForEditComTest.periodValue);
            await contentPage.writeCommentary(input);

            await contentPage.saveContent();

            // Act
            const alertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
            expect(alertMessage).toBe(constants.messages.commentaryCreationSuccessMessage);

            await appPage.closeAlerts();

            await searchPage.searchContentByRefData(constants.characterCountFailureForEditComTest.commodityValue, constants.characterCountFailureForEditComTest.commentaryTypeValue, constants.characterCountFailureForEditComTest.regionValue);
            await contentPage.appendCommentary(' ' + moreText);
            await contentPage.saveContent();

            // Assert
            const alert = await searchPage.getCommentaryCreationFailureMessageText();
            expect(alert).toBe(constants.messages.characterCountLimitErrorMessage);

            const charCountOnTextEditor = await searchPage.verifyCommentaryCreation();
            const countOnEditor = charCountOnTextEditor.length;
            const getFooterText = await contentPage.getCommentaryFooter();
            const num = getFooterText.replace(/^\D+/g, '');
            const getCountFromFooterText = Number(num.substring(0, 3));

            expect(countOnEditor).toBe(getCountFromFooterText);
        });

        it('Verify when user edits and updates the commentary content word count is updated accordingly', async () => {
            const phrase = randomWords({exactly: 150, join: ' '});
            const input = phrase.substr(0, 1500);
            const moreText = randomWords({exactly: 10, join: ' '});

            // Arrange
            await contentPage.selectDate(3);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commodity, constants.characterCountSuccessForEditComTest.commodityValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.commentaryType, constants.characterCountSuccessForEditComTest.commentaryTypeValue);
            await contentPage.selectContentFromDropDown(constants.refDataLocators.region, constants.characterCountSuccessForEditComTest.regionValue);
            await contentPage.writeCommentary(input);

            await contentPage.saveContent();

            // Act
            const alertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
            expect(alertMessage).toBe(constants.messages.commentaryCreationSuccessMessage);

            await appPage.closeAlerts();

            await searchPage.searchContentByRefData(constants.characterCountSuccessForEditComTest.commodityValue, constants.characterCountSuccessForEditComTest.commentaryTypeValue, constants.characterCountSuccessForEditComTest.regionValue);
            await contentPage.appendCommentary(' ' + moreText);

            await contentPage.saveContent();

            // Assert
            const editAlertMessage = await searchPage.getCommentaryCreationSuccessMessageText();
            expect(editAlertMessage).toBe(constants.messages.commentaryEditSuccessMessage);
            const charCountOnTextEditor = await searchPage.verifyCommentaryCreation();
            const countOnEditor = charCountOnTextEditor.length.toString();
            const getFooterText = await contentPage.getCommentaryFooter();
            const num = getFooterText.replace(/^\D+/g, '');
            const getCountFromFooterText = num.substring(0, 4);

            expect(countOnEditor).toBe(getCountFromFooterText);
        });
    });
});