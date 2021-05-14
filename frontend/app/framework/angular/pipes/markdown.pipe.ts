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
    if (href && href.startsWith('mailto')) {
        return text;
    } else {
        return `<a href="${href}" target="_blank", rel="noopener">${text} <i class="icon-external-link"></i></a>`;
    }
};

const inlinerRenderer = new marked.Renderer();

inlinerRenderer.paragraph = (text) => {
    return text;
};

inlinerRenderer.link = renderer.link;

@Pipe({
    name: 'sqxMarkdown',
    pure: true,
})
export class MarkdownPipe implements PipeTransform {
    public transform(text: string | undefined | null): string {
        if (text) {
            return marked(text, { renderer });
        } else {
            return '';
        }
    }
}

@Pipe({
    name: 'sqxMarkdownInline',
    pure: true,
})
export class MarkdownInlinePipe implements PipeTransform {
    public transform(text: string | undefined | null): string {
        if (text) {
            return marked(text, { renderer: inlinerRenderer });
        } else {
            return '';
        }
    }
}
