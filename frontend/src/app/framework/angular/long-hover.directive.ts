/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, EventEmitter, HostListener, Input, Output, Renderer2 } from '@angular/core';

@Directive({
    selector: '[sqxLongHover]',
})
export class LongHoverDirective {
    private timerOut: Function | null = null;
    private timer?: any;

    @Output('sqxLongHover')
    public hover = new EventEmitter();

    @Output('longHoverCancelled')
    public cancelled = new EventEmitter();

    @Input('longHoverSelector')
    public selector?: string;

    @Input('longHoverDuration')
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

        this.timer = setTimeout(() => {
            this.hover.emit();
        }, this.duration);

        this.timerOut = this.renderer.listen(event.target, 'mouseleave', () => {
            this.clearTimer();
        });
    }

    private clearTimer() {
        if (this.timer) {
            clearTimeout(this.timer);
            this.cancelled.emit();
            this.timer = null;
            this.timerOut?.();
            this.timerOut = null;
        }
    }
}