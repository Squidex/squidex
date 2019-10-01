import {
    constants,
    Users
} from './../../utils';

import {
    AppPage,
    GeneralSettingsPage,
    HomePage,
    LoginPage
} from './../../pages';


describe('VEGA-323 : App Settings', () => {
    let hasRunBefore = false;

    const appPage = new AppPage();
    const homePage = new HomePage();
    const loginPage = new LoginPage();
    const generalSettingsPage = new GeneralSettingsPage();

    beforeAll(async () => {
        await loginPage.login(Users.find(u => u.name === 'vegaAdmin')!);
    });

    beforeEach(async () => {
        if (hasRunBefore) {
            // Go back to the home page and reset local store to get rid of all autosaved content.
            await homePage.navigateTo();
            await homePage.resetBrowserLocalStore();

            await homePage.navigateTo();
            await homePage.selectCommentaryApp('Commentaries');

        } else {

            await homePage.navigateTo();
            await homePage.selectCommentaryApp('commentary');
        }

        await appPage.selectSettingsMenuItems();
    });

    afterEach(async () => {
        await appPage.closeAlerts();

        hasRunBefore = true;
    });

    afterAll(async () => {
        await homePage.logout();
    });

    describe('VEGA-316 : Changes to Landing Page and General Settings', () => {
    xit('Change the label and description of the app and verify its updated on home page', async () => {

        // Arrange
        await generalSettingsPage.updateLabel(constants.labelAndDescVerificationTest.labelValue);
        await generalSettingsPage.updateDescription(constants.labelAndDescVerificationTest.DescValue);

        // Act
        await generalSettingsPage.saveContent();
        await generalSettingsPage.navigateToAppHomePage();

        // Assert
        const getLabel = await homePage.getAppNameAfterChange();
        expect(getLabel).toBe(constants.labelAndDescVerificationTest.labelValue);
        const getDescription = await homePage.getDescription();
        expect(getDescription).toBe(constants.labelAndDescVerificationTest.DescValue);

    });

    xit('Change the label and description of the app and quit without saving verify its not updated on home page', async () => {

        // Arrange
        await generalSettingsPage.updateLabel(constants.labelAndDescVerificationTest.labelValue);
        await generalSettingsPage.updateDescription(constants.labelAndDescVerificationTest.DescValue);

        // Act
        await generalSettingsPage.navigateToAppHomePage();

        // Assert
        const getLabel = await homePage.getAppNameAfterChange();
        expect(getLabel).toBe(constants.labelAndDescVerificationTest.labelValue);
        const getDescription = await homePage.getDescription();
        expect(getDescription).toBe(constants.labelAndDescVerificationTest.DescValue);

    });

    it('Upload Image for the app and verify its updated and visible on home page', async () => {

        // Arrange
        await generalSettingsPage.uploadImage(constants.labelAndDescVerificationTest.imagePath);

        // Act
        await generalSettingsPage.navigateToAppHomePage();

        // Assert
        const src = await generalSettingsPage.imgSrc();
        expect<any>(src.getAttribute('src')).toBe(constants.labelAndDescVerificationTest.imagesrc);

    });

    });

});