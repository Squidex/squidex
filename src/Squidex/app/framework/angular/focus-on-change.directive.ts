/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

@Ng2.Directive({
    selector: '[sqxFocusOnChange]'
})
export class FocusOnChangeDirective implements Ng2.OnChanges {
    @Ng2.Input()
    public sqxFocusOnChange: any;

    @Ng2.Input()
    public select: boolean;

    constructor(
        private readonly elementRef: Ng2.ElementRef,
        private readonly renderer: Ng2.Renderer
    ) {
    }

    public ngOnChanges(changes: { [key: string]: Ng2.SimpleChange }) {
        setTimeout(() => {
            this.renderer.invokeElementMethod(this.elementRef.nativeElement, 'focus', []);

            if (this.select) {
                this.renderer.invokeElementMethod(this.elementRef.nativeElement, 'select', []);
            }
        }, 100);
    }
}