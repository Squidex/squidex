/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/no-input-rename */
/* eslint-disable @angular-eslint/no-output-rename */

import { Directive, EventEmitter, HostListener, Input, numberAttribute, Output, Renderer2 } from '@angular/core';

@Directive({
    selector: '[sqxLongHover]',
    standalone: true,
})
export class LongHoverDirective {
    private timerOut: Function | null = null;
    private timer?: any;
    private wasHovering = false;

    @Output('sqxLongHover')
    public hover = new EventEmitter();

    @Output('longHoverCancelled')
    public cancelled = new EventEmitter();

    @Input('longHoverSelector')
    public selector?: string;

    @Input({ alias: 'longHoverDuration', transform: numberAttribute })
    public duration = 2000;

    constructor(
        private readonly renderer: Renderer2,
    ) {
    }

    @HostListener('mouseover', ['$event'])
    public onMove(event: MouseEvent) {
        if (!(event.target instanceof Element)) {
            this.clearTimer();
            return;
        }

        const isMatch = !this.selector || event.target.matches(this.selector);

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
        }, this.duration);

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