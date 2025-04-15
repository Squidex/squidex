/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable no-template-curly-in-string */

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
                name: 'editor2',
                url: 'url/to/editor2',
            }),
            new EditorDto({
                name: 'editor3',
                url: 'url/to/editor3',
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

    [null, undefined, ''].forEach(url => {
        it(`should return empty string if url is ${url}`, () => {
            const result = computeEditorUrl(url, settings);

            expect(result).toEqual('');
        });
    });
});
