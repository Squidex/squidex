/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, inject, input, Renderer2 } from '@angular/core';

@Directive({
    selector: '[sqxHoverBackground]',
    host: {
        '(mouseenter)': 'onEnter()',
        '(mouseleave)': 'onLeave()',
    },
})
export class HoverBackgroundDirective {
    private readonly element = inject<ElementRef<HTMLElement>>(ElementRef);
    private readonly renderer = inject(Renderer2);
    private previousBackground?: string | null;

    /** The background to apply while the mouse is over the element. */
    public readonly background = input.required<string>({ alias: 'sqxHoverBackground' });

    protected onEnter() {
        this.previousBackground = this.element.nativeElement.style.background;

        this.renderer.setStyle(this.element.nativeElement, 'background', this.background());
    }

    protected onLeave() {
        this.renderer.setStyle(this.element.nativeElement, 'background', this.previousBackground);
    }
}
