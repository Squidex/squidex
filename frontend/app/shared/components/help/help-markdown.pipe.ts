/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';
import marked from 'marked';

const renderer = new marked.Renderer();

renderer.link = (href, _, text) => {
    if (href && !href.startsWith('http')) {
        href = `https://docs.squidex.io/${href}`;
    }

    return `<a href="${href}" target="_blank", rel="noopener">${text} <i class="icon-external-link"></i></a>`;
};

@Pipe({
    name: 'sqxHelpMarkdown',
    pure: true,
})
export class HelpMarkdownPipe implements PipeTransform {
    public transform(text: string | undefined | null): string {
        if (text) {
            return marked(text, { renderer });
        } else {
            return '';
        }
    }
}
