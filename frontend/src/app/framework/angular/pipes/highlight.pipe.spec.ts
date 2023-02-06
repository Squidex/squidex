/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HighlightPipe } from './highlight.pipe';

describe('Hightlight', () => {
    const pipe = new HighlightPipe();

    [null, undefined, ''].forEach(search => {
        it('should not highlight if no search passed', () => {
            const input = 'Hello World';

            const result = pipe.transform(input, search);

            expect(result).toEqual(input);
        });
    });

    it('should encode html if no search specified', () => {
        const input = '<h1>Hello World</h1>';

        const result = pipe.transform(input, null);

        expect(result).toEqual('&lt;h1&gt;Hello World&lt;/h1&gt;');
    });

    it('should highlight with search string', () => {
        const input = 'Hello World';

        const result = pipe.transform(input, 'ello');

        expect(result).toEqual('H<b>ello</b> World');
    });

    it('should highlight with search regex', () => {
        const input = 'Hello World';

        const result = pipe.transform(input, new RegExp('ello', 'i'));

        expect(result).toEqual('H<b>ello</b> World');
    });

    it('should encode html if search specified', () => {
        const input = '<h1>Hello World</h1>';

        const result = pipe.transform(input, 'ello');

        expect(result).toEqual('&lt;h1&gt;H<b>ello</b> World&lt;/h1&gt;');
    });
});
