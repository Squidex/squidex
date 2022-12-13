/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, Input, OnChanges, Renderer2 } from '@angular/core';
import { renderMarkdown } from '@app/framework/internal';

@Directive({
    selector: '[sqxMarkdown]',
})
export class MarkdownDirective implements OnChanges {
    @Input('sqxMarkdown')
    public markdown!: string;

    @Input()
    public inline = true;

    @Input()
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

        if (hasExclamation) {
            markdown = markdown.substring(1);
        }

        if (!markdown) {
            html = markdown;
        } else if (this.optional && !hasExclamation) {
            html = markdown;
        } else if (this.markdown) {
            html = renderMarkdown(markdown, this.inline);
        }

        const hasHtml = html.indexOf('<') >= 0;

        if (hasHtml) {
            this.renderer.setProperty(this.element.nativeElement, 'innerHTML', html);
        } else {
            this.renderer.setProperty(this.element.nativeElement, 'textContent', html);
        }
    }
}
