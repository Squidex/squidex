/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AppSettingsDto, EditorDto } from '../model';
import { computeEditorUrl } from './editor-utils';

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
});
