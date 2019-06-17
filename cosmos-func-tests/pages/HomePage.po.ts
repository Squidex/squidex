import { BrowserUtil } from "../utils/Browser.util";
import {
  browser,
  element,
  by,
  ExpectedConditions,
  ElementFinder,
  $
} from "protractor";
import { config } from "../config";
import { Alert } from "selenium-webdriver";

/**
 * Class representing login page.
 * Login window which opens after clicking on Login button on Squidex base page
 */
export class HomePage {
  //to validate Squidex home page options after login
  commentaryDisplay(): ElementFinder {
    return element(by.className("card-title"));
  }

  userNameDisplay(): ElementFinder {
    return element(by.className("apps-title"));
  }

  userProfileIconDisplay(): ElementFinder {
    return element(by.className("user-picture"));
  }

  userLogout() {
    const userProfile = element(by.xpath("//span[@class='ng-tns-c7-3']"));
    const logoutButton = userProfile.element(
      by.xpath("//a[contains(text(),'Logout')]")
    );
    userProfile.click().then(async () => {
      await logoutButton.click();
    });
  }
}
