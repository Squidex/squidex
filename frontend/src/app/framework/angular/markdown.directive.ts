/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Directive, ElementRef, Input, Renderer2 } from '@angular/core';
import { markdownRender } from '@app/framework/internal';

@Directive({
    selector: '[sqxMarkdown]',
    standalone: true,
})
export class MarkdownDirective {
    @Input('sqxMarkdown')
    public markdown!: string;

    @Input({ transform: booleanAttribute })
    public trusted = false;

    @Input({ transform: booleanAttribute })
    public inline = true;

    @Input({ transform: booleanAttribute })
    public optional = false;

    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer2,
    ) {
    }

    public ngOnChanges() {
        let html = '';

        let markdown = this.markdown;

        const hasExclamation = markdown.indexOf('!') === 0;

        if (hasExclamation && this.optional) {
            markdown = markdown.substring(1);
        }

        if (!markdown) {
            html = markdown;
        } else if (this.optional && !hasExclamation) {
            html = markdown;
        } else if (this.markdown) {
            html = markdownRender(markdown, this.inline, this.trusted);
        }

        const hasHtml = html.indexOf('<') >= 0 || html.indexOf('&') >= 0;

        if (hasHtml) {
            this.renderer.setProperty(this.element.nativeElement, 'innerHTML', html);
        } else {
            this.renderer.setProperty(this.element.nativeElement, 'textContent', html);
        }
    }
}
