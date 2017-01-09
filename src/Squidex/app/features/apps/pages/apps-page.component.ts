/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';

import {
    AppsStoreService,
    fadeAnimation,
    ModalView
} from 'shared';

@Component({
    selector: 'sqx-apps-page',
    styleUrls: ['./apps-page.component.scss'],
    templateUrl: './apps-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class AppsPageComponent implements OnInit {
    public addAppDialog = new ModalView();

    public apps =
        this.appsStore.apps.map(a => a || []);

    constructor(
        private readonly appsStore: AppsStoreService
    ) {
    }

    public ngOnInit() {
        this.appsStore.selectApp(null);
    }
}