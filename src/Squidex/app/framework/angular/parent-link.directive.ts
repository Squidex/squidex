/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Directive, ElementRef, HostListener, Input, OnDestroy, OnInit, Renderer } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';

@Directive({
    selector: '[sqxParentLink]'
})
export class ParentLinkDirective implements OnDestroy, OnInit {
    private urlSubscription: Subscription;
    private url: string;

    @Input()
    public isLazyLoaded = false;

    constructor(
        private readonly router: Router,
        private readonly route: ActivatedRoute,
        private readonly element: ElementRef,
        private readonly renderer: Renderer
    ) {
    }

    public ngOnDestroy() {
        if (this.urlSubscription) {
            this.urlSubscription.unsubscribe();
        }
    }

    public ngOnInit() {
        this.urlSubscription =
            this.route.url.subscribe(() => {
                this.url = this.isLazyLoaded ?
                    this.router.createUrlTree(['.'], { relativeTo: this.route.parent!.parent }).toString() :
                    this.router.createUrlTree(['.'], { relativeTo: this.route.parent }).toString();

                this.renderer.setElementAttribute(this.element.nativeElement, 'href', this.url);
            });
    }

    @HostListener('click')
    public onClick(): boolean {
        this.router.navigateByUrl(this.url);

        return false;
    }
}