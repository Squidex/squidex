/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Directive, ElementRef, Input, Renderer2 } from '@angular/core';

@Directive({
    selector: '[sqxExternalLink]',
})
export class ExternalLinkDirective implements AfterViewInit {
    @Input('sqxExternalLink')
    public type?: string;

    constructor(
        private readonly element: ElementRef<Element>,
        private readonly renderer: Renderer2,
    ) {
    }

    public ngAfterViewInit() {
        const element = this.element.nativeElement;

        this.renderer.setProperty(element, 'target', '_blank');
        this.renderer.setProperty(element, 'rel', 'noopener');
        this.renderer.addClass(element, 'external');

        if (this.type !== 'noicon') {
            const icon = this.renderer.createElement('i');

            this.renderer.addClass(icon, 'icon-external-link');

            this.renderer.appendChild(element, this.renderer.createText(' '));
            this.renderer.appendChild(element, icon);
        }
    }
}
