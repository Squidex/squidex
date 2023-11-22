/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/no-input-rename */

import { AfterViewInit, booleanAttribute, Directive, ElementRef, Input, numberAttribute, Renderer2 } from '@angular/core';
import { TypedSimpleChanges, Types } from '../internal';

@Directive({
    selector: '[sqxScrollActive]',
    standalone: true,
})
export class ScrollActiveDirective implements AfterViewInit {
    @Input({ alias: 'sqxScrollActive', transform: booleanAttribute })
    public isActive = false;

    @Input({ alias: 'sqxScrollOffset', transform: numberAttribute })
    public offset = 0;

    @Input('sqxScrollContainer')
    public container?: HTMLElement | string | null;

    constructor(
        private readonly element: ElementRef<HTMLElement>,
        private readonly renderer: Renderer2,
    ) {
    }

    public ngAfterViewInit() {
        this.check();
    }

    public ngOnChanges(changes: TypedSimpleChanges<ScrollActiveDirective>) {
        if (changes.isActive) {
            this.check();
        }
    }

    private check() {
        if (this.isActive && this.container) {
            let container = this.container;

            if (Types.isString(container)) {
                container = this.element.nativeElement.closest(container) as HTMLElement;
            }

            if (container) {
                this.scrollInView(container, this.element.nativeElement);
            }
        }
    }

    private scrollInView(parent: HTMLElement, target: HTMLElement) {
        const boundsParent = parent.getBoundingClientRect();
        const boundsTarget = target.getBoundingClientRect();

        const body = document.body;

        const scrollDiff = (boundsTarget.top + body.scrollTop) - (boundsParent.top + body.scrollTop);
        const scrollTop = parent.scrollTop;

        if (scrollDiff < 0) {
            this.renderer.setProperty(parent, 'scrollTop', scrollTop + scrollDiff);
        } else {
            const targetHeight = boundsTarget.height;
            const parentHeight = boundsParent.height;

            if ((scrollDiff + targetHeight) > parentHeight) {
                this.renderer.setProperty(parent, 'scrollTop', scrollTop + scrollDiff - parentHeight + targetHeight + this.offset);
            }
        }
    }
}
