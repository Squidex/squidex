import { LoginData } from './../data/loginData';
import { browser, element, by, ExpectedConditions, ElementFinder, $ } from 'protractor';
import {BasePage} from "./basePage.po";
import { config } from '../config';
import { Alert } from 'selenium-webdriver';

/** 
 * Class representing login page.
 * Login window which opens after clicking on Login button on Squidex base page
 */
export class LoginPage extends BasePage{

    //not required for this page. Just written for reference
    userNameTextBox() {
        return element(by.id('userNameInput'));
    }

    userPasswordTextBox() {
        return element(by.id('passwordInput'));
    }

    loginSubmitButton() {
        return element(by.id('submitButton'));
    }

    /**
     * signs in using specified username/password in Data File.
     */
    login(loginData : LoginData) {
        const usernameInput = element(by.id('userNameInput'));
        const passwordInput = element(by.id('passwordInput'));
        const submitButton = element(by.id('submitButton'));
        usernameInput.sendKeys(loginData.username);
        passwordInput.sendKeys(loginData.password);
        submitButton.click();
    }

    //switching to login pop-up
    navigateToSquidex(relativeUrl: string) {
        browser.get(config.params.baseUrl + relativeUrl);
    }

    //navigating to Squidex base page
    navigateTo() {
        browser.get(config.params.baseUrl);
    }
}