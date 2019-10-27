/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';
import Marked from 'marked';

const renderer = new Marked.Renderer();

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
            return Marked(text, { renderer });
        } else {
            return '';
        }
    }
}