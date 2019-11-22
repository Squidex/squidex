/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    AssetFoldersDto,
    AssetsDto,
    AssetsService,
    AssetsState,
    DialogService,
    LocalStoreService,
    MathHelper,
    Pager,
    versioned
} from '@app/shared/internal';

import { createAsset, createAssetFolder } from './../services/assets.service.spec';

import { TestValues } from './_test-helpers';
import { encodeQuery } from './query';

describe('AssetsState', () => {
    const {
        app,
        appsState,
        newVersion,
        version
    } = TestValues;

    const asset1 = createAsset(1, ['tag1', 'shared']);
    const asset2 = createAsset(2, ['tag2', 'shared']);

    const folder1 = createAssetFolder(1);
    const folder2 = createAssetFolder(1);

    let dialogs: IMock<DialogService>;
    let assetsService: IMock<AssetsService>;
    let assetsState: AssetsState;
    let localStore: IMock<LocalStoreService>;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        localStore = Mock.ofType<LocalStoreService>();
        localStore.setup(x => x.getInt('assets.pageSize', 30))
            .returns(() => 30);

        assetsService = Mock.ofType<AssetsService>();
        assetsService.setup(x => x.getTags(app))
            .returns(() => of({ tag1: 1, shared: 2, tag2: 1 }));

        assetsState = new AssetsState(appsState.object, assetsService.object, dialogs.object, localStore.object);
    });

    afterEach(() => {
        assetsService.verifyAll();
    });

    describe('Loading', () => {
        beforeEach(() => {
            assetsService.setup(x => x.getAssetFolders(app, MathHelper.EMPTY_GUID))
                .returns(() => of(new AssetFoldersDto(2, [folder1, folder2]))).verifiable(Times.atLeastOnce());
        });

        it('should load assets', () => {
            assetsService.setup(x => x.getAssets(app, 30, 0, undefined, It.isValue([]), undefined, MathHelper.EMPTY_GUID))
                .returns(() => of(new AssetsDto(200, [asset1, asset2]))).verifiable();

            assetsState.load().subscribe();

            expect(assetsState.snapshot.assets).toEqual([asset1, asset2]);
            expect(assetsState.snapshot.assetsPager.numberOfItems).toEqual(200);
            expect(assetsState.snapshot.isLoaded).toBeTruthy();
            expect(assetsState.snapshot.isLoading).toBeFalsy();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should show notification on load when reload is true', () => {
            assetsService.setup(x => x.getAssets(app, 30, 0, undefined, It.isValue([]), undefined, MathHelper.EMPTY_GUID))
                .returns(() => of(new AssetsDto(200, [asset1, asset2]))).verifiable();

            assetsState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

        it('should load without tags when tag untoggled', () => {
            assetsService.setup(x => x.getAssets(app, 30, 0, undefined, It.isValue(['tag1']), undefined, undefined))
                .returns(() => of(new AssetsDto(0, []))).verifiable();

            assetsService.setup(x => x.getAssets(app, 30, 0, undefined, It.isValue([]), undefined, MathHelper.EMPTY_GUID))
                .returns(() => of(new AssetsDto(0, []))).verifiable();

            assetsState.toggleTag('tag1').subscribe();
            assetsState.toggleTag('tag1').subscribe();

            expect(assetsState.isTagSelected('tag1')).toBeFalsy();
        });

        it('should load without tags when tags reset', () => {
            assetsService.setup(x => x.getAssets(app, 30, 0, undefined, It.isValue([]), undefined, MathHelper.EMPTY_GUID))
                .returns(() => of(new AssetsDto(0, []))).verifiable();

            assetsState.resetTags().subscribe();

            expect(assetsState.isTagSelectionEmpty()).toBeTruthy();
        });

        it('should load with new pagination when paging', () => {
            assetsService.setup(x => x.getAssets(app, 30, 30, undefined, It.isValue([]), undefined, MathHelper.EMPTY_GUID))
                .returns(() => of(new AssetsDto(200, []))).verifiable();

            assetsState.setPager(new Pager(200, 1, 30)).subscribe();

            expect().nothing();
        });

        it('should update page size in local store', () => {
            assetsService.setup(x => x.getAssets(app, 50, 0, undefined, It.isValue([]), undefined, MathHelper.EMPTY_GUID))
                .returns(() => of(new AssetsDto(200, []))).verifiable();

            assetsState.setPager(new Pager(0, 0, 50));

            localStore.verify(x => x.setInt('assets.pageSize', 50), Times.atLeastOnce());

            expect().nothing();
        });
    });

    describe('Searching', () => {
        it('should load with tags when tag toggled', () => {
            assetsService.setup(x => x.getAssets(app, 30, 0, undefined, It.isValue(['tag1']), undefined, undefined))
                .returns(() => of(new AssetsDto(0, []))).verifiable();

            assetsState.toggleTag('tag1').subscribe();

            expect(assetsState.isTagSelected('tag1')).toBeTruthy();
        });

        it('should load with tags when tags selected', () => {
            assetsService.setup(x => x.getAssets(app, 30, 0, undefined, It.isValue(['tag1', 'tag2']), undefined, undefined))
                .returns(() => of(new AssetsDto(0, []))).verifiable();

            assetsState.selectTags(['tag1', 'tag2']).subscribe();

            expect(assetsState.isTagSelected('tag1')).toBeTruthy();
        });

        it('should load with query when searching', () => {
            const query = { fullText: 'my-query' };

            assetsService.setup(x => x.getAssets(app, 30, 0, query, It.isValue([]), undefined, undefined))
                .returns(() => of(new AssetsDto(0, []))).verifiable();

            assetsState.search(query).subscribe();

            expect(assetsState.snapshot.assetsQuery).toEqual(query);
            expect(assetsState.isQueryUsed({ name: 'name', query, queryJson: encodeQuery(query) })).toBeTruthy();
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            assetsService.setup(x => x.getAssetFolders(app, MathHelper.EMPTY_GUID))
                .returns(() => of(new AssetFoldersDto(2, [folder1, folder2])));
            assetsService.setup(x => x.getAssets(app, 30, 0, undefined, It.isValue([]), undefined, MathHelper.EMPTY_GUID))
                .returns(() => of(new AssetsDto(200, [asset1, asset2]))).verifiable();

            assetsState.load(true).subscribe();
        });

        it('should add asset to snapshot when created', () => {
            const newAsset = createAsset(5, ['new']);

            assetsState.addAsset(newAsset);

            expect(assetsState.snapshot.assets).toEqual([newAsset, asset1, asset2]);
            expect(assetsState.snapshot.assetsPager.numberOfItems).toBe(201);
        });

        it('should not add asset to snapshot when parent id is not the same', () => {
            const newAsset = createAsset(5, ['new'], 'new', 'other-parent');

            assetsState.addAsset(newAsset);

            expect(assetsState.snapshot.assets).toEqual([asset1, asset2]);
            expect(assetsState.snapshot.assetsPager.numberOfItems).toBe(200);
        });

        it('should increment tags when asset added', () => {
            assetsState.addAsset(createAsset(5, ['new']));
            assetsState.addAsset(createAsset(6, ['new']));

            expect(assetsState.snapshot.tagsAvailable).toEqual({ tag1: 1, tag2: 1, shared: 2, new: 2 });
        });

        /*
        it('should update asset when updated', () => {
            const update = createAsset(1, ['new'], '_new');

            assetsState.update(update);

            const newAsset1 = assetsState.snapshot.assets[0];

            expect(newAsset1).toEqual(update);
            expect(assetsState.snapshot.tagsAvailable).toEqual({ tag2: 1, shared: 1, new: 1 });
        });
        */

        it('should remove asset from snapshot when deleted', () => {
            assetsService.setup(x => x.deleteAssetItem(app, asset1, version))
                .returns(() => of(versioned(newVersion)));

            assetsState.deleteAsset(asset1).subscribe();

            expect(assetsState.snapshot.assets.length).toBe(1);
            expect(assetsState.snapshot.assetsPager.numberOfItems).toBe(199);
            expect(assetsState.snapshot.tagsAvailable).toEqual({ shared: 1, tag2: 1 });
        });
    });
});