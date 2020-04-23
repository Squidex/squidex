/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { UIOptions } from '@app/shared';
import { TranslateService } from '@ngx-translate/core';

@Component({
    selector: 'sqx-app',
    styleUrls: ['./app.component.scss'],
    templateUrl: './app.component.html'
})
export class AppComponent {
    public isLoaded = false;

    constructor(translate: TranslateService, uiOptions: UIOptions) {
        translate.addLangs(['en', 'nl']);
        translate.setDefaultLang('en');
        const browserLang = translate.getBrowserLang();
        translate.use(browserLang.match(/en|nl/) ? browserLang : uiOptions.get('ngxLanuage.language'));
      }

}
