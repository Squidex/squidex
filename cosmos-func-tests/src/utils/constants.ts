import { browser } from 'protractor';

export const constants = {

    loginTest: {
        editorWelcomeMessage: 'Hi Vega Test. Editor',
        reviewerWelcomeMessage: 'Hi Vega Test. Reviewer'
    },

    refDataLocators: {
        commodity: ' Commodity  ',
        commentaryType: ' Commentary Type  ',
        region: ' Region  ',
        editorUrl: `${browser.params.baseUrl}/editors/toastui/md-editor.html`,
        editorOptionsBold: 'tui-bold tui-toolbar-icons',
        editorOptionsBulletPointList: 'tui-ul tui-toolbar-icons',
        editorOptionsItalic: 'tui-italic tui-toolbar-icons',
        editorOptionsNumberedList: 'tui-ol tui-toolbar-icons'
    },

    commentaryTest : {
        commodityValue: 'Styrene',
        commentaryTypeValue: 'Price Commentary',
        regionValue: 'Middle East',
        contentBody: 'Commentary creation for test'
    },

    partialCommentaryTest: {
        commodityValue: 'Tolu',
        commentaryTypeValue: 'Charts',
        regionValue: 'Latin',
        contentBody: 'This is search & filter test for ref data with partial text',
        commodityValueFilteredByPartialText: 'Toluene',
        commentaryTypeValueFilteredByPartialText: 'Charts Commentary',
        regionValueFilteredByPartialText: 'Latin America'
    },

    editCommentaryTest: {
        commodityValue: 'Styrene',
        commentaryTypeValue: 'Price Commentary',
        regionValue: 'Middle East',
        contentBody: 'This is commentary edit test',
        modifiedCommodityValue: 'Propylene',
        modifiedCommentaryTypeValue: 'Deals Commentary',
        modifiedRegionValue: 'CIS/Central Asia',
        modifiedContentBody: 'Editing existing commentary'
    },

    duplicateCommentaryCreationTest: {
        commodityValue: 'Propylene',
        commentaryTypeValue: 'Analyst Commentary',
        regionValue: 'South East Asia & Pacific',
        contentBody: 'This is duplicate commentary creation test'
    },

    savingAutoSavedCommentaryTest: {
        commodityValue: 'Benzene',
        commentaryTypeValue: 'Deals Commentary',
        regionValue: 'Europe',
        contentBody: 'This is content creation test'
    },

    invalidRefDataTest: {
        invalidRefDataValue: 'invalid'
    },

    tuiEditorOptionsTest: {
        boldCommentaryContentBody: 'Bold Letters Commentary Test',
        bulletPointsContentBody: 'Bullet Point Commentary Test',
        italicCommentaryContentBody: 'Italic Commentary Test',
        numberedListContentBody: 'Numbered List Test'
    },

    messages: {
        validationFailureErrorMessage: 'A content item with these values already exists.',
        unsavedChangesPopUpMessage: ' You have unsaved changes, do you want to close the current content view and discard your changes? ',
        commentaryCreationSuccessMessage: 'Contents created successfully.',
        commentaryEditSuccessMessage: 'Content updated successfully.',
        commentaryCretaionFailureMessage: 'Content element not valid, please check the field with the red bar on the left in all languages (if localizable).'
    }

};