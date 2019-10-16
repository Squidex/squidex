import { by, element } from 'protractor';

import { BrowserUtil } from '../utils';

export class ContributorsPage extends BrowserUtil {

    public $contributorsTab() {
        return element(by.cssContainingText('.nav-link', ' Contributors '));
    }

    public $contributorInput() {
        return element(by.xpath('//span/input'));
    }

    public $bulkImport() {
        return element(by.linkText('Add many contributors at once'));
    }

    public $bulkImportInputArea() {
        return element(by.tagName('textarea'));
    }

    public $addContributorButton() {
        return element(by.buttonText('Add Contributor'));
    }

    public $addContributorsButton() {
        return element(by.buttonText('Add Contributors'));
    }

    public $importContributorsButton() {
        return element(by.buttonText('Import'));
    }

    public $getRoleDropDown() {
        return element.all(by.xpath('//div[@class=\'col truncate aligned\']/following-sibling::div/select'));
    }

    public $selectDropDown() {
        return element(by.xpath('//div[@class=\'col\']/following-sibling::div/select'));
    }

    public $selectUserRole(text: string) {
        return element(by.xpath(`//div[@class=\'col\']/following-sibling::div/select/option[contains(text(),\'${text}\')]`));
    }

    public $userNameList() {
        return element.all(by.xpath('//span[@class=\'user-name table-cell\']'));
    }

    public $deleteUser() {
        return element.all(by.css('.icon-bin2'));
    }

    public $historyTab() {
        return element(by.css('.icon-time'));
    }

    public $iconCheckMark() {
        return element.all(by.xpath('//i[@class=\'icon-checkmark\']'));
    }

    public $iconError() {
        return element.all(by.css('.icon-exclamation'));
    }

    public $closeIcon() {
        return element(by.css('.icon-close'));
    }

    public $rolesList() {
        return element.all(by.xpath('//td[@class=\'cell-time\']/select'));
    }

    public async pagination() {
        await this.scrollToEndOfPage();
        const pageCount = await this.mouseMoveAndReturn(element(by.xpath('//span[contains(@class, \'pagination-text\')]')));
        if (pageCount) {
            return pageCount;
        }
    }

    public async $pagination() {
        return await this.waitForElementToBeNotVisible(element(by.xpath('//span[contains(@class, \'pagination-text\')]')));
    }

    public async verifyContributorAlertMessage() {
        return await this.waitForElementToBeVisibleAndGetText(element(by.xpath('//div[contains(@class, \'alert\')]/span')));
    }

    public async changeLogMessage() {
        return await this.waitForElementToBeVisibleAndGetText(element(by.css('.event-message')).element(by.tagName('span')));
    }

    public async navigateToContributorsTab() {
        await this.waitForElementToBeVisibleAndClick(this.$contributorsTab());
    }

    public async addContributor(user: string, role: string) {
        await this.waitForElementToBePresentAndWrite(this.$contributorInput(), user);
        await this.waitForElementToBeVisibleAndClick(this.$selectDropDown());
        await this.waitForElementToBeVisibleAndClick(this.$selectUserRole(role));
        await this.waitForElementToBeVisibleAndClick(this.$addContributorButton());
    }

    public async verifyContributorAddition(userName: string): Promise<boolean> {
        const users = await this.$userNameList();
        expect(users.length).toBeGreaterThan(0);

        for (let user of users) {
            const text = await user.getText();
            if (text.includes(userName)) {
                return true;
            }
        }
        throw `No Contributor with ${userName} found`;
    }

    public async deleteContributor(userName: string) {
        const users = await this.$userNameList();
        const deleteButton = await this.$deleteUser();
        expect(users.length).toBeGreaterThan(0);
        for (let user of users) {
            const text = await user.getText();
            if (text.includes(userName)) {
                const index = users.indexOf(user);
                await this.mouseMoveAndClick(deleteButton[index]);
                return;

            }
        }
        throw `No Contributor with ${userName} found`;
    }

    public async verifyContributorDeletion(userName: string) {
        const users = await this.$userNameList();
        expect(users.length).toBeGreaterThan(0);

        for (let user of users) {
            const text = await user.getText();
            if (text !== userName) {
                return true;
            }
        }
        throw `User with ${userName} not deleted`;
    }

    public async verifyChangeLogMessage() {
        await this.waitForElementToBeVisibleAndClick(this.$historyTab());
        return await this.changeLogMessage();
    }

    public async editContributorRoleAssignment(userName: string, role: string) {
        const users = await this.$userNameList();
        const roles = await this.$rolesList();
        expect(users.length).toBeGreaterThan(0);

        for (let user of users) {
            const text = await user.getText();
            if (text.includes(userName)) {
                const index = users.indexOf(user);
                const roleList = await roles[index];
                await this.waitForElementToBePresentAndAppend(roleList, role);
                return;
            }
        }
        throw `No Contributor with ${userName} found`;
    }

    public async verifyContributorRoleEdit(userName: string, role: string) {
        const users = await this.$userNameList();
        const roles = await this.$rolesList();
        expect(users.length).toBeGreaterThan(0);

        for (let user of users) {
            const text = await user.getText();
            if (text.includes(userName)) {
                const index = users.indexOf(user);
                const roleList = await roles[index];
                const roleName = await roleList.getText();
                if (roleName.includes(role)) {
                    return true;
                }
            }
        }
        throw `No Contributor with ${userName} found`;
    }

    public async bulkImportOfContributors(contributors: Array<string>, role: string) {
        await this.waitForElementToBeVisibleAndClick(this.$bulkImport());
        await this.waitForElementToBeVisibleAndClick(this.$bulkImportInputArea());
        for (const user of contributors) {
            await this.waitForElementToBePresentAndAppend(this.$bulkImportInputArea(), user);
        }
        await this.waitForElementToBeVisibleAndClick(await this.$addContributorsButton());
        const selectRole = await this.$getRoleDropDown();

        for (let select of selectRole) {
            await this.waitForElementToBeVisibleAndClick(select);
            await this.waitForElementToBePresentAndAppend(select, role);
            await this.waitForElementToBeVisibleAndClick(select);
            break;
        }
        await this.waitForElementToBeVisibleAndClick(await this.$importContributorsButton());
    }

    public async verifyTickMarkAfterSuccessFullUserAddition(): Promise<boolean> {
        const userAdditionCheckMarkList = await this.$iconCheckMark();
        for (let userAdditionTick of userAdditionCheckMarkList) {
            if (userAdditionTick.isPresent() === true) {
                return true;
            }
        }
    }

    public async verifyBulkImport(contributors: Array<string>): Promise<boolean> {
        await this.waitForElementToBeVisibleAndClick(await this.$closeIcon());

        const users = await this.$userNameList().getText();
        expect(users.length).toBeGreaterThan(0);

        if (expect(users).toEqual(jasmine.arrayContaining(contributors))) {
            return true;
        }

    }

    public async importingContributorWithSameRole(contributor: string, role: string) {
        await this.waitForElementToBeVisibleAndClick(this.$bulkImport());
        await this.waitForElementToBeVisibleAndClick(this.$bulkImportInputArea());
        await this.waitForElementToBePresentAndAppend(this.$bulkImportInputArea(), contributor);
        await this.waitForElementToBeVisibleAndClick(await this.$addContributorsButton());
        const selectRole = await this.$getRoleDropDown();

        for (let select of selectRole) {
            await this.waitForElementToBeVisibleAndClick(select);
            await this.waitForElementToBePresentAndAppend(select, role);
            await this.waitForElementToBeVisibleAndClick(select);
            break;
        }
        await this.waitForElementToBeVisibleAndClick(await this.$importContributorsButton());
    }

    public async verifyErrorIconDisplay(): Promise<boolean> {
        const userAdditionError = await this.$iconError();
        for (let errorIcon of userAdditionError) {
            if (errorIcon.isDisplayed()) {
                return true;
            }
        }
    }

    public async verifyPaginationForLessThanTenUsers(): Promise<boolean> {
        if (expect(await this.pagination()).isNot) {
            return true;
        }
    }
}