/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Router from '@angular/router';

import { AppsStoreService } from './../services/apps-store.service';

@Ng2.Directive({
    selector: '[dashboardLink]'
})
export class DashboardLinkDirective implements Ng2.OnInit, Ng2.OnDestroy {
    private appSubscription: any;
    private appName: string;

    constructor(
        private readonly appsStore: AppsStoreService,
        private readonly router: Ng2Router.Router
    ) {
    }

    public ngOnInit() {
        this.appSubscription =
            this.appsStore.selectedApp.subscribe(app => {
                this.appName = app.name;
            });
    }

    public ngOnDestroy() {
        this.appSubscription.unsubscribe();
    }

    @Ng2.HostListener('click')
    public onClick() {
        if (this.appName) {
            this.router.navigate(['app', this.appName]);
        }
    }
}