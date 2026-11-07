/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/no-input-rename */

import { afterNextRender, booleanAttribute, Directive, effect, ElementRef, inject, input, numberAttribute, Renderer2 } from '@angular/core';
import { Types } from '../internal';

@Directive({
    selector: '[sqxScrollActive]',
})
export class ScrollActiveDirective {
    private readonly element = inject<ElementRef<HTMLElement>>(ElementRef);
    private readonly renderer = inject(Renderer2);

    /** Scrolls the element into the view of the container when true. */
    public readonly isActive = input(false, { alias: 'sqxScrollActive', transform: booleanAttribute });

    /** The additional offset when scrolling down. */
    public readonly offset = input(0, { alias: 'sqxScrollOffset', transform: numberAttribute });

    /** The scroll container or a CSS selector to find it in the ancestors of the element. */
    public readonly container = input<HTMLElement | string | null | undefined>(undefined, { alias: 'sqxScrollContainer' });

    constructor() {
        effect(() => {
            this.check();
        });

        afterNextRender(() => {
            this.check();
        });
    }

    private check() {
        if (!this.isActive()) {
            return;
        }

        let container = this.container();
        if (Types.isString(container)) {
            container = this.element.nativeElement.closest(container) as HTMLElement;
        }

        if (container) {
            this.scrollInView(container, this.element.nativeElement);
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
                this.renderer.setProperty(parent, 'scrollTop', scrollTop + scrollDiff - parentHeight + targetHeight + this.offset());
            }
        }
    }
}
