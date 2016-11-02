/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import {
    AppDto, 
    AppsStoreService 
} from './../../shared';

import { fadeAnimation, ModalView } from './../../framework';

@Ng2.Component({
    selector: 'sqx-apps-menu',
    styles,
    template,
    animations: [
        fadeAnimation()
    ]
})
export class AppsMenuComponent implements Ng2.OnInit, Ng2.OnDestroy {
    private subscription: any | null = null;

    public modalMenu = new ModalView();
    public modalDialog = new ModalView();

    public apps: AppDto[] | null = null;

    constructor(
        private readonly appsStore: AppsStoreService 
    ) {
    }

    public ngOnInit() {
        this.subscription = this.appsStore.appsChanges.subscribe(apps => {
            this.apps = apps;
        });
    }

    public ngOnDestroy() {
        if (this.subscription) {
            this.subscription.unsubscribe();
        }
    }

    public createApp() {
        this.modalMenu.hide();
        this.modalDialog.show();
    }
}