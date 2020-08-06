/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { MarkdownPipe } from './markdown.pipe';

describe('MarkdownPipe', () => {
    it('should convert link to html', () => {
        const actual = new MarkdownPipe().transform('[link-name](link-url)');

        expect(actual).toBe('<a href="link-url" target="_blank", rel="noopener">link-name <i class="icon-external-link"></i></a>');
    });

    it('should convert markdown to html', () => {
        const actual = new MarkdownPipe().transform('*bold*');

        expect(actual).toBe('<em>bold</em>');
    });

    [null, undefined, ''].map(x => {
        it('should return empty string for invalid value', () => {
            const actual = new MarkdownPipe().transform(x);

            expect(actual).toBe('');
        });
    });
});