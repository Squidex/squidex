import { Injectable } from '@angular/core';
import {TranslateService as NgxTranslateService} from '@ngx-translate/core';


@Injectable()
export class TranslateService {
    constructor(
    ngxTranslateService: NgxTranslateService
    ) {
        console.log(ngxTranslateService.get('HELLO'));
    }

    public getTranslation(): string { 
        return 'test';
    }

}