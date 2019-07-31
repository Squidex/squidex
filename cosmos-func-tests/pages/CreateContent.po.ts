import { BrowserUtil } from "./../utils/Browser.util";
import {
  element,
  by
} from "protractor";

export class CreateContent extends BrowserUtil {

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

  public referencePlaceHolder(referenceName) {
    return element(
      by.xpath(
        "//label[contains(text(), '" + referenceName +  "')]/following-sibling::div/sqx-references-editor/div/div/div[@class='drop-area']"
      )
    );
  }

  public applyEditorToolBarOptions(option) {
    return element(by.className(option));
  }

  public navigateBackToContentsPage() {
    return element(by.xpath("//h3[contains(text(),'Content')]"));
  }

  public newButton() {
    return element(by.className("btn btn-success"));
  }

  public saveContent() {
    return element(by.buttonText(" Save ")).click();
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
    await this.scrollIntoView(this.referencePlaceHolder('Commodity'));
    await this.randomSelection();
    await this.scrollIntoView(this.referencePlaceHolder('Commentary Type'));
    await this.randomSelection();
    await this.scrollIntoView(this.referencePlaceHolder('Region'));
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
                return;
              });
            }
          });
        } catch (error) {
          process.stdout.write("Commodity doesn't exist" + error);
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
    await this.scrollIntoView(this.referencePlaceHolder('Commodity'));
    await this.selectContent(commodity);
  }

  public async selectCommentaryType(commentaryType) {
    await this.scrollIntoView(this.referencePlaceHolder('Commentary Type'));
    await this.selectContent(commentaryType);
  }

  public async selectRegion(region) {
    await this.scrollIntoView(this.referencePlaceHolder('Region'));
    await this.selectContent(region);
  }

  public addCommentary(commentaryText) {
    const commentaryBody = element(
      by.xpath("//div[@class='te-editor']/div/div")
    );
    if (commentaryBody != null) {
      commentaryBody.sendKeys(commentaryText);
      this.saveContent();
    } else {
      process.stdout.write("error");
    }
  }

  public addCommentaryEditorOptions(commentaryText) {
    const commentaryBody = element(
      by.xpath("//div[@class='te-editor']/div/div")
    );
    if (commentaryBody != null) {
      commentaryBody.sendKeys(commentaryText);
      this.selectAllContent();
    } else {
      process.stdout.write("error");
    }
  }
  public async commentaryEditorTest(commentary) {
    this.navigateBackToContentsPage();
    this.newButton().click();
    this.selectRandomReferences();
    this.addCommentaryEditorOptions(commentary);
  }

  public async createCommentaryWithBoldLetters(commentary) {
    this.commentaryEditorTest(commentary);
    this.applyEditorToolBarOptions("tui-bold tui-toolbar-icons").click();
    this.saveContent();
  }

  public async createCommentaryWithItalicFont(commentary) {
    this.commentaryEditorTest(commentary);
    this.applyEditorToolBarOptions("tui-italic tui-toolbar-icons").click();
    this.saveContent();
  }

  public async createBulletPointsCommentary(commentary) {
    this.commentaryEditorTest(commentary);
    this.applyEditorToolBarOptions("tui-ul tui-toolbar-icons").click();
    this.saveContent();
  }

  public async createNumberedCommentary(commentary) {
    this.commentaryEditorTest(commentary);
    this.applyEditorToolBarOptions("tui-ol tui-toolbar-icons").click();
    this.saveContent();
}
}
