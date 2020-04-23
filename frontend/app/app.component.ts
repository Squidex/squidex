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

        if (uiOptions.get('ngxTranslate.useBrowserLanguage')) {
            const browserLang = translate.getBrowserLang();
            translate.use(browserLang.match(/en|nl/) ? browserLang  : 'en');
        } else {
            translate.use(uiOptions.get('ngxTranslate.language'));
        }
      }

}
