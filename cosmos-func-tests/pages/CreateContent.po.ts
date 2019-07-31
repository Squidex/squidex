import { BrowserUtil } from "./../utils/Browser.util";
import {
  element,
  by,
  ElementFinder,
  browser,
  ElementArrayFinder,
  protractor
} from "protractor";
import constants from "../utils/constants";

export class CreateContent {
  constructor() {}

  // Create Commentary after Navigating to Commentary Page under Content
  public contentTable() {
    return element.all(
      by.xpath(
        "//div[@class='modal ng-trigger ng-trigger-fade']/div/div/div/div[@class='grid-content']/div/table[@class='table table-items table-fixed ng-star-inserted']/tbody/tr[@isreadonly='true']/td[3]/sqx-content-value/span"
      )
    );
  }

  public selectRefData() {
    return element.all(
      by.xpath("//td[@class='cell-select']/input[@type='checkbox']")
    );
  }

  public submitSelection() {
    return element(by.buttonText("Link selected contents (1)")).click();
  }

  public commodityPlaceHolder() {
    return element(
      by.xpath(
        "//label[contains(text(), ' Commodity  ')]/following-sibling::div/sqx-references-editor/div/div/div[@class='drop-area']"
      )
    );
  }

  public commentaryTypePlaceHolder() {
    return element(
      by.xpath(
        "//label[contains(text(), ' Commentary Type  ')]/following-sibling::div/sqx-references-editor/div/div/div[@class='drop-area']"
      )
    );
  }

  public regionPlaceHolder() {
    return element(
      by.xpath(
        "//label[contains(text(), ' Region  ')]/following-sibling::div/sqx-references-editor/div/div/div[@class='drop-area']"
      )
    );
  }

  public applyBoldFont() {
    return element(by.className("tui-bold tui-toolbar-icons"));
  }

  public applyItalicFont() {
    return element(by.className("tui-italic tui-toolbar-icons"));
  }

  public applyBulletPoints() {
    return element(by.className("tui-ul tui-toolbar-icons"));
  }

  public applyNumbering() {
    return element(by.className("tui-ol tui-toolbar-icons"));
  }

  public navigateBackToContentsPage() {
    return element(by.xpath("//h3[contains(text(),'Content')]"));
  }

  public newButton() {
    return element(by.className("btn btn-success"));
  }

  public saveContent(){
      return element(by.buttonText(" Save "));
  }

  public scrollIntoView(webelement: ElementFinder) {
    browser
      .executeScript("arguments[0].scrollIntoView()", webelement)
      .then(() => {
        webelement.click();
      });
  }

  public async navigateToContentPage() {
    const commentaryApp = element(by.xpath("//h4[@class='card-title']"));
    const content = element(by.xpath("//li[2]//a[1]"));
    const commentarySchema = element(by.xpath("//li[1]//a[1]//span[1]"));
    await commentaryApp.click();
    await content.click();
    await commentarySchema.click();
    await this.newButton().click();
  }

  public async selectRandomReferences() {
    await this.scrollIntoView(this.commodityPlaceHolder());
    await this.randomSelection();
    await this.scrollIntoView(this.regionPlaceHolder());
    await this.randomSelection();
    await this.scrollIntoView(this.commodityPlaceHolder());
    await this.randomSelection();
  }

  public async selectContent(content) {
    this.contentTable().then(contents => {
      contents.forEach(contentName => {
        try {
          contentName.getText().then(text => {
            const name = text;
            if (name === content) {
              const fav = contents.indexOf(contentName);
              this.selectRefData().then(checkbox => {
                checkbox[fav].getWebElement().click();
                browser.sleep(5000);
                return;
              });
            }
          });
        } catch (error) {
          // tslint:disable-next-line: no-console
          console.log("Commodity doesn't exist", error);
        }
      });
    });
    this.submitSelection();
  }

  public async randomSelection() {
    this.selectRefData().then(checkbox => {
      const randomItem = checkbox[Math.floor(Math.random() * checkbox.length)];
      randomItem.getWebElement().click();
      return;
    });
    this.submitSelection();
  }

  public async selectCommodity(commodity) {
    await this.scrollIntoView(this.commodityPlaceHolder());
    await this.selectContent(commodity);
  }

  public async selectCommentaryType(commentaryType) {
    await this.scrollIntoView(this.commentaryTypePlaceHolder());
    await this.selectContent(commentaryType);
  }

  public async selectRegion(region) {
    await this.scrollIntoView(this.regionPlaceHolder());
    await this.selectContent(region);
  }

  public addCommentary(commentaryText) {
    const commentaryBody = element(
      by.xpath("//div[@class='te-editor']/div/div")
    );
    if (commentaryBody != null) {
      commentaryBody.sendKeys(commentaryText);
      this.saveContent().click();
    } else {
      // tslint:disable-next-line: no-console
      console.log("error");
    }
  }

  public addCommentaryEditorOptions(commentaryText) {
    const commentaryBody = element(
      by.xpath("//div[@class='te-editor']/div/div")
    );
    if (commentaryBody != null) {
      commentaryBody.sendKeys(commentaryText);
      browser
      .actions()
      .keyDown(protractor.Key.ALT)
      .sendKeys("a")
      .keyUp(protractor.Key.ALT)
      .perform();
    } else {
      // tslint:disable-next-line: no-console
      console.log("error");
    }
  }
  public async commentaryEditorTest(commentary){
    this.navigateBackToContentsPage();
    this.newButton().click();
    this.selectRandomReferences();
    this.addCommentaryEditorOptions(commentary);
    this.saveContent().click();
  }


  public async createCommentaryWithBoldLetters(commentary) {
    this.commentaryEditorTest(commentary);
    this.applyBoldFont().click();
  }

  public async createCommentaryWithItalicFont(commentary) {
    this.commentaryEditorTest(commentary);
    this.applyItalicFont().click();
  }

  public async createBulletPointsCommentary(commentary) {
    this.commentaryEditorTest(commentary);
    this.applyBulletPoints().click();
  }

  public async createNumberedCommentary(commentary) {
    this.commentaryEditorTest(commentary);
    this.applyNumbering().click();
  }
}
