import { browser, element, by, ExpectedConditions, ElementFinder, $ } from 'protractor';
import {BasePage} from "./basePage.po";
import { config } from '../config';
import { Alert } from 'selenium-webdriver';

/** 
 * Class representing login page.
 * Login window which opens after clicking on Login button on Squidex base page
 */
export class HomePage {

    constructor() {}

    //to validate Squidex home page options after login for Vega test Editor
    commentaryDisplay() :ElementFinder{
        return element(by.className('card-title'));
    }

    userNameDisplay(): ElementFinder {
        return element(by.className('apps-title'));
    }

    userProfileIconDisplay() : ElementFinder{
        return element(by.className('user-picture'));
    }

    userLogout(){
        const userProfile = element(by.css('.ng-tns-c7-3'));
        const logoutButton = userProfile.element(by.cssContainingText('.dropdown-item', 'Logout'));
        userProfile.click().then(()=>{
        logoutButton.click();
        })
        
    }

    
}