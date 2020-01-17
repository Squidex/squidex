/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder } from '@angular/forms';

import { AnnotateAssetForm } from './assets.forms';

describe('AnnotateAssetForm', () => {
    let form: AnnotateAssetForm;

    const asset: any = {
        slug: 'my-file.png',
        tags: [
            'Tag1',
            'Tag2'
        ],
        isProtected: false,
        metadata: {
            key1: null,
            key2: 'String',
            key3: 13,
            key4: true
        },
        fileName: 'My File.png'
    };

    beforeEach(() => {
        form = new AnnotateAssetForm(new FormBuilder());
    });

    it('Should remov extension when loading asset file name', () => {
        form.load(asset);

        const slug = form.form.get('fileName')!.value;

        expect(slug).toBe('My File');
    });

    it('Should create slug from file name', () => {
        form.load(asset);
        form.generateSlug({} as any);

        const slug = form.form.get('slug')!.value;

        expect(slug).toBe('my-file');
    });

    it('Should create slug from file name and append extension', () => {
        form.form.get('fileName')!.setValue('My New File');
        form.generateSlug(asset);

        const slug = form.form.get('slug')!.value;

        expect(slug).toBe('my-new-file.png');
    });

    it('Should convert metadata when loading', () => {
        form.load(asset);

        const metadata = form.metadata.value;

        expect(metadata).toEqual([
            { name: 'key1', value: '' },
            { name: 'key2', value: 'String' },
            { name: 'key3', value: '13' },
            { name: 'key4', value: 'true' }
        ]);
    });

    it('Should convert values when submitting', () => {
        form.load(asset);

        const request = form.submit({ fileName: 'Old File.png' } as any)!;

        expect(request).toEqual(asset);
        expect(form.form.enabled).toBeFalsy();
    });

    it('Should return null when nothing changed before submit', () => {
        form.load(asset);

        const result = form.submit(asset);

        expect(result).toBeNull();
        expect(form.form.enabled).toBeTruthy();
    });

    it('Should remove previous metadata when loaded', () => {
        const newAsset: any = {
            metadata: {
                key1: 'Value'
            }
        };

        form.load(newAsset);

        const metadata = form.metadata.value;

        expect(metadata).toEqual([
            { name: 'key1', value: 'Value' }
        ]);
    });
});