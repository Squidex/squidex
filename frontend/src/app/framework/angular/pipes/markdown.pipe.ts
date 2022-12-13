/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';
import { renderMarkdown } from '@app/framework/internal';

@Pipe({
    name: 'sqxMarkdown',
    pure: true,
})
export class MarkdownPipe implements PipeTransform {
    public transform(text: string | undefined | null): string {
        return renderMarkdown(text, false);
    }
}

@Pipe({
    name: 'sqxMarkdownInline',
    pure: true,
})
export class MarkdownInlinePipe implements PipeTransform {
    public transform(text: string | undefined | null): string {
        return renderMarkdown(text, true);
    }
}