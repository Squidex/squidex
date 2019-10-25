/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';
import * as Marked from 'marked';

const renderer = new Marked.Renderer();

renderer.link = (href, _, text) => {
    if (!href.startsWith('http')) {
        href = `https://docs.squidex.io/${href}`;
    }

    return `<a href="${href}" target="_blank", rel="noopener">${text} <i class="icon-external-link"></i></a>`;
};

@Pipe({
    name: 'sqxHelpMarkdown',
    pure: true
})
export class HelpMarkdownPipe implements PipeTransform {
    public transform(text: string | null | undefined): string {
        if (text) {
            return Marked(text, { renderer });
        } else {
            return '';
        }
    }
}