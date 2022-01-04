/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, HostListener, Input, OnInit, Renderer2 } from '@angular/core';
import { ActivatedRoute, NavigationEnd, QueryParamsHandling, Router } from '@angular/router';
import { ResourceOwner } from '@app/framework/internal';

@Directive({
    selector: '[sqxParentLink]',
})
export class ParentLinkDirective extends ResourceOwner implements OnInit {
    private url?: string;

    @Input()
    public isLazyLoaded?: boolean | null;

    @Input()
    public queryParamsHandling?: QueryParamsHandling;

    constructor(
        private readonly router: Router,
        private readonly route: ActivatedRoute,
        private readonly element: ElementRef,
        private readonly renderer: Renderer2,
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.route.url
                .subscribe(() => {
                    this.updateUrl();
                }));

        this.own(
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
