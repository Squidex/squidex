/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { MarkdownInlinePipe, MarkdownPipe } from './markdown.pipe';

describe('MarkdownInlinePipe', () => {
    it('should convert link to html', () => {
        const actual = new MarkdownInlinePipe().transform('[link-name](link-url)');

        expect(actual).toBe('<a href="link-url" target="_blank", rel="noopener">link-name <i class="icon-external-link"></i></a>');
    });

    it('should convert markdown to html', () => {
        const actual = new MarkdownInlinePipe().transform('*bold*');

        expect(actual).toBe('<em>bold</em>');
    });

    [null, undefined, ''].forEach(x => {
        it('should return empty string for invalid value', () => {
            const actual = new MarkdownInlinePipe().transform(x);

            expect(actual).toBe('');
        });
    });
});

describe('MarkdownPipe', () => {
    it('should convert link to html', () => {
        const actual = new MarkdownPipe().transform('[link-name](link-url)');

        expect(actual).toBe('<p><a href="link-url" target="_blank", rel="noopener">link-name <i class="icon-external-link"></i></a></p>\n');
    });

    it('should convert markdown to html', () => {
        const actual = new MarkdownPipe().transform('*bold*');

        expect(actual).toBe('<p><em>bold</em></p>\n');
    });

    [null, undefined, ''].forEach(x => {
        it('should return empty string for invalid value', () => {
            const actual = new MarkdownPipe().transform(x);

            expect(actual).toBe('');
        });
    });
});
