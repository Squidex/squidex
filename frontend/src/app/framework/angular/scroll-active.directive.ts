/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Directive, ElementRef, Input, Renderer2 } from '@angular/core';

@Directive({
    selector: '[sqxScrollActive]',
})
export class ScrollActiveDirective implements AfterViewInit {
    @Input('sqxScrollActive')
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

        const scrollOffset = (targetRect.top + body.scrollTop) - (parentRect.top + body.scrollTop);
        const scrollTop = parent.scrollTop;

        if (scrollOffset < 0) {
            this.renderer.setProperty(parent, 'scrollTop', scrollTop + scrollOffset);
        } else {
            const targetHeight = targetRect.height;
            const parentHeight = parentRect.height;

            if ((scrollOffset + targetHeight) > parentHeight) {
                this.renderer.setProperty(parent, 'scrollTop', scrollTop + scrollOffset - parentHeight + targetHeight);
            }
        }
    }
}
