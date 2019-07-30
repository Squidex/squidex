import { element, by } from "protractor";

export default class SearchContent{

    constructor(){}


    public contentsList(){
        return element.all(by.tagName("ng-reflect-value")).all(by.tagName("span"));
    }

    public commentaryTest(){
        return element(by.xpath("//div[@class='te-editor']/div/div"));
    }

    public boldContentTest(){
        return element(by.tagName("b"));
    }

    public italicContentTest(){
        return element(by.tagName("i"));
    }

    public BulletPointContentTest(){
        return element(by.tagName("ul")).element(by.tagName("li"));
    }

    public NumberedContentTest(){
        return element(by.tagName("ol")).element(by.tagName("li"));
    }

    public verifyCommentaryCreation() : string{
        let value = null;
        this.contentsList().then((contents)=>{
            contents.forEach(contentName => {
                 try {
                     if(contentName.contains("Commentary") && contentName!=null){
                     contentName.click();
                     value = this.commentaryTest().getText();
                     return;
                     }
                 }
                 catch (error) {
                     // tslint:disable-next-line: no-console
                     console.log("Commentary doesn't exist", error);
                 }
             });
         });
         return value;
    }

    public verifyBoldCommentaryCreation() : string {
        let value = null;
        this.contentsList().then((contents)=>{
           contents.forEach(contentName => {
                try {
                    if(contentName.contains("Bold")&& contentName!=null){
                    contentName.click();
                    value = this.boldContentTest().getText();
                    return;
                    }
                }
                catch (error) {
                    // tslint:disable-next-line: no-console
                    console.log("Commentary doesn't exist", error);                   
                }
            });
        });
        return value;
    }


        public verifyItalicCommentaryCreation(): string {
            let value = null;
            this.contentsList().then((contents)=>{
               contents.forEach(contentName => {
                    try {
                        if(contentName.contains("Italic")&& contentName!=null){
                        contentName.click();
                        value = this.italicContentTest().getText();
                        return;
                        }
                    }
                    catch (error) {
                        // tslint:disable-next-line: no-console
                        console.log("Commentary doesn't exist", error);                     
                    }
                });
            });
            return value;
        }


            public verifyNumberedCommentaryCreation() : string {
                let value = null;
                this.contentsList().then((contents)=>{
                   contents.forEach(contentName => {
                        try {
                            if(contentName.contains("Numbered")&& contentName!=null){
                            contentName.click();
                            value = this.NumberedContentTest().getText();
                            return;
                            }
                        }
                        catch (error) {
                            // tslint:disable-next-line: no-console
                            console.log("Commentary doesn't exist", error);
                        }
                    });
                });
                return value;
            }


            public verifyBulletPointsCommentaryCreation() : string {
                let value = null;
                this.contentsList().then((contents)=>{
                   contents.forEach(contentName => {
                        try {
                            if(contentName.contains("Bullet")){
                            contentName.click();
                            value = this.BulletPointContentTest().getText();
                                return;
                            }
                        }
                        catch (error) {
                            // tslint:disable-next-line: no-console
                            console.log("Commentary doesn't exist", error);
                        }
                    });
                });
                return value;
            }
}