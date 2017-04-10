/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Directive, ElementRef, HostListener, Renderer } from '@angular/core';

@Directive({
    selector: '[sqxHideInvalidImage]'
})
export class HideInvalidImage {
    constructor(
        private readonly elementRef: ElementRef,
        private readonly renderer: Renderer
    ) {
    }

    @HostListener('error')
    public onError() {
        this.renderer.setElementStyle(this.elementRef.nativeElement, 'visibility', 'hidden');
    }

    @HostListener('load')
    public onLoad() {
        this.renderer.setElementStyle(this.elementRef.nativeElement, 'visibility', 'visible');
    }
}