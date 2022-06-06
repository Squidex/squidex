/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, Input, OnChanges, Renderer2 } from '@angular/core';
import { marked } from 'marked';

const RENDERER_DEFAULT = new marked.Renderer();
const RENDERER_INLINE = new marked.Renderer();

RENDERER_DEFAULT.link = (href, _, text) => {
    if (href && href.startsWith('mailto')) {
        return text;
    } else {
        return `<a href="${href}" target="_blank", rel="noopener">${text} <i class="icon-external-link"></i></a>`;
    }
};

RENDERER_INLINE.paragraph = (text) => {
    return text;
};

RENDERER_INLINE.link = RENDERER_DEFAULT.link;

@Directive({
    selector: '[sqxMarkdown]',
})
export class MarkdownDirective implements OnChanges {
    @Input('sqxMarkdown')
    public markdown!: string;

    @Input()
    public inline = true;

    @Input()
    public html = false;

    @Input()
    public optional = false;

    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer2,
    ) {
    }

    public ngOnChanges() {
        let html = '';

        const markdown = this.markdown;

        if (!markdown) {
            html = markdown;
        } else if (this.optional && markdown.indexOf('!') !== 0) {
            html = markdown;
        } else if (this.markdown) {
            const renderer = this.inline ? RENDERER_INLINE : RENDERER_DEFAULT;

            html = marked(this.markdown, { renderer });
        }

        if (!this.html && (!html || html === this.markdown || html.indexOf('<') < 0)) {
            this.renderer.setProperty(this.element.nativeElement, 'textContent', html);
        } else {
            this.renderer.setProperty(this.element.nativeElement, 'innerHTML', html);
        }
    }
}
