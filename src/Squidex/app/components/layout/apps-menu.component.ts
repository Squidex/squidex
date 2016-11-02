/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Router from '@angular/router';

import {
    AppDto, 
    AppsStoreService 
} from './../../shared';

import { fadeAnimation, ModalView } from './../../framework';

const FALLBACK_NAME = 'Apps Overview';

@Ng2.Component({
    selector: 'sqx-apps-menu',
    styles,
    template,
    animations: [
        fadeAnimation()
    ]
})
export class AppsMenuComponent implements Ng2.OnInit, Ng2.OnDestroy {
    private appsSubscription: any | null = null;
    private routeSubscription: any | null = null;

    public modalMenu = new ModalView();
    public modalDialog = new ModalView();

    public apps: AppDto[] | null = null;

    public app = FALLBACK_NAME;

    constructor(
        private readonly appsStore: AppsStoreService,
        private readonly route: Ng2Router.ActivatedRoute
    ) {
    }

    public ngOnInit() {
        this.appsSubscription = this.appsStore.appsChanges.subscribe(apps => {
            this.apps = apps;
        });

        this.routeSubscription = this.route.params.map(p => p['app']).subscribe(app => {
            this.app = app || FALLBACK_NAME;
        });
    }

    public ngOnDestroy() {
        if (this.appsSubscription) {
            this.appsSubscription.unsubscribe();
            this.appsSubscription = null;
        }

        if (this.routeSubscription) {
            this.routeSubscription.unsubscribe();
            this.routeSubscription = null;
        }
    }

    public createApp() {
        this.modalMenu.hide();
        this.modalDialog.show();
    }
}