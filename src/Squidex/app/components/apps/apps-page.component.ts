/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { 
    fadeAnimation, 
    ModalView,
    TitleService 
} from './../../framework';

@Ng2.Component({
    selector: 'sqx-apps-page',
    styles,
    template,
    animations: [
        fadeAnimation()
    ]
})
export class AppsPageComponent implements Ng2.OnInit {
    public modalDialog = new ModalView();

    constructor(
        private readonly title: TitleService
    ) {
    }

    public ngOnInit() {
        this.title.setTitle('Apps');
    }
}