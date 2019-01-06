/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Directive, ElementRef, Input, Renderer2 } from '@angular/core';

@Directive({
    selector: '[sqxExternalLink]'
})
export class ExternalLinkDirective implements AfterViewInit {
    @Input('sqxExternalLink')
    public type?: string;

    constructor(
        private readonly element: ElementRef<Element>,
        private readonly renderer: Renderer2
    ) {
    }

    public ngAfterViewInit() {
        this.renderer.setAttribute(this.element.nativeElement, 'target', '_blank');
        this.renderer.setAttribute(this.element.nativeElement, 'rel', 'noopener');

        if (this.type !== 'noicon') {
            const icon = this.renderer.createElement('i');

            this.renderer.addClass(icon, 'icon-external-link');
            this.renderer.addClass(icon, 'ml-1');

            this.renderer.appendChild(this.element.nativeElement, icon);
        }
    }
}