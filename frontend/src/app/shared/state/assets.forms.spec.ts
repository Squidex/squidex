/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AnnotateAssetForm } from './assets.forms';

describe('AnnotateAssetForm', () => {
    let form: AnnotateAssetForm;

    const asset: any = {
        slug: 'my-file.png',
        tags: [
            'Tag1',
            'Tag2',
        ],
        isProtected: false,
        metadata: {
            key1: null,
            key2: 'String',
            key3: 13,
            key4: true,
        },
        fileName: 'My File.png',
    };

    beforeEach(() => {
        form = new AnnotateAssetForm();
    });

    it('shoulde remove extension if loading asset file name', () => {
        form.load(asset);

        const slug = form.form.get('fileName')!.value;

        expect(slug).toBe('My File');
    });

    it('should create slug from file name', () => {
        form.load(asset);

        form.generateSlug({} as any);

        const slug = form.form.get('slug')!.value;

        expect(slug).toBe('my-file');
    });

    it('should create slug from file name and append extension', () => {
        form.form.get('fileName')!.setValue('My New File');

        form.generateSlug(asset);

        const slug = form.form.get('slug')!.value;

        expect(slug).toBe('my-new-file.png');
    });

    it('should convert metadata if loading', () => {
        form.load(asset);

        const metadata = form.metadata.value;

        expect(metadata).toEqual([
            { name: 'key1', value: '' },
            { name: 'key2', value: 'String' },
            { name: 'key3', value: '13' },
            { name: 'key4', value: 'true' },
        ]);
    });

    it('should convert values if submitting', () => {
        form.load(asset);

        const request = form.submit({ fileName: 'Old File.png' } as any)!;

        expect(request).toEqual(asset);
        expect(form.form.enabled).toBeFalsy();
    });

    it('should return null if nothing changed before submit', () => {
        form.load(asset);

        const result = form.submit(asset);

        expect(result).toBeNull();
        expect(form.form.enabled).toBeTruthy();
    });

    it('should remove previous metadata if loaded', () => {
        const newAsset: any = {
            metadata: {
                key1: 'Value',
            },
        };

        form.load(newAsset);

        const metadata = form.metadata.value;

        expect(metadata).toEqual([
            { name: 'key1', value: 'Value' },
        ]);
    });
});
