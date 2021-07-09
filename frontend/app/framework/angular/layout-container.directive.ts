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
    public pushLayout(layout: LayoutComponent) {
        this.layouts.push(layout);
    }

    public ngAfterViewInit() {
        this.isViewInit = true;

        this.invalidate(true);
    }

    public peek() {
        return this.layouts[this.layouts.length - 1];
    }

    public popLayout() {
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

        const diff = Math.max(0, this.element.nativeElement.scrollWidth - this.containerWidth);

        this.renderer.setProperty(this.element.nativeElement, 'scrollLeft', diff);
    }
}
