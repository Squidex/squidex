/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Directive, effect, ElementRef, inject, input, Renderer2 } from '@angular/core';
import { ActivatedRoute, NavigationEnd, QueryParamsHandling, Router } from '@angular/router';

@Directive({
    selector: '[sqxParentLink]',
    host: {
        '(click)': 'onClick()',
    },
})
export class ParentLinkDirective {
    private readonly element = inject(ElementRef);
    private readonly renderer = inject(Renderer2);
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private url?: string;

    /** Navigates one level further up, because the current route is a lazy loaded module. */
    public readonly isLazyLoaded = input(false, { transform: booleanAttribute });

    /** Defines how the query parameters are handled for the parent URL. */
    public readonly queryParamsHandling = input<QueryParamsHandling>();

    constructor() {
        effect(onCleanup => {
            const isLazyLoaded = this.isLazyLoaded();
            const queryParamsHandling = this.queryParamsHandling();

            const subscriptionUrl = this.route.url
                .subscribe(() => {
                    this.updateUrl(isLazyLoaded, queryParamsHandling);
                });

            const subscriptionEvents = this.router.events
                .subscribe(event => {
                    if (event instanceof NavigationEnd) {
                        this.updateUrl(isLazyLoaded, queryParamsHandling);
                    }
                });

            onCleanup(() => {
                subscriptionUrl.unsubscribe();
                subscriptionEvents.unsubscribe();
            });
        });
    }

    protected onClick(): boolean {
        if (this.url) {
            this.router.navigateByUrl(this.url);
        }

        return false;
    }

    private updateUrl(isLazyLoaded: boolean, queryParamsHandling: QueryParamsHandling | undefined) {
        this.url = isLazyLoaded ?
            this.router.createUrlTree(['.'], { queryParamsHandling, relativeTo: this.route.parent!.parent }).toString() :
            this.router.createUrlTree(['.'], { queryParamsHandling, relativeTo: this.route.parent }).toString();

        this.renderer.setProperty(this.element.nativeElement, 'href', this.url);
    }
}
