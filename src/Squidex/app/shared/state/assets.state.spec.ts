/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    AssetsDto,
    AssetsService,
    AssetsState,
    DialogService,
    versioned
} from '@app/shared/internal';

import { createAsset } from './../services/assets.service.spec';

import { TestValues } from './_test-helpers';

describe('AssetsState', () => {
    const {
        app,
        appsState,
        newVersion,
        version
    } = TestValues;

    const asset1 = createAsset(1, ['tag1', 'shared']);
    const asset2 = createAsset(2, ['tag2', 'shared']);

    const newAsset = createAsset(3, ['new']);

    let dialogs: IMock<DialogService>;
    let assetsService: IMock<AssetsService>;
    let assetsState: AssetsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        assetsService = Mock.ofType<AssetsService>();
        assetsService.setup(x => x.getTags(app))
            .returns(() => of({ tag1: 1, shared: 2, tag2: 1 })).verifiable(Times.atLeastOnce());

        assetsState = new AssetsState(appsState.object, assetsService.object, dialogs.object);
    });

    afterEach(() => {
        assetsService.verifyAll();
    });

    describe('Loading', () => {
        it('should load assets', () => {
            assetsService.setup(x => x.getAssets(app, 30, 0, undefined, It.isValue([])))
                .returns(() => of(new AssetsDto(200, [asset1, asset2]))).verifiable();

            assetsState.load().subscribe();

            expect(assetsState.snapshot.assets.values).toEqual([asset1, asset2]);
            expect(assetsState.snapshot.assetsPager.numberOfItems).toEqual(200);
            expect(assetsState.snapshot.isLoaded).toBeTruthy();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should show notification on load when reload is true', () => {
            assetsService.setup(x => x.getAssets(app, 30, 0, undefined, It.isValue([])))
                .returns(() => of(new AssetsDto(200, [asset1, asset2]))).verifiable();

            assetsState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

        it('should load with tags when tag toggled', () => {
            assetsService.setup(x => x.getAssets(app, 30, 0, undefined, It.isValue(['tag1'])))
                .returns(() => of(new AssetsDto(0, []))).verifiable();

            assetsState.toggleTag('tag1').subscribe();

            expect(assetsState.isTagSelected('tag1')).toBeTruthy();
        });

        it('should load without tags when tag untoggled', () => {
            assetsService.setup(x => x.getAssets(app, 30, 0, undefined, It.isValue(['tag1'])))
                .returns(() => of(new AssetsDto(0, []))).verifiable();

            assetsService.setup(x => x.getAssets(app, 30, 0, undefined, It.isValue([])))
                .returns(() => of(new AssetsDto(0, []))).verifiable();

            assetsState.toggleTag('tag1').subscribe();
            assetsState.toggleTag('tag1').subscribe();

            expect(assetsState.isTagSelected('tag1')).toBeFalsy();
        });

        it('should load with tags when tags selected', () => {
            assetsService.setup(x => x.getAssets(app, 30, 0, undefined, It.isValue(['tag1', 'tag2'])))
                .returns(() => of(new AssetsDto(0, []))).verifiable();

            assetsState.selectTags(['tag1', 'tag2']).subscribe();

            expect(assetsState.isTagSelected('tag1')).toBeTruthy();
        });

        it('should load without tags when tags reset', () => {
            assetsService.setup(x => x.getAssets(app, 30, 0, undefined, It.isValue([])))
                .returns(() => of(new AssetsDto(0, []))).verifiable();

            assetsState.resetTags().subscribe();

            expect(assetsState.isTagSelectionEmpty()).toBeTruthy();
        });

        it('should load next page and prev page when paging', () => {
            assetsService.setup(x => x.getAssets(app, 30, 0, undefined, It.isValue([])))
                .returns(() => of(new AssetsDto(200, []))).verifiable(Times.exactly(2));

            assetsService.setup(x => x.getAssets(app, 30, 30, undefined, It.isValue([])))
                .returns(() => of(new AssetsDto(200, []))).verifiable();

            assetsState.load().subscribe();
            assetsState.goNext().subscribe();
            assetsState.goPrev().subscribe();

            expect().nothing();
        });

        it('should load with query when searching', () => {
            assetsService.setup(x => x.getAssets(app, 30, 0, 'my-query', It.isValue([])))
                .returns(() => of(new AssetsDto(0, []))).verifiable();

            assetsState.search('my-query').subscribe();

            expect(assetsState.snapshot.assetsQuery).toEqual('my-query');
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            assetsService.setup(x => x.getAssets(app, 30, 0, undefined, It.isValue([])))
                .returns(() => of(new AssetsDto(200, [asset1, asset2]))).verifiable();

            assetsState.load(true).subscribe();
        });

        it('should add asset to snapshot when created', () => {
            assetsState.add(newAsset);

            expect(assetsState.snapshot.assets.values).toEqual([newAsset, asset1, asset2]);
            expect(assetsState.snapshot.assetsPager.numberOfItems).toBe(201);
        });

        it('should increment tags when asset added', () => {
            assetsState.add(newAsset);
            assetsState.add(newAsset);

            expect(assetsState.snapshot.tags).toEqual({ tag1: 1, tag2: 1, shared: 2, new: 2 });
        });

        it('should update asset when updated', () => {
            const update = createAsset(1, ['new'], '_new');

            assetsState.update(update);

            const newAsset1 = assetsState.snapshot.assets.at(0);

            expect(newAsset1).toEqual(update);
            expect(assetsState.snapshot.tags).toEqual({ tag2: 1, shared: 1, new: 1 });
        });

        it('should remove asset from snapshot when deleted', () => {
            assetsService.setup(x => x.deleteAsset(app, asset1, version))
                .returns(() => of(versioned(newVersion)));

            assetsState.delete(asset1).subscribe();

            expect(assetsState.snapshot.assets.values.length).toBe(1);
            expect(assetsState.snapshot.assetsPager.numberOfItems).toBe(199);
            expect(assetsState.snapshot.tags).toEqual({ shared: 1, tag2: 1 });
        });
    });
});