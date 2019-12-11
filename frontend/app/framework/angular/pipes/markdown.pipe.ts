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
    return `<a href="${href}" target="_blank", rel="noopener">${text} <i class="icon-external-link"></i></a>`;
};

@Pipe({
    name: 'sqxMarkdown',
    pure: true
})
export class MarkdownPipe implements PipeTransform {
    public transform(text: string | null | undefined): string {
        if (text) {
            return marked(text, { renderer });
        } else {
            return '';
        }
    }
}