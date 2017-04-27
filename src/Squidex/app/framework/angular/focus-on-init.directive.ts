/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Directive, ElementRef, Input, OnInit, Renderer } from '@angular/core';

@Directive({
    selector: '[sqxFocusOnInit]'
})
export class FocusOnInitDirective implements OnInit {
    @Input()
    public select: boolean;

    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer
    ) {
    }

    public ngOnInit() {
        setTimeout(() => {
            this.renderer.invokeElementMethod(this.element.nativeElement, 'focus', []);

            if (this.select) {
                this.renderer.invokeElementMethod(this.element.nativeElement, 'select', []);
            }
        });
    }
}