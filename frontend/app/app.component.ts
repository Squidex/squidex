/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Component({
    selector: 'sqx-app',
    styleUrls: ['./app.component.scss'],
    templateUrl: './app.component.html'
})
export class AppComponent {
    public isLoaded = false;

    constructor(translate: TranslateService) {
        translate.addLangs(['en', 'nl']);
        translate.setDefaultLang('en');
        translate.use('en');
      }

}
