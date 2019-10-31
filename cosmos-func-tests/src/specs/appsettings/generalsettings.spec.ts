import {
    constants,
    Users
} from '../../utils';

import {
    AppPage,
    GeneralSettingsPage,
    HomePage,
    LoginPage
} from '../../pages';

describe('VEGA-316: App Settings', () => {
    let hasRunBefore = false;
    let appPage: AppPage;
    let loginPage: LoginPage;
    let homePage: HomePage;
    let generalSettingsPage: GeneralSettingsPage;

    beforeAll(async () => {
        loginPage = new LoginPage();
        await loginPage.login(Users.find(u => u.name === 'vegaAdmin')!);
    });

    beforeEach(async () => {
        // initializing page object classes before every test so they don't refer to stale elements
        appPage = new AppPage();
        homePage = new HomePage();
        loginPage = new LoginPage();
        generalSettingsPage = new GeneralSettingsPage();

        if (hasRunBefore) {
            // Go back to the home page and reset local store to get rid of all autosaved content.
            await homePage.navigateTo();
            await homePage.resetBrowserLocalStore();
        }

        await homePage.navigateTo();
        await homePage.selectCommentaryApp('commentary');
        await appPage.selectSettingsMenuItem();
    });

    afterEach(async () => {
        hasRunBefore = true;
    });

    afterAll(async () => {
        await homePage.logout();
    });

    describe('VEGA-316 : Changes to Landing Page and General Settings', () => {
        it('Change the label and description of the app and verify its updated on home page', async () => {
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

            // Reset the app name back to 'commentary' to not block other tests
            await homePage.selectCommentaryApp('Commentaries');
            await appPage.selectSettingsMenuItem();
            await generalSettingsPage.updateLabel(constants.labelAndDescVerificationTest.originalLabelValue);
            await generalSettingsPage.saveContent();

        });

        it('Change the label and description of the app and quit without saving verify its not updated on home page', async () => {
            // Arrange
            await generalSettingsPage.updateLabel(constants.labelAndDescVerificationTest.labelValue);
            await generalSettingsPage.updateDescription(constants.labelAndDescVerificationTest.DescValue);

            // Act
            await generalSettingsPage.navigateToAppHomePage();

            // Assert
            const getLabel = await homePage.getAppNameAfterChange();
            expect(getLabel).toBe(constants.labelAndDescVerificationTest.originalLabelValue);
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