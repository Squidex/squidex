/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable import/no-cycle */

import { AfterViewInit, Directive, ElementRef, HostListener, Renderer2 } from '@angular/core';
import { LayoutComponent } from './layout.component';

@Directive({
    selector: '[sqxLayoutContainer]',
})
export class LayoutContainerDirective implements AfterViewInit {
    private readonly layouts: LayoutComponent[] = [];
    private isViewInit = false;
    private containerWidth = 0;

    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer2,
    ) {
    }

    @HostListener('window:resize')
    public onResize() {
        this.invalidate(true);
    }

    public push(layout: LayoutComponent) {
        this.layouts.push(layout);
    }

    public ngAfterViewInit() {
        this.isViewInit = true;

        this.invalidate(true);
    }

    public peek() {
        this.layouts.splice(-1, 1);

        this.invalidate();
    }

    public invalidate(resize = false) {
        if (!this.isViewInit) {
            return;
        }

        if (resize) {
            this.containerWidth = this.element.nativeElement.offsetWidth;
        }

        const layouts = this.layouts;

        for (const layout of layouts) {
            if (!layout.isViewInit) {
                return;
            }
        }

        let currentSize = 0;
        let layoutsWidthSpread = 0;

        for (const layout of layouts) {
            if (layout.desiredWidth > 0) {
                const layoutWidth = layout.desiredWidth;

                layout.measure(`${layoutWidth}rem`);

                currentSize += layout.renderWidth;
            } else {
                layoutsWidthSpread++;
            }
        }

        const spreadWidth = (this.containerWidth - currentSize) / layoutsWidthSpread;

        for (const layout of layouts) {
            if (layout.desiredWidth <= 0) {
                layout.measure(`${spreadWidth}px`);

                currentSize += layout.renderWidth;
            }
        }

        let currentPosition = 0;
        let currentLayer = layouts.length * 10;

        for (const layout of layouts) {
            layout.arrange(`${currentPosition}px`, currentLayer.toString());

            currentPosition += layout.renderWidth;
            currentLayer -= 10;
        }

        const diff = Math.max(0, currentPosition - this.containerWidth);

        this.renderer.setProperty(this.element.nativeElement, 'scrollLeft', diff);
    }
}
