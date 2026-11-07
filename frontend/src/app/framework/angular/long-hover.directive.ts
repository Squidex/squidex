/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/no-input-rename */
/* eslint-disable @angular-eslint/no-output-rename */

import { DestroyRef, Directive, inject, input, numberAttribute, output, Renderer2 } from '@angular/core';

@Directive({
    selector: '[sqxLongHover]',
    host: {
        '(mouseover)': 'onMove($event)',
    },
})
export class LongHoverDirective {
    private readonly renderer = inject(Renderer2);
    private timerOut: Function | null = null;
    private timer?: any;
    private wasHovering = false;

    /** Emits when the mouse stays over the element for the given duration. */
    public readonly hover = output({ alias: 'sqxLongHover' });

    /** Emits when the mouse leaves the element after a long hover. */
    public readonly cancelled = output({ alias: 'longHoverCancelled' });

    /** The CSS selector of the child elements that trigger the long hover. */
    public readonly selector = input<string | undefined>(undefined, { alias: 'longHoverSelector' });

    /** The time in milliseconds until the long hover is triggered. */
    public readonly duration = input(2000, { alias: 'longHoverDuration', transform: numberAttribute });

    constructor() {
        inject(DestroyRef).onDestroy(() => {
            clearTimeout(this.timer);
            this.timerOut?.();
        });
    }

    protected onMove(event: MouseEvent) {
        if (!(event.target instanceof Element)) {
            this.clearTimer();
            return;
        }

        const selector = this.selector();

        const isMatch = !selector || event.target.matches(selector);
        if (!isMatch) {
            this.clearTimer();
            return;
        }

        if (this.timer) {
            return;
        }

        this.wasHovering = false;

        this.timer = setTimeout(() => {
            this.wasHovering = true;

            this.hover.emit();
        }, this.duration());

        this.timerOut = this.renderer.listen(event.target, 'mouseleave', () => {
            this.clearTimer();
        });
    }

    private clearTimer() {
        if (this.timer) {
            if (this.wasHovering) {
                this.cancelled.emit();
            }

            clearTimeout(this.timer);
            this.timer = null;
            this.timerOut?.();
            this.timerOut = null;
        }
    }
}
