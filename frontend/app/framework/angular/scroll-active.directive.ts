/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Directive, ElementRef, Input, OnChanges, Renderer2 } from '@angular/core';

@Directive({
    selector: '[sqxScrollActive]',
})
export class ScrollActiveDirective implements AfterViewInit, OnChanges {
    @Input('sqxScrollActive')
    public isActive = false;

    @Input('sqxScrollContainer')
    public container: HTMLElement;

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

        const offset = (targetRect.top + body.scrollTop) - (parentRect.top + body.scrollTop);

        const scroll = parent.scrollTop;

        if (offset < 0) {
            this.renderer.setProperty(parent, 'scrollTop', scroll + offset);
        } else {
            const targetHeight = targetRect.height;
            const parentHeight = parentRect.height;

            if ((offset + targetHeight) > parentHeight) {
                this.renderer.setProperty(parent, 'scrollTop', scroll + offset - parentHeight + targetHeight);
            }
        }
    }
}
