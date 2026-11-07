/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { afterNextRender, booleanAttribute, DestroyRef, Directive, ElementRef, inject, input, Renderer2 } from '@angular/core';
import { ResizeListener, ResizeService } from '@app/framework/internal';

@Directive({
    selector: '[sqxCompensateScrollbar]',
    host: {
        '(scroll)': 'onScroll()',
    },
})
export class CompensateScrollbarDirective implements ResizeListener {
    private readonly destroyRef = inject(DestroyRef);
    private readonly element = inject<ElementRef<HTMLElement>>(ElementRef);
    private readonly renderer = inject(Renderer2);
    private readonly resizeService = inject(ResizeService);
    private previousScrollbarWidth = -1;

    /** Adds a left padding with the width of the scrollbar to keep the content centered. */
    public readonly enabled = input(true, { alias: 'sqxCompensateScrollbar', transform: booleanAttribute });

    constructor() {
        const unlisten = this.resizeService.listen(this.element.nativeElement, this);

        this.destroyRef.onDestroy(unlisten);

        afterNextRender(() => {
            this.resize();
        });
    }

    public onResize() {
        this.resize();
    }

    protected onScroll() {
        this.resize();
    }

    private resize() {
        if (!this.enabled()) {
            return;
        }

        const element = this.element.nativeElement;

        const clientWidth = element.clientWidth;
        const scrollWidth = element.scrollWidth;
        const scollbarWidth = clientWidth === scrollWidth ? element.offsetWidth - clientWidth : 0;

        if (this.previousScrollbarWidth == scollbarWidth) {
            return;
        }

        this.renderer.setStyle(element, 'padding-left', `${scollbarWidth}px`);

        this.previousScrollbarWidth = scollbarWidth;
    }
}
