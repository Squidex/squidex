/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Directive, ElementRef, Input, OnChanges, Renderer }from '@angular/core';

@Directive({
    selector: '[sqxFocusOnChange]'
})
export class FocusOnChangeDirective implements OnChanges {
    @Input()
    public sqxFocusOnChange: any;

    @Input()
    public select: boolean;

    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer
    ) {
    }

    public ngOnChanges() {
        setTimeout(() => {
            this.renderer.invokeElementMethod(this.element.nativeElement, 'focus', []);

            if (this.select) {
                this.renderer.invokeElementMethod(this.element.nativeElement, 'select', []);
            }
        }, 100);
    }
}