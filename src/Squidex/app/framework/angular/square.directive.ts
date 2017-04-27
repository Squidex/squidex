/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Directive, ElementRef, HostListener, OnInit, Renderer } from '@angular/core';

@Directive({
    selector: '[sqxSquare]'
})
export class SquareDirective implements OnInit {

    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer
    ) {
    }

    public ngOnInit() {
        this.resize();
    }

    @HostListener('resize')
    public onResize() {
        this.resize();
    }

    private resize() {
        const size = this.element.nativeElement.getBoundingClientRect();

        this.renderer.setElementStyle(this.element.nativeElement, 'height', size.width + 'px');
    }
}