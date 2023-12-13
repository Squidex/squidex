/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Directive, ElementRef, HostListener, Input, OnInit, Renderer2 } from '@angular/core';
import { ActivatedRoute, NavigationEnd, QueryParamsHandling, Router } from '@angular/router';
import { Subscriptions } from '@app/framework/internal';

@Directive({
    selector: '[sqxParentLink]',
    standalone: true,
})
export class ParentLinkDirective implements OnInit {
    private readonly subscriptions = new Subscriptions();
    private url?: string;

    @Input({ transform: booleanAttribute })
    public isLazyLoaded?: boolean | null;

    @Input()
    public queryParamsHandling?: QueryParamsHandling;

    constructor(
        private readonly router: Router,
        private readonly route: ActivatedRoute,
        private readonly element: ElementRef,
        private readonly renderer: Renderer2,
    ) {
    }

    public ngOnInit() {
        this.subscriptions.add(
            this.route.url
                .subscribe(() => {
                    this.updateUrl();
                }));

        this.subscriptions.add(
            this.router.events
                .subscribe(event => {
                    if (event instanceof NavigationEnd) {
                        this.updateUrl();
                    }
                }));
    }

    @HostListener('click')
    public onClick(): boolean {
        if (this.url) {
            this.router.navigateByUrl(this.url);
        }

        return false;
    }

    private updateUrl() {
        const queryParamsHandling = this.queryParamsHandling;

        this.url = this.isLazyLoaded ?
            this.router.createUrlTree(['.'], { queryParamsHandling, relativeTo: this.route.parent!.parent }).toString() :
            this.router.createUrlTree(['.'], { queryParamsHandling, relativeTo: this.route.parent }).toString();

        this.renderer.setProperty(this.element.nativeElement, 'href', this.url);
    }
}
