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
    private url: string;

    constructor(
        private readonly appsStore: AppsStoreService,
        private readonly router: Ng2Router.Router,
        private readonly element: Ng2.ElementRef,
        private readonly renderer: Ng2.Renderer
    ) {
    }

    public ngOnInit() {
        this.appSubscription =
            this.appsStore.selectedApp.subscribe(app => {
                this.url = this.router.createUrlTree(['app', app.name]).toString();

                this.renderer.setElementAttribute(this.element.nativeElement, 'href', this.url);
            });
    }

    public ngOnDestroy() {
        this.appSubscription.unsubscribe();
    }

    @Ng2.HostListener('click')
    public onClick() {
        this.router.navigateByUrl(this.url);

        return false;
    }
}