/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Directive, ElementRef, Input, Renderer } from '@angular/core';

@Directive({
    selector: '[sqxFocusOnInit]'
})
export class FocusOnInitDirective implements AfterViewInit {
    @Input()
    public select: boolean;

    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer
    ) {
    }

    public ngAfterViewInit() {
        setTimeout(() => {
            this.renderer.invokeElementMethod(this.element.nativeElement, 'focus', []);

            if (this.select) {
                this.renderer.invokeElementMethod(this.element.nativeElement, 'select', []);
            }
        }, 100);
    }
}