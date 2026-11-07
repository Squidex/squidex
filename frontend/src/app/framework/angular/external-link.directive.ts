/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { afterNextRender, Directive, ElementRef, inject, input, Renderer2 } from '@angular/core';

@Directive({
    selector: '[sqxExternalLink]',
})
export class ExternalLinkDirective {
    private readonly element = inject<ElementRef<Element>>(ElementRef);
    private readonly renderer = inject(Renderer2);

    /** Use "noicon" to hide the external link icon. */
    public readonly type = input<'noicon' | 'default' | string | undefined>(undefined, { alias: 'sqxExternalLink' });

    constructor() {
        afterNextRender(() => {
            const element = this.element.nativeElement;

            this.renderer.setProperty(element, 'target', '_blank');
            this.renderer.setProperty(element, 'rel', 'noopener');
            this.renderer.addClass(element, 'external');

            if (this.type() !== 'noicon') {
                const icon = this.renderer.createElement('i');

                this.renderer.addClass(icon, 'icon-external-link');

                this.renderer.appendChild(element, this.renderer.createText(' '));
                this.renderer.appendChild(element, icon);
            }
        });
    }
}
