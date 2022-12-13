/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { MarkdownInlinePipe, MarkdownPipe } from './markdown.pipe';

describe('MarkdownInlinePipe', () => {
    const pipe = new MarkdownInlinePipe();

    it('should convert link to html', () => {
        const actual = pipe.transform('[link-name](link-url)');

        expect(actual).toBe('<a href="link-url" target="_blank", rel="noopener">link-name <i class="icon-external-link"></i></a>');
    });

    it('should convert markdown to html', () => {
        const actual = pipe.transform('*bold*');

        expect(actual).toBe('<em>bold</em>');
    });

    it('should escape input html', () => {
        const actual = pipe.transform('<h1>Header</h1>');

        expect(actual).toBe('&lt;h1&gt;Header&lt;/h1&gt;');
    });

    [null, undefined, ''].forEach(x => {
        it('should return empty string for invalid value', () => {
            const actual = pipe.transform(x);

            expect(actual).toBe('');
        });
    });
});

describe('MarkdownPipe', () => {
    const pipe = new MarkdownPipe();

    it('should convert link to html', () => {
        const actual = pipe.transform('[link-name](link-url)');

        expect(actual).toBe('<p><a href="link-url" target="_blank", rel="noopener">link-name <i class="icon-external-link"></i></a></p>\n');
    });

    it('should convert markdown to html', () => {
        const actual = pipe.transform('*bold*');

        expect(actual).toBe('<p><em>bold</em></p>\n');
    });

    it('should escape input html', () => {
        const actual = pipe.transform('<h1>Header</h1>');

        expect(actual).toBe('<p>&lt;h1&gt;Header&lt;/h1&gt;</p>\n');
    });

    [null, undefined, ''].forEach(x => {
        it('should return empty string for invalid value', () => {
            const actual = pipe.transform(x);

            expect(actual).toBe('');
        });
    });
});
