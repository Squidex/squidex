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
})
export class MarkdownDirective {
    @Input('sqxMarkdown')
    public markdown!: string | undefined;

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
        let markdown = this.markdown || '';

        const hasExclamation = markdown.indexOf('!') === 0;

        // Optional markdown without exclamation is plain text and must never be interpreted as HTML.
        if (this.optional && !hasExclamation) {
            this.renderText(markdown);
            return;
        }

        if (this.optional) {
            markdown = markdown.substring(1);
        }

        const html = markdownRender(markdown, this.inline, this.trusted);
        const hasHtml = html.indexOf('<') >= 0 || html.indexOf('&') >= 0;

        if (hasHtml) {
            this.renderer.setProperty(this.element.nativeElement, 'innerHTML', html);
        } else {
            this.renderText(html);
        }
    }

    private renderText(text: string) {
        this.renderer.setProperty(this.element.nativeElement, 'textContent', text);
    }
}
