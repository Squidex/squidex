/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Directive, effect, ElementRef, inject, input, Renderer2 } from '@angular/core';
import { markdownRender } from '@app/framework/internal';

@Directive({
    selector: '[sqxMarkdown]',
})
export class MarkdownDirective {
    private readonly element = inject(ElementRef);
    private readonly renderer = inject(Renderer2);

    /** The markdown to render. */
    public readonly markdown = input<string | undefined | null>(undefined, { alias: 'sqxMarkdown' });

    /** Renders HTML in the markdown instead of escaping it. */
    public readonly trusted = input(false, { transform: booleanAttribute });

    /** Renders the markdown without a wrapping paragraph. */
    public readonly inline = input(true, { transform: booleanAttribute });

    /** Renders the text as plain text, unless it starts with an exclamation mark. */
    public readonly optional = input(false, { transform: booleanAttribute });

    constructor() {
        effect(() => {
            this.render();
        });
    }

    private render() {
        let markdown = this.markdown() || '';

        const hasExclamation = markdown.indexOf('!') === 0;

        // Optional markdown without exclamation is plain text and must never be interpreted as HTML.
        if (this.optional() && !hasExclamation) {
            this.renderText(markdown);
            return;
        }

        if (this.optional()) {
            markdown = markdown.substring(1);
        }

        const html = markdownRender(markdown, this.inline(), this.trusted());
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
