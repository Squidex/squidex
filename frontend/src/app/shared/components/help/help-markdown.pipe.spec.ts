/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HelpMarkdownPipe } from './help-markdown.pipe';

describe('MarkdownPipe', () => {
    it('should convert absolute link to html', () => {
        const actual = new HelpMarkdownPipe().transform('[link-name](https://squidex.io)');

        expect(actual).toBe('<p><a href="https://squidex.io" target="_blank", rel="noopener">link-name <i class="icon-external-link"></i></a></p>\n');
    });

    it('should convert relative link to html', () => {
        const actual = new HelpMarkdownPipe().transform('[link-name](link-url)');

        expect(actual).toBe('<p><a href="https://docs.squidex.io/link-url" target="_blank", rel="noopener">link-name <i class="icon-external-link"></i></a></p>\n');
    });

    it('should convert markdown to html', () => {
        const actual = new HelpMarkdownPipe().transform('*bold*');

        expect(actual).toBe('<p><em>bold</em></p>\n');
    });

    [null, undefined, ''].forEach(x => {
        it('should return empty string for invalid value', () => {
            const actual = new HelpMarkdownPipe().transform(x);

            expect(actual).toBe('');
        });
    });
});
