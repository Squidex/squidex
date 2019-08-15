import {
  element,
  by,
  promise,
  ElementFinder,
} from "protractor";
/**
 * Class representing login page.
 * Login window which opens after clicking on Login button on Squidex base page
 */
export class HomePage {
  // to validate Squidex home page options after login
  public commentaryDisplay(): ElementFinder {
    return element(by.className("card-title"));
  }

  public userNameDisplay() : promise.Promise<string> {
    return element(by.className("apps-title")).getText();
  }

 public userProfileIconDisplay(): ElementFinder {
    return element(by.className("user-picture"));
  }

  public userLogout() {
    const userProfile = element(by.css(".user"));
    const logoutButton = userProfile.element(
      by.xpath("//a[contains(text(),'Logout')]")
    );
    userProfile.click().then(async () => {
      await logoutButton.click();
    });
  }
}
