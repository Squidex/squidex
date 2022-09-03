/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, HostListener, Input, Renderer2 } from '@angular/core';
import { ResizeListener, ResizeService, ResourceOwner } from '@app/framework/internal';

@Directive({
    selector: '[sqxCompensateScrollbar]',
})
export class CompensateScrollbarDirective extends ResourceOwner implements ResizeListener {
    private previousScrollbarWidth = -1;

    @Input('sqxCompensateScrollbar')
    public enabled?: string | boolean | null = true;

    constructor(
        private readonly renderer: Renderer2,
        private readonly element: ElementRef<HTMLElement>,
        private readonly resizeService: ResizeService,
    ) {
        super();

        this.own(this.resizeService.listen(this.element.nativeElement, this));
    }

    public ngAfterViewInit() {
        this.resize();
    }

    public onResize() {
        this.resize();
    }

    @HostListener('scroll')
    public onScroll() {
        this.resize();
    }

    private resize() {
        if (this.enabled === false) {
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
