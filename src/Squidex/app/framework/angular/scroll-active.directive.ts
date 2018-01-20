/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Directive, ElementRef, Input, OnChanges } from '@angular/core';

@Directive({
    selector: '[sqxScrollActive]'
})
export class ScrollActiveDirective implements AfterViewInit, OnChanges {
    @Input('sqxScrollActive')
    public isActive = false;

    @Input()
    public container: HTMLElement;

    constructor(
        private readonly element: ElementRef
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
        if (!parent.getBoundingClientRect) {
            return;
        }

        const parentRect = parent.getBoundingClientRect();
        const targetRect = target.getBoundingClientRect();

        const offset = (targetRect.top + document.body.scrollTop) - (parentRect.top + document.body.scrollTop);

        const scroll = parent.scrollTop;

        if (offset < 0) {
            parent.scrollTop = scroll + offset;
        } else {
            const targetHeight = targetRect.height;
            const parentHeight = parentRect.height;

            if ((offset + targetHeight) > parentHeight) {
                parent.scrollTop = scroll + offset - parentHeight + targetHeight;
            }
        }
    }
}