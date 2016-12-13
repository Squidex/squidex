/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';

import {
    AppsStoreService,
    ModalView,
    TitleService
} from 'shared';

@Component({
    selector: 'sqx-apps-page',
    styleUrls: ['./apps-page.component.scss'],
    templateUrl: './apps-page.component.html'
})
export class AppsPageComponent implements OnInit {
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