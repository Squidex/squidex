import { Injectable } from '@angular/core';
import {TranslateService as NgxTranslateService} from '@ngx-translate/core';


@Injectable()
export class TranslateService {
    public res: string;

    constructor(
    private ngxTranslateService: NgxTranslateService
    ) {}

    public getTranslation(): string {
        this.ngxTranslateService.get('test', {value: 'world'}).subscribe((res: string) => {
            this.res = res;
        });

        return this.res;
    }

}