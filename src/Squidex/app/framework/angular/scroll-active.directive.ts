/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

@Ng2.Directive({
    selector: '[sqxScrollActive]'
})
export class ScrollActiveDirective implements Ng2.OnChanges {
    @Ng2.Input('sqxScrollActive')
    public isActive = false;

    @Ng2.Input()
    public container: HTMLElement;

    constructor(
        private readonly element: Ng2.ElementRef
    ) {
    }

    public ngOnChanges() {
        if (this.isActive && this.container) {
            this.scrollInView(this.container, this.element.nativeElement);
        }
    }

    private scrollInView(parent: HTMLElement, target: HTMLElement) {
        if (!parent.getBoundingClientRect) {
            return;
        }

        const parentRect = parent.getBoundingClientRect();
        const targetRect = target.getBoundingClientRect();

        const offset = (targetRect.top + document.body.scrollTop) - (parentRect.top + document.body.scrollTop);

        const scroll = parent.scrollTop;

        if (offset < 0) {
            parent.scrollTop = scroll + offset;
        } else {
            const targetHeight = targetRect.height;
            const parentHeight = parentRect.height;

            if ((offset + targetHeight) > parentHeight) {
                parent.scrollTop = scroll + offset - parentHeight + targetHeight;
            }
        }
    }
}