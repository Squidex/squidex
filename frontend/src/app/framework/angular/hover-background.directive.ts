/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, HostListener, Input, Renderer2 } from '@angular/core';

@Directive({
    selector: '[sqxHoverBackground]',
})
export class HoverBackgroundDirective {
    private previousBackground?: string | null;

    @Input('sqxHoverBackground')
    public background!: string;

    constructor(
        private readonly element: ElementRef<HTMLElement>,
        private readonly renderer: Renderer2,
    ) {
    }

    @HostListener('mouseenter')
    public onEnter() {
        this.previousBackground = this.element.nativeElement.style.background;

        this.renderer.setStyle(this.element.nativeElement, 'background', this.background);
    }

    @HostListener('mouseleave')
    public onLEave() {
        this.renderer.setStyle(this.element.nativeElement, 'background', this.previousBackground);
    }
}
