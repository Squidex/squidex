/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import {
    AppsStoreService,
    ModalView,
    TitleService
} from 'shared';

@Ng2.Component({
    selector: 'sqx-apps-page',
    styles,
    template
})
export class AppsPageComponent implements Ng2.OnInit {
    public modalDialog = new ModalView();

    constructor(
        private readonly title: TitleService,
        private readonly appsStore: AppsStoreService
    ) {
    }

    public ngOnInit() {
        this.appsStore.selectApp(null);

        this.title.setTitle('Apps');
    }
}