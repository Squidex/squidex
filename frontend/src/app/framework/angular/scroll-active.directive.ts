/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/no-input-rename */

import { AfterViewInit, booleanAttribute, Directive, ElementRef, Input, Renderer2 } from '@angular/core';

@Directive({
    selector: '[sqxScrollActive]',
    standalone: true,
})
export class ScrollActiveDirective implements AfterViewInit {
    @Input({ alias: 'sqxScrollActive', transform: booleanAttribute })
    public isActive = false;

    @Input('sqxScrollContainer')
    public container!: HTMLElement;

    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer2,
    ) {
    }

    public ngAfterViewInit() {
        this.check();
    }

    public ngOnChanges() {
        this.check();
    }

    private check() {
        if (this.isActive && this.container) {
            this.scrollInView(this.container, this.element.nativeElement);
        }
    }

    private scrollInView(parent: HTMLElement, target: HTMLElement) {
        const parentRect = parent.getBoundingClientRect();
        const targetRect = target.getBoundingClientRect();

        const body = document.body;

        const scrollDiff = (targetRect.top + body.scrollTop) - (parentRect.top + body.scrollTop);
        const scrollTop = parent.scrollTop;

        if (scrollDiff < 0) {
            this.renderer.setProperty(parent, 'scrollTop', scrollTop + scrollDiff);
        } else {
            const targetHeight = targetRect.height;
            const parentHeight = parentRect.height;

            if ((scrollDiff + targetHeight) > parentHeight) {
                this.renderer.setProperty(parent, 'scrollTop', scrollTop + scrollDiff - parentHeight + targetHeight);
            }
        }
    }
}
