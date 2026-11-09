/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { afterEveryRender, Directive, ElementRef, inject, input, numberAttribute, Renderer2 } from '@angular/core';

@Directive({
    selector: 'textarea[sqxAutosize]',
    host: {
        '[style.overflow]': '"hidden"',
        '[rows]': 'rows()',
        '(input)': 'resize()',
    },
})
export class AutosizeDirective {
    private readonly element = inject<ElementRef<HTMLTextAreaElement>>(ElementRef);
    private readonly renderer = inject(Renderer2);

    /** The minimum number of rows. */
    public readonly rows = input(1, { transform: numberAttribute });

    constructor() {
        afterEveryRender(() => {
            this.resize();
        });
    }

    protected resize() {
        const textarea = this.element.nativeElement;

        // Calculate border height which is not included in scrollHeight
        const borderHeight = textarea.offsetHeight - textarea.clientHeight;

        this.setHeight(textarea, 'auto');
        this.setHeight(textarea, `${textarea.scrollHeight + borderHeight}px`);
    }

    private setHeight(element: HTMLTextAreaElement, value: string) {
        this.renderer.setStyle(element, 'height', value);
    }
}
