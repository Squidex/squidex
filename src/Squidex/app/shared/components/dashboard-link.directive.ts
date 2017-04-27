/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Directive, ElementRef, HostListener, OnDestroy, OnInit, Renderer } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';

import { AppsStoreService } from './../declarations-base';

@Directive({
    selector: '[sqxDashboardLink]'
})
export class DashboardLinkDirective implements OnInit, OnDestroy {
    private appSubscription: Subscription;
    private url: string;

    constructor(
        private readonly appsStore: AppsStoreService,
        private readonly router: Router,
        private readonly element: ElementRef,
        private readonly renderer: Renderer
    ) {
    }

    public ngOnDestroy() {
        this.appSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.appSubscription =
            this.appsStore.selectedApp.subscribe(app => {
                this.url = this.router.createUrlTree(['app', app!.name]).toString();

                this.renderer.setElementAttribute(this.element.nativeElement, 'href', this.url);
            });
    }

    @HostListener('click')
    public onClick(): boolean {
        this.router.navigateByUrl(this.url);

        return false;
    }
}