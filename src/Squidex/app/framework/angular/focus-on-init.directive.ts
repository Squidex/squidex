/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

@Ng2.Directive({
    selector: '[sqxFocusOnInit]'
})
export class FocusOnInitDirective implements Ng2.OnInit {
    @Ng2.Input()
    public select: boolean;

    constructor(
        private readonly elementRef: Ng2.ElementRef,
        private readonly renderer: Ng2.Renderer
    ) {
    }

    public ngOnInit() {
        setTimeout(() => {
            this.renderer.invokeElementMethod(this.elementRef.nativeElement, 'focus', []);

            if (this.select) {
                this.renderer.invokeElementMethod(this.elementRef.nativeElement, 'select', []);
            }
        });
    }
}