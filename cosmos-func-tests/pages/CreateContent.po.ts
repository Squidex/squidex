// import { ENTER } from './../../src/Squidex/app/framework/utils/keys';
// import { IFrameEditorComponent } from './../../src/Squidex/app/framework/angular/forms/iframe-editor.component';
import { BrowserUtil } from "./../utils/Browser.util";
import {
  element,
  by,
  browser,
  protractor,
  ElementFinder,
  WebElement,
  Key
} from "protractor";
import { Driver } from "selenium-webdriver/chrome";

const searchBar = element(by.xpath("//input[@placeholder='Search']"));

const searchResult = element.all(by.xpath("//span[@class='truncate ng-star-inserted']/b"));

export class CreateContent extends BrowserUtil {

  // Create Commentary after Navigating to Commentary Page under Content
  public async contentTable() {
    return await element.all(
      by.xpath("//div[@class='control-dropdown-items']/div/span")
    );
  }

  public async selectRefData() {
    return await element.all(
      by.xpath("//td[@class='cell-select']/input[@type='checkbox']")
    );
  }

  public async submitSelection() {
    return await element(by.buttonText("Link selected contents (1)")).click();
  }

  public async referencePlaceHolder(referenceName) {
    return await element(
      by.xpath(
        "//label[contains(text(), '"+referenceName+"')]/following-sibling::div/sqx-references-dropdown/sqx-dropdown/span/div/input[@class='form-control']"
      )
    );
  }

  public async applyEditorToolBarOptions(option) {
    return await element(by.className(option));
  }

  public async navigateToContentsPage() {
    return await element(by.xpath("//h3[contains(text(),'Content')]"));
  }

  public async newButton() {
    return await element(by.className("btn btn-success"));
  }

  public async saveContent() {
    return await element(by.buttonText(" Save ")).click();
  }

  public async navigateToCommentaryAppPage() {
    const commentaryApp = element(by.cssContainingText('.card-title','commentary'));
    const content = element(by.cssContainingText('.nav-text','Content'));
    const commentarySchema = element(by.xpath("//li[1]//a[1]//span[1]"));
    await this.waitForElementToBeVisibleAndClick(commentaryApp);
    await this.waitForElementToBeVisibleAndClick(content);
    await this.waitForElementToBeVisibleAndClick(commentarySchema);
    await this.waitForElementToBeVisibleAndClick(await this.newButton());
  }

  public async selectRandomReferences() {
    await this.scrollIntoViewAndClick(await this.referencePlaceHolder('Commodity'));
    await this.randomSelection();
    await this.scrollIntoViewAndClick(await this.referencePlaceHolder('Commentary Type'));
    await this.randomSelection();
    await this.scrollIntoViewAndClick(await this.referencePlaceHolder('Region'));
    await this.randomSelection();
  }

  public async selectContent(content){
    await this.waitForElementToBePresentAndWrite(searchBar, content);
    if(searchResult.isPresent() && searchResult.getText() === content){
      await browser.actions().sendKeys(protractor.Key.ARROW_DOWN).perform();
    }
    await browser.actions().sendKeys(protractor.Key.ENTER).perform();
  }

  public async randomSelection() {
    this.selectRefData().then(checkbox => {
      const randomItem = checkbox[Math.floor(Math.random() * checkbox.length)];
      randomItem.getWebElement().click();
      return;
    });
  }

  public async selectCommodity(commodity) {
    await this.scrollIntoViewAndClick(await this.referencePlaceHolder(' Commodity  '));
    await this.selectContent(commodity);
  }

  public async selectCommentaryType(commentaryType) {
    await this.scrollIntoViewAndClick(await this.referencePlaceHolder(' Commentary Type  '));
    await this.selectContent(commentaryType);
  }

  public async selectRegion(region) {
    await this.scrollIntoViewAndClick(await this.referencePlaceHolder(' Region  '));
    await this.selectContent(region);
  }

  public async addCommentary(commentaryText) {
    const editorFrame = await element(by.xpath("//iframe[@src='https://localhost:5000/editors/toastui/md-editor.html']")).getWebElement();
    try {
      await browser.switchTo().frame(editorFrame);
      // tslint:disable-next-line: no-console
      await console.log("first step done");
      await browser.waitForAngularEnabled(false);
      // tslint:disable-next-line: no-console
      await console.log("second step done");
      const editor = await element(by.xpath("//div[@class='te-editor']/div[@contenteditable='true']/div"));
      await editor.click();
      await browser.executeScript(editor.sendKeys("Test"));
      // tslint:disable-next-line: no-console
      await console.log("After Click");
      // await browser.switchTo().frame(0);
      // await editor.sendKeys('C:\VEGA-261\\cosmos\\cosmos-func-tests\\input.txt');
      // tslint:disable-next-line: no-console
      await console.log("editor null");
      // tslint:disable-next-line: no-console
      await console.log("fourth step done");
      await browser.switchTo().defaultContent();
      // await browser.waitForAngular();
    } catch (error) {
      process.stdout.write(error);
    }
  }

  public async createCommentary(commentary){
    await this.addCommentary(commentary);
    await this.saveContent();
  }

  public async commentaryEditorTest(commentary) {
    await this.navigateToContentsPage();
    await this.waitForElementToBeVisibleAndClick(await this.newButton());
    await this.selectRandomReferences();
    await this.addCommentary(commentary);
    await this.selectAllContent();
  }
  public async createCommentaryWithBoldLetters(commentary) {
    await this.commentaryEditorTest(commentary);
    await this.waitForElementToBeVisibleAndClick(await this.applyEditorToolBarOptions("tui-bold tui-toolbar-icons"));
    await this.saveContent();
  }

  public async createCommentaryWithItalicFont(commentary) {
    await this.commentaryEditorTest(commentary);
    await this.waitForElementToBeVisibleAndClick(await this.applyEditorToolBarOptions("tui-italic tui-toolbar-icons"));
    await this.saveContent();
  }

  public async createBulletPointsCommentary(commentary) {
    await this.commentaryEditorTest(commentary);
    await this.waitForElementToBeVisibleAndClick(await this.applyEditorToolBarOptions("tui-ul tui-toolbar-icons"))
    await this.saveContent();
  }

  public async createNumberedCommentary(commentary) {
    await this.commentaryEditorTest(commentary);
    await this.waitForElementToBeVisibleAndClick(await this.applyEditorToolBarOptions("tui-ol tui-toolbar-icons"))
    await this.saveContent();
}
}
