import {
    constants,
    Users
} from '../../utils';

import {
    AppPage,
    ContributorsPage,
    HomePage,
    LoginPage
} from '../../pages';

describe('VEGA-323 : Contributors Functionality', () => {
    let hasRunBefore = false;
    let appPage: AppPage;
    let contributorsPage: ContributorsPage;
    let homePage: HomePage;
    let loginPage: LoginPage;


    beforeAll(async () => {

        loginPage = new LoginPage();
        await loginPage.login(Users.find(u => u.name === 'vegaAdmin')!);
    });

    beforeEach(async () => {

        // initializing page object classes before every test so they don't refer to stale elements
        appPage = new AppPage();
        contributorsPage = new ContributorsPage();
        homePage = new HomePage();

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

    describe('VEGA-343 : Extend the UI with a dropdown to select the role for new contributors', () => {
        it('Add a contributor & assign a role. Verify their name is listed on Contributors list and change log is updated accordingly', async () => {

            // Arrange
            await contributorsPage.navigateToContributorsTab();

            // Act
            await contributorsPage.addContributor(constants.contributorsTest.addContributor, constants.contributorsTest.role);

            // Assert
            expect(await contributorsPage.verifyContributorAlertMessage()).toBe(constants.messages.contributorSuccessMessage);
            expect(await contributorsPage.verifyContributorAddition(constants.contributorsTest.addContributor)).toBe(true);

        });

        it('Delete a contributor. Verify their name is removed from the contributors list and change log is updated accordingly', async () => {

            // Arrange
            await contributorsPage.navigateToContributorsTab();

            // Act
            await contributorsPage.deleteContributor(constants.contributorsTest.deleteContributor);

            // Assert
            expect(await contributorsPage.verifyContributorDeletion(constants.contributorsTest.deleteContributor)).toBe(true);
        });


        it('Verify adding the same user twice and capture the error message', async () => {

            // Arrange
            await contributorsPage.navigateToContributorsTab();

            // Act
            await contributorsPage.addContributor(constants.contributorsTest.addContributor, constants.contributorsTest.role);

            // Assert
            expect(await contributorsPage.verifyContributorAlertMessage()).toBe(constants.messages.contributorAdditionFailureMessage);

        });

        it('Edit the role of a existing contributor and verify the change is reflected on contributors list and change log is updated accordingly', async () => {

            // Arrange
            await contributorsPage.navigateToContributorsTab();

            // Act
            await contributorsPage.editContributorRoleAssignment(constants.contributorsTest.editContributor, constants.contributorsTest.editRole);

            // Assert
            expect(await contributorsPage.verifyContributorRoleEdit(constants.contributorsTest.editContributor, constants.contributorsTest.editRole)).toBe(true);

        });

        it('Verify pagination is not displayed when there are less than 10 contributors', async () => {

            // Act
            await contributorsPage.navigateToContributorsTab();

            // Assert
            expect(await contributorsPage.$pagination()).toBe(true);
        });

    });

    describe('VEGA-344 : Import Bulk Contributors list UI changes', () => {
        it('Add multiple contributors at once with different roles and verify they are added successfully', async () => {

            // Arrange
            await contributorsPage.navigateToContributorsTab();

            // Act
            await contributorsPage.bulkImportOfContributors(constants.contributorsTest.multipleContributors, constants.contributorsTest.importRole);

            // Assert
            expect(await contributorsPage.verifyTickMarkAfterSuccessFullUserAddition()).toBe(true);
            expect(await contributorsPage.verifyBulkImport(constants.contributorsTest.contributorsOnListScreen)).toBe(true);

        });

        it('Verify importing the same user with same role twice and capture the error message', async () => {

            // Arrange
            await contributorsPage.navigateToContributorsTab();

            // Act
            await contributorsPage.importingContributorWithSameRole(constants.contributorsTest.importingSameUser, constants.contributorsTest.importRole);

            // Assert
            expect(await contributorsPage.verifyErrorIconDisplay()).toBe(true);

        });

        it('Verify pagination appears only if there are more than 10 contributors', async () => {

            // Act
            await contributorsPage.navigateToContributorsTab();

            // Assert
            const paginationDisplay = await contributorsPage.pagination();
            expect(await paginationDisplay.isDisplayed()).toBe(true);

        });
    });

});