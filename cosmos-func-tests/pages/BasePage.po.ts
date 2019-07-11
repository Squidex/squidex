import { browser, element, by, ExpectedConditions } from "protractor";

/**
 * This is like the landing URL which we are re-directed to
 * Methods/properties for global elements should go here.
 * explain about default keyword
 */
export class BasePage {
  constructor() {}

 public loginButton() {
    return element(
      by.className("btn btn-success btn-lg login-button login-element")
    ).click();
  }
}
