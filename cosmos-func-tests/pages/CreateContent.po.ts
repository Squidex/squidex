import {
    element,
    by,
    ElementFinder,
    browser,
    ElementArrayFinder,
  } from "protractor";
import constants from "../utils/constants";

export class CreateContent{

        constructor(){}
    

        // Create Commentary after Navigating to Commentary Page under Content

        public contentTable()  {
            return element.all(by.xpath("//div[@class='grid-content']/div/table[@class='table table-items table-fixed ng-star-inserted']/tbody/tr[@isreadonly='true']/td[3]/sqx-content-value/span"));
        }

        public selectRefData(): any {
            return element.all(by.xpath("//td[@class='cell-select']/input[@type='checkbox']"));
        }

        public submitSelection():ElementFinder{
            return element(by.xpath("//button[@class='float-right btn btn-success']"));
        }
        
        public NavigateToContentPage(){
            const commentaryApp : ElementFinder = element(by.xpath("//h4[@class='card-title']"));
            const content : ElementFinder = element(by.xpath("//li[2]//a[1]"));
            const commentarySchema : ElementFinder = element(by.xpath("//li[1]//a[1]//span[1]"));
            const newButton : ElementFinder = element(by.className("btn btn-success"));
            commentaryApp.click().then(async()=>{
                await content.click();
                await commentarySchema.click();
                browser.sleep(1000);
                await newButton.click();
              });
            
        }

        public SelectCommodity(commodity){
            const commodityPlaceHolder:ElementFinder = element(by.xpath("//label[contains(text(), ' Commodity  ')]/following-sibling::div/sqx-references-editor/div/div/div[@class='drop-area']"));           
            commodityPlaceHolder.click().then(async()=>{
                this.contentTable().forEach(commodityName => {
                    try {
                        if(commodityName.text === commodity){
                            const fav = this.contentTable().IndexOf(commodity);
                            // tslint:disable-next-line: no-console
                            console.log("failed");
                            this.selectRefData().ElementAt(fav).click();
                            }
                    } catch (error) {
                        // console.log("Commodity doesn't exist", error);
                    }
                });
                await this.submitSelection().click();
            })
        }

        public SelectRegion(region){
            const regionPlaceHolder:ElementFinder = element(by.xpath("//label[contains(text(), ' Region  ')]/following-sibling::div/sqx-references-editor/div/div/div[@class='drop-area']"));
            regionPlaceHolder.click().then(async()=>{
                this.contentTable().forEach((regionName) => {
                    try {
                        if(regionName.text === region){
                            const fav = this.contentTable().IndexOf(region);
                            this.selectRefData().ElementAt(fav).click();
                            }
                    } catch (error) {
                        // console.log("Region doesn't exist");
                    }
                });
                await this.submitSelection().click();
            })
        }

        public SelectCommentaryType(commentaryType){
            const commentaryTypePlaceHolder:ElementFinder = element(by.xpath("//label[contains(text(), ' Commentary Type  ')]/following-sibling::div/sqx-references-editor/div/div/div[@class='drop-area']"));          
            commentaryTypePlaceHolder.click().then(async()=>{
                this.contentTable().forEach(commentaryTypeName => {
                    try {
                        if(commentaryTypeName.text === commentaryType){
                            const fav = this.contentTable().IndexOf(commentaryType);
                            this.selectRefData().ElementAt(fav).click();
                            }
                    } catch (error) {
                        // console.log("Commentary Type doesn't exist");
                    }
                });
                await this.submitSelection().click();
            })
        }

        public AddCommentary(){
            const saveContent = element(by.buttonText(" Save "));
            const commentaryBody = element(by.className("tui-editor-contents tui-editor-contents-placeholder"));
            commentaryBody.sendKeys(constants.contentBody);          
            saveContent.click();
        }

      }
      

