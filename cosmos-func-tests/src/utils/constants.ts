import { browser } from 'protractor';

export const constants = {

    loginTest: {
        editorWelcomeMessage: 'Hi Vega Test. Editor',
        reviewerWelcomeMessage: 'Hi Vega Test. Reviewer'
    },

    refDataLocators: {
        commodity: ' Commodity  ',
        commentaryType: ' Commentary Type  ',
        period: ' Period  ',
        region: ' Region  ',
        editorUrl: `${browser.params.baseUrl}/editors/toastui/md-editor.html`,
        editorOptionsBold: 'tui-bold tui-toolbar-icons',
        editorOptionsBulletPointList: 'tui-ul tui-toolbar-icons',
        editorOptionsItalic: 'tui-italic tui-toolbar-icons',
        editorOptionsNumberedList: 'tui-ol tui-toolbar-icons'
    },

    commentaryTest : {
        commodityValue: 'Styrene',
        // Temporary bug that fields are added to the UI even if they do not contain any value.
        commentaryTypeValue: 'Price Commentary, ,',
        regionValue: 'Middle East',
        contentBody: 'Commentary creation for test'
    },

    partialCommentaryTest: {
        commodityValue: 'Tolu',
        commentaryTypeValue: 'Charts',
        regionValue: 'Latin',
        periodValue: 'Settl',
        contentBody: 'This is atext',
        commodityValueFilteredByPartialText: 'Toluene',
        commentaryTypeValueFilteredByPartialText: 'Charts Commentary, 200, Yes',
        regionValueFilteredByPartialText: 'Latin America'
    },

    editCommentaryTest: {
        commodityValue: 'Styrene',
        commentaryTypeValue: 'Price Commentary',
        regionValue: 'Middle East',
        periodValue: 'Settlement',
        contentBody: 'This is commentary edit test',
        modifiedCommodityValue: 'Propylene',
        modifiedCommentaryTypeValue: 'Deals Commentary, 100, No',
        modifiedRegionValue: 'CIS/Central Asia',
        modifiedContentBody: 'Editing existing commentary'
    },

    duplicateCommentaryCreationTest: {
        commodityValue: 'Propylene',
        commentaryTypeValue: 'Analyst Commentary',
        regionValue: 'South East Asia & Pacific',
        periodValue: 'Settlement',
        contentBody: 'This is duplicate commentary creation test'
    },

    savingAutoSavedCommentaryTest: {
        commodityValue: 'Benzene',
        commentaryTypeValue: 'Deals Commentary, 100, No',
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

    labelAndDescVerificationTest: {
        labelValue: 'Commentaries',
        DescValue: 'This is the app to update Commentaries for Commodities based on regions',
        labelValueWithoutSave: 'Commentaries - Save',
        DescValueWithoutSave: 'Description Added',
        imagePath: '../../_images/website.jpg',
        imagesrc: `${browser.params.baseUrl}/api/apps/commentary/image`
    },

    messages: {
        validationFailureErrorMessage: 'A content item with these values already exists.',
        unsavedChangesPopUpMessage: 'You have unsaved changes, do you want to close the current content view and discard your changes?',
        commentaryCreationSuccessMessage: 'Contents created successfully.',
        commentaryEditSuccessMessage: 'Content updated successfully.',
        commentaryCretaionFailureMessage: 'Content element not valid, please check the field with the red bar on the left in all languages (if localizable).'
    }

};