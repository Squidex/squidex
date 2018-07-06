/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';

import {
    AppsState,
    DialogModel,
    fadeAnimation,
    ModalModel
} from '@app/shared';

@Component({
    selector: 'sqx-apps-menu',
    styleUrls: ['./apps-menu.component.scss'],
    templateUrl: './apps-menu.component.html',
    animations: [
        fadeAnimation
    ]
})
export class AppsMenuComponent {
    public addAppDialog = new DialogModel();

    public appsMenu = new ModalModel();

    constructor(
        public readonly appsState: AppsState
    ) {
    }

    public createApp() {
        this.appsMenu.hide();
        this.addAppDialog.show();
    }
}