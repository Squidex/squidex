/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { markdownRender } from './markdown';

describe('Markdown', () => {
    it('should render text', () => {
        const md = 'Text';

        const result = markdownRender(md, false);

        expect(result).toEqual('<p>Text</p>\n');
    });

    it('should render text inline', () => {
        const md = 'Text';

        const result = markdownRender(md, true);

        expect(result).toEqual('Text');
    });

    it('should render escaped', () => {
        const md = '<h1>Header</h1>';

        const result = markdownRender(md, false);

        expect(result).toEqual('<p>&lt;h1&gt;Header&lt;/h1&gt;</p>\n');
    });

    it('should render non escaped', () => {
        const md = '<h1>Header</h1>';

        const result = markdownRender(md, false, true);

        expect(result).toEqual('<h1>Header</h1>');
    });

    it('should render mailto link', () => {
        const md = '[mail](mailto:hello@squidex.io)';

        const result = markdownRender(md, true);

        expect(result).toEqual('<a href="mailto:hello@squidex.io">mail</a>');
    });

    it('should render normal link', () => {
        const md = '[squidex](https://squidex.io)';

        const result = markdownRender(md, true);

        expect(result).toEqual('<a href="https://squidex.io" target="_blank", rel="noopener">squidex <i class="icon-external-link"></i></a>');
    });

    it('should render image', () => {
        const md = '![{name}](https://localhost:5001/ai-images/dall-e/ea68c867-6472-4d77-a526-7c9d9c4698fe)';

        const result = markdownRender(md, true);

        expect(result).toEqual('<img src="https://localhost:5001/ai-images/dall-e/ea68c867-6472-4d77-a526-7c9d9c4698fe" alt="{name}">');
    });
});