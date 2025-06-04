/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';
import { marked, Renderer, Tokens } from 'marked';

class HelpRenderer extends Renderer {
    public static readonly INSTANCE = new HelpRenderer();

    public link({ href, tokens }: Tokens.Link): string {
        const text = this.parser.parseInline(tokens);
        if (href && !href.startsWith('http')) {
            href = `https://docs.squidex.io/${href}`;
        }

        return `<a href="${href}" target="_blank", rel="noopener">${text} <i class="icon-external-link"></i></a>`;
    }
}

@Pipe({
    name: 'sqxHelpMarkdown',
    pure: true,
})
export class HelpMarkdownPipe implements PipeTransform {
    public transform(text: string | undefined | null): string {
        if (text) {
            return marked(text, { renderer: HelpRenderer.INSTANCE }) as string;
        } else {
            return '';
        }
    }
}
