/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AppSettingsDto, EditorDto } from '../model';
import { computeEditorUrl, getSafeOrigin, isSafeUrl } from './editor-utils';

describe('EditorUtils', () => {
    const settings = new AppSettingsDto({
        patterns: [],
        hideDateTimeModeButton: false,
        hideScheduler: false,
        editors: [
            new EditorDto({
                name: 'editor1',
                url: 'url/to/editor1',
            }),
            new EditorDto({
                name: 'duplicate',
                url: 'url/to/duplicate1',
            }),
            new EditorDto({
                name: 'duplicate',
                url: 'url/to/duplicate2',
            }),
        ],
        version: 1,
        _links: {},
    });

    it('should interpolate editor url', () => {
        const result = computeEditorUrl('http://${editor1}?query=value', settings);

        expect(result).toEqual('http://url/to/editor1?query=value');
    });

    it('should interpolate editor url without dollar', () => {
        const result = computeEditorUrl('http://{editor1}?query=value', settings);

        expect(result).toEqual('http://url/to/editor1?query=value');
    });

    it('should interpolate editor url from second duplicate', () => {
        const result = computeEditorUrl('http://${duplicate}?query=value', settings);

        expect(result).toEqual('http://url/to/duplicate2?query=value');
    });

    it('should not interpolate if setting ist not defined', () => {
        const result = computeEditorUrl('http://${duplicate}?query=value', undefined);

        expect(result).toEqual('http://undefined?query=value');
    });

    it('should not interpolate if url is not found', () => {
        const result = computeEditorUrl('http://${not-found}?query=value', settings);

        expect(result).toEqual('http://undefined?query=value');
    });

    it('should keep relative editor url', () => {
        const result = computeEditorUrl('/editors/${editor1}', settings);

        expect(result).toEqual('/editors/url/to/editor1');
    });

    ['javascript:alert(1)', ' JavaScript:alert(1)', 'data:text/html,<script>alert(1)</script>', 'vbscript:msgbox(1)'].forEach(url => {
        it(`should return empty string if url has unsafe protocol ${url}`, () => {
            const result = computeEditorUrl(url, settings);

            expect(result).toEqual('');
        });
    });

    it('should return empty string if interpolated url has unsafe protocol', () => {
        const unsafeSettings = new AppSettingsDto({
            patterns: [],
            hideDateTimeModeButton: false,
            hideScheduler: false,
            editors: [
                new EditorDto({
                    name: 'evil',
                    url: 'javascript:alert(1)',
                }),
            ],
            version: 1,
            _links: {},
        });

        const result = computeEditorUrl('${evil}', unsafeSettings);

        expect(result).toEqual('');
    });

    [null, undefined, ''].forEach(url => {
        it(`should return empty string if url is ${url}`, () => {
            const result = computeEditorUrl(url, settings);

            expect(result).toEqual('');
        });
    });

    ['http://squidex.io/preview', 'https://squidex.io/preview?id=1', '/preview/1'].forEach(url => {
        it(`should treat url as safe ${url}`, () => {
            const result = isSafeUrl(url);

            expect(result).toBe(true);
        });
    });

    ['javascript:alert(1)', ' JavaScript:alert(1)', 'data:text/html,<script>alert(1)</script>', 'vbscript:msgbox(1)', '', null, undefined].forEach(url => {
        it(`should treat url as unsafe ${url}`, () => {
            const result = isSafeUrl(url);

            expect(result).toBe(false);
        });
    });

    it('should return origin of absolute url', () => {
        const result = getSafeOrigin('https://editor.squidex.io/path/editor.html?query=1');

        expect(result).toEqual('https://editor.squidex.io');
    });

    it('should return own origin of relative url', () => {
        const result = getSafeOrigin('/editors/editor.html');

        expect(result).toEqual(window.location.origin);
    });

    ['javascript:alert(1)', 'data:text/html,<script>alert(1)</script>', '', null, undefined].forEach(url => {
        it(`should return null as origin of unsafe url ${url}`, () => {
            const result = getSafeOrigin(url);

            expect(result).toBeNull();
        });
    });
});
