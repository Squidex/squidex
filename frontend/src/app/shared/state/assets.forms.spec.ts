/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { AnnotateAssetDto } from '../model';
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
        form = TestBed.runInInjectionContext(() => new AnnotateAssetForm());
    });

    it('shoulde remove extension if loading asset file name', () => {
        form.load(asset);

        const slug = form.model().fileName;

        expect(slug).toBe('My File');
    });

    it('should create slug from file name', () => {
        form.load(asset);

        form.generateSlug({} as any);

        const slug = form.model().slug;

        expect(slug).toBe('my-file');
    });

    it('should create slug from file name and append extension', () => {
        form.form.fileName().value.set('My New File');

        form.generateSlug(asset);

        const slug = form.model().slug;

        expect(slug).toBe('my-new-file.png');
    });

    it('should create slug from file with dots', () => {
        form.form.fileName().value.set('My.New.File');

        form.generateSlug(asset);

        const slug = form.model().slug;

        expect(slug).toBe('my.new.file.png');
    });

    it('should convert metadata if loading', () => {
        form.load(asset);

        const metadata = form.model().metadata.map(({ name, value }) => ({ name, value }));

        expect(metadata).toEqual([
            { name: 'key1', value: '' },
            { name: 'key2', value: 'String' },
            { name: 'key3', value: '13' },
            { name: 'key4', value: 'true' },
        ]);
    });

    it('should convert values if submitting', async () => {
        form.load(asset);

        let request = null as AnnotateAssetDto | null;
        let isDisabled = false;

        await form.submitChanges({ fileName: 'Old File.png' } as any, async x => {
            request = x;
            isDisabled = form.form().disabled();
        });

        expect(request!.toJSON()).toEqual(asset);
        expect(isDisabled).toBeTruthy();
    });

    it('should return null if nothing changed before submit', async () => {
        form.load(asset);

        let request = undefined as AnnotateAssetDto | null | undefined;

        await form.submitChanges(asset, async x => {
            request = x;
        });

        expect(request).toBeNull();
        expect(form.form().disabled()).toBeFalsy();
    });

    it('should remove previous metadata if loaded', () => {
        const newAsset: any = {
            metadata: {
                key1: 'Value',
            },
        };

        form.load(newAsset);

        const metadata = form.model().metadata.map(({ name, value }) => ({ name, value }));

        expect(metadata).toEqual([
            { name: 'key1', value: 'Value' },
        ]);
    });

    it('should add and remove metadata', () => {
        form.load(asset);
        form.addMetadata();
        form.removeMetadata(0);

        const metadata = form.model().metadata.map(({ name, value }) => ({ name, value }));

        expect(metadata).toEqual([
            { name: 'key2', value: 'String' },
            { name: 'key3', value: '13' },
            { name: 'key4', value: 'true' },
            { name: '', value: '' },
        ]);
    });

    it('should not submit if metadata name is missing', async () => {
        form.load(asset);
        form.addMetadata();

        const action = vi.fn(async () => {});

        const result = await form.submitChanges(asset, action);

        expect(result).toBeFalsy();
        expect(action).not.toHaveBeenCalled();
        expect(form.form.metadata[4].name().errors().length).toBe(1);
    });
});
