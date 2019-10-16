import { browser } from 'protractor';

export const constants = {

    loginTest: {
        editorWelcomeMessage: 'Hi Vega Test. Editor',
        reviewerWelcomeMessage: 'Hi Vega Test. Reviewer',
        adminWelcomeMessage: 'Hi Vega Test. Admin'
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

    commentaryRedirectTest: {
        commentaryUrl: `${browser.params.baseUrl}/app/commentary/content/commentary`
    },

    commentaryTest: {
        commodityValue: 'Styrene',
        // Temporary bug that fields are added to the UI even if they do not contain any value.
        commentaryTypeValue: 'Price Commentary',
        regionValue: 'Middle East',
        periodValue: 'Settlement',
        contentBody: 'Commentary creation for test'
    },

    partialCommentaryTest: {
        commodityValue: 'Tolu',
        commentaryTypeValue: 'Deals',
        regionValue: 'Latin',
        periodValue: 'Settl',
        contentBody: 'This is a text',
        commodityValueFilteredByPartialText: 'Toluene',
        commentaryTypeValueFilteredByPartialText: 'Deals Commentary',
        regionValueFilteredByPartialText: 'Latin America'
    },

    editCommentaryTest: {
        commodityValue: 'Styrene',
        commentaryTypeValue: 'Price Commentary',
        regionValue: 'Middle East',
        periodValue: 'Settlement',
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
        periodValue: 'Settlement',
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
        numberedListContentBody: 'Numbered List Test.'
    },

    characterCountWithNoLimitTest: {
        commodityValue: 'Benzene',
        commentaryTypeValue: 'Price Commentary',
        regionValue: 'Middle East',
        periodValue: 'Settlement'
    },

    characterCountWithWordLimitTest: {
        commentaryTypeValue: 'Deals Commentary',
        commodityValue: 'Propylene',
        regionValue: 'South East Asia & Pacific'
    },

    characterCountFailureTest: {
        commodityValue: 'Toluene',
        commentaryTypeValue: 'Overview',
        regionValue: 'South East Asia & Pacific'
    },

    characterCountSuccessForEditComTest: {
        commentaryTypeValue: 'Deals Commentary',
        commodityValue: 'Polyols',
        regionValue: 'Europe'
    },

    characterCountFailureForEditComTest: {
        commentaryTypeValue: 'Overview',
        commodityValue: 'Polyols',
        periodValue: 'Settlement',
        regionValue: 'Africa'
    },

    periodFieldSetAsNotRequiredTest: {
        contentBody: 'ShortText',
        commentaryTypeValue: 'Overview',
        commodityValue: 'Propylene',
        regionValue: 'South East Asia & Pacific'
    },

    periodFieldInvalidValueTest: {
        contentBody: 'ShortText',
        commentaryTypeValue: 'Charts Commentary',
        commodityValue: 'Propylene',
        regionValue: 'South East Asia & Pacific'
    },

    periodFieldSetAsRequiredTest: {
        contentBody: 'ShortText',
        commentaryTypeValue: 'Charts Commentary',
        commodityValue: 'Propylene',
        periodValue: 'Settlement',
        regionValue: 'South East Asia & Pacific'
    },

    labelAndDescVerificationTest: {
        originalLabelValue: 'commentary',
        labelValue: 'Commentaries',
        DescValue: 'This is the app to update Commentaries for Commodities based on regions',
        labelValueWithoutSave: 'Commentaries - Save',
        DescValueWithoutSave: 'Description Added',
        imagePath: '../../_images/website.jpg',
        imagesrc: `${browser.params.baseUrl}/api/apps/commentary/image`
    },

    contributorsTest: {
        addContributor: 'hello@world.com',
        role: 'CMS Managing Editor',
        multipleContributors: ['test2@dummy.com;', 'test3@dummy.com;', 'test4@dummy.com;', 'test5@dummy.com;', 'test6@dummy.com;', 'test7@dummy.com;', 'test8@dummy.com;'],
        contributorsOnListScreen: ['test2@dummy.com', 'test3@dummy.com', 'test4@dummy.com', 'test5@dummy.com', 'test6@dummy.com', 'test7@dummy.com', 'test8@dummy.com'],
        deleteContributor: 'vegatestreviewer@cha.rbxd.ds',
        editRole: 'Reader',
        editContributor: 'vegatesteditor@cha.rbxd.ds',
        importRole: 'Reader',
        importingSameUser: 'test2@dummy.com'
    },

    messages: {
        validationFailureErrorMessage: 'A content item with these values already exists.',
        unsavedChangesPopUpMessage: 'You have unsaved changes, do you want to close the current content view and discard your changes?',
        commentaryCreationSuccessMessage: 'Content created successfully.',
        commentaryEditSuccessMessage: 'Content updated successfully.',
        commentaryCreationFailureMessage: 'Content element not valid, please check the field with the red bar on the left in all languages (if localizable).',
        contributorSuccessMessage: 'User has been added as contributor.',
        changeLogAssignmentMessage: 'Me assigned dummy@cha.rbxd.ds as Editor',
        changeLogDeletionMessage: 'Me removed vegatestreviewer@cha.rbxd.ds from app',
        chnageLogRoleEditMessage: 'Me assigned vegatesteditor@cha.rbxd.ds as Owner',
        contributorAdditionFailureMessage: 'Cannot assign contributor: Role: Contributor has already this role.',
        characterCountLimitErrorMessage: 'Failed to save commentary: Exceeded character limit of \'800\' characters for language \'en\'.',
        observedPeriodNotSetErrorMessage: 'Failed to save commentary: Period is required.'
    }

};