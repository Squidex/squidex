/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, throwError } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    AssetFoldersDto,
    AssetPathItem,
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

describe('AssetsState', () => {
    const {
        app,
        appsState,
        newVersion
    } = TestValues;

    const asset1 = createAsset(1, ['tag1', 'shared']);
    const asset2 = createAsset(2, ['tag2', 'shared']);

    const assetFolder1 = createAssetFolder(1);
    const assetFolder2 = createAssetFolder(2);

    const newAssetFolder = createAssetFolder(0, '_new');

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
                .returns(() => of(new AssetFoldersDto(2, [assetFolder1, assetFolder2]))).verifiable(Times.atLeastOnce());
        });

        it('should load assets', () => {
            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 0, parentId: MathHelper.EMPTY_GUID }))
                .returns(() => of(new AssetsDto(200, [asset1, asset2]))).verifiable();

            assetsState.load().subscribe();

            expect(assetsState.snapshot.assets).toEqual([asset1, asset2]);
            expect(assetsState.snapshot.assetsPager.numberOfItems).toEqual(200);
            expect(assetsState.snapshot.isLoaded).toBeTruthy();
            expect(assetsState.snapshot.isLoading).toBeFalsy();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should show notification on load when reload is true', () => {
            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 0, parentId: MathHelper.EMPTY_GUID }))
                .returns(() => of(new AssetsDto(200, [asset1, asset2]))).verifiable();

            assetsState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

        it('should load without tags when tag untoggled', () => {
            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 0, tags: ['tag1'] }))
                .returns(() => of(new AssetsDto(0, []))).verifiable();

            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 0, parentId: MathHelper.EMPTY_GUID }))
                .returns(() => of(new AssetsDto(0, []))).verifiable();

            assetsState.toggleTag('tag1').subscribe();
            assetsState.toggleTag('tag1').subscribe();

            expect(assetsState.snapshot.tagsSelected).toEqual({});
        });

        it('should load without tags when tags reset', () => {
            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 0, parentId: MathHelper.EMPTY_GUID }))
                .returns(() => of(new AssetsDto(0, []))).verifiable();

            assetsState.resetTags().subscribe();

            expect(assetsState.snapshot.tagsSelected).toEqual({});
        });

        it('should load with new pagination when paging', () => {
            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 30, parentId: MathHelper.EMPTY_GUID }))
                .returns(() => of(new AssetsDto(200, []))).verifiable();

            assetsState.setPager(new Pager(200, 1, 30)).subscribe();

            expect().nothing();
        });

        it('should update page size in local store', () => {
            assetsService.setup(x => x.getAssets(app, { take: 50, skip: 0, parentId: MathHelper.EMPTY_GUID }))
                .returns(() => of(new AssetsDto(200, []))).verifiable();

            assetsState.setPager(new Pager(0, 0, 50));

            localStore.verify(x => x.setInt('assets.pageSize', 50), Times.atLeastOnce());

            expect().nothing();
        });
    });

    describe('Navigating', () => {
        beforeEach(() => {
            assetsService.setup(x => x.getAssets(app, It.isAny()))
                .returns(() => of(new AssetsDto(0, [])));

            assetsService.setup(x => x.getAssetFolders(app, It.isAny()))
                .returns(() => of(new AssetFoldersDto(0, [])));
        });

        it('should move to child', () => {
            assetsState.navigate({ id: '1', folderName: 'Folder1' }).subscribe();
            assetsState.navigate({ id: '2', folderName: 'Folder2' }).subscribe();

            let path: ReadonlyArray<AssetPathItem>;

            assetsState.path.subscribe(result => {
                path = result;
            });

            expect(path!).toEqual([
                { id: MathHelper.EMPTY_GUID, folderName: 'Assets' },
                { id: '1', folderName: 'Folder1' },
                { id: '2', folderName: 'Folder2' }
            ]);
        });

        it('should navigate back to parent', () => {
            assetsState.navigate({ id: '1', folderName: 'Folder1' }).subscribe();
            assetsState.navigate({ id: '2', folderName: 'Folder2' }).subscribe();
            assetsState.navigate({ id: MathHelper.EMPTY_GUID, folderName: 'Assets' }).subscribe();

            let path: ReadonlyArray<AssetPathItem>;

            assetsState.path.subscribe(result => {
                path = result;
            });

            expect(path!).toEqual([
                { id: MathHelper.EMPTY_GUID, folderName: 'Assets' }
            ]);
        });
    });

    describe('Searching', () => {
        it('should load with tags when tag toggled', () => {
            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 0, tags: ['tag1'] }))
                .returns(() => of(new AssetsDto(0, []))).verifiable();

            assetsState.toggleTag('tag1').subscribe();

            expect(assetsState.snapshot.tagsSelected).toEqual({ tag1: true });
        });

        it('should load with tags when tags selected', () => {
            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 0, tags: ['tag1', 'tag2'] }))
                .returns(() => of(new AssetsDto(0, []))).verifiable();

            assetsState.selectTags(['tag1', 'tag2']).subscribe();

            expect(assetsState.snapshot.tagsSelected).toEqual({ tag1: true, tag2: true });
        });

        it('should load with query when searching', () => {
            const query = { fullText: 'my-query' };

            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 0, query }))
                .returns(() => of(new AssetsDto(0, []))).verifiable();

            assetsState.search(query).subscribe();

            expect(assetsState.snapshot.assetsQuery).toEqual(query);
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            assetsService.setup(x => x.getAssetFolders(app, MathHelper.EMPTY_GUID))
                .returns(() => of(new AssetFoldersDto(2, [assetFolder1, assetFolder2])));

            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 0, parentId: MathHelper.EMPTY_GUID }))
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

        it('should add asset folder when created', () => {
            const request = { folderName: 'New Folder', parentId: MathHelper.EMPTY_GUID };

            assetsService.setup(x => x.postAssetFolder(app, It.isValue(request)))
                .returns(() => of(newAssetFolder));

            assetsState.createAssetFolder(request.folderName);

            expect(assetsState.snapshot.assetFolders).toEqual([newAssetFolder, assetFolder1, assetFolder2]);
        });

        it('should add asset folder when path has changed', () => {
            const otherPath = createAssetFolder(3, '_new', 'otherParent');

            const request = { folderName: 'New Folder', parentId: MathHelper.EMPTY_GUID };

            assetsService.setup(x => x.postAssetFolder(app, It.isValue(request)))
                .returns(() => of(otherPath));

            assetsState.createAssetFolder(request.folderName);

            expect(assetsState.snapshot.assetFolders).toEqual([assetFolder1, assetFolder2]);
        });

        it('should update asset when updated', () => {
            const updated = createAsset(1, ['new'], '_new');

            const request = { fileName: 'New Name' };

            assetsService.setup(x => x.putAsset(app, asset1, request, asset1.version))
                .returns(() => of(updated));

            assetsState.updateAsset(asset1, request);

            const asset1New = assetsState.snapshot.assets[0];

            expect(asset1New).toEqual(updated);
            expect(assetsState.snapshot.tagsAvailable).toEqual({ tag2: 1, shared: 1, new: 1 });
        });

        it('should update asset folder when updated', () => {
            const updated = createAssetFolder(1, '_new');

            const request = { folderName: 'New Name' };

            assetsService.setup(x => x.putAssetFolder(app, assetFolder1, request, assetFolder1.version))
                .returns(() => of(updated));

            assetsState.updateAssetFolder(assetFolder1, request);

            const assetFolder1New = assetsState.snapshot.assetFolders[0];

            expect(assetFolder1New).toEqual(updated);
        });

        it('should remove asset from snapshot when moved to other folder', () => {
            const request = { parentId: 'newParent' };

            assetsService.setup(x => x.putAssetItemParent(app, asset1, It.isValue(request), asset1.version))
                .returns(() => of(versioned(newVersion)));

            assetsState.moveAsset(asset1, request.parentId).subscribe();

            expect(assetsState.snapshot.assets.length).toBe(1);
            expect(assetsState.snapshot.assetsPager.numberOfItems).toBe(200);
        });

        it('should not do anything when moving asset to current parent', () => {
            const request = { parentId: MathHelper.EMPTY_GUID };

            assetsState.moveAsset(asset1, request.parentId).pipe(onErrorResumeNext()).subscribe();

            expect(assetsState.snapshot.assets.length).toBe(2);
            expect(assetsState.snapshot.assetsPager.numberOfItems).toBe(200);
        });

        it('should move asset back to snapshot when moving via api failed', () => {
            const request = { parentId: 'newParent' };

            assetsService.setup(x => x.putAssetItemParent(app, asset1, It.isValue(request), asset1.version))
                .returns(() => throwError('error'));

            assetsState.moveAsset(asset1, request.parentId).pipe(onErrorResumeNext()).subscribe();

            expect(assetsState.snapshot.assets.length).toBe(2);
            expect(assetsState.snapshot.assetsPager.numberOfItems).toBe(200);
        });

        it('should remove asset folder from snapshot when moved to other folder', () => {
            const request = { parentId: 'newParent' };

            assetsService.setup(x => x.putAssetItemParent(app, assetFolder1, It.isValue(request), assetFolder1.version))
                .returns(() => of(versioned(newVersion)));

            assetsState.moveAssetFolder(assetFolder1, request.parentId).subscribe();

            expect(assetsState.snapshot.assetFolders.length).toBe(1);
        });

        it('should not do anything when moving asset folder to itself', () => {
            const request = { parentId: assetFolder1.id };

            assetsState.moveAssetFolder(assetFolder1, request.parentId).pipe(onErrorResumeNext()).subscribe();

            expect(assetsState.snapshot.assets.length).toBe(2);
        });

        it('should not do anything when moving asset folder to current parent', () => {
            const request = { parentId: MathHelper.EMPTY_GUID };

            assetsState.moveAssetFolder(assetFolder1, request.parentId).pipe(onErrorResumeNext()).subscribe();

            expect(assetsState.snapshot.assets.length).toBe(2);
        });

        it('should move asset folder back to snapshot when moving via api failed', () => {
            const request = { parentId: 'newParent' };

            assetsService.setup(x => x.putAssetItemParent(app, assetFolder1, It.isValue(request), assetFolder1.version))
                .returns(() => throwError('error'));

            assetsState.moveAssetFolder(assetFolder1, request.parentId).pipe(onErrorResumeNext()).subscribe();

            expect(assetsState.snapshot.assets.length).toBe(2);
        });

        it('should remove asset from snapshot when deleted', () => {
            assetsService.setup(x => x.deleteAssetItem(app, asset1, asset1.version))
                .returns(() => of(versioned(newVersion)));

            assetsState.deleteAsset(asset1).subscribe();

            expect(assetsState.snapshot.assets.length).toBe(1);
            expect(assetsState.snapshot.assetsPager.numberOfItems).toBe(199);
            expect(assetsState.snapshot.tagsAvailable).toEqual({ shared: 1, tag2: 1 });
        });

        it('should remove asset folder from snapshot when deleted', () => {
            assetsService.setup(x => x.deleteAssetItem(app, assetFolder1, assetFolder1.version))
                .returns(() => of(versioned(newVersion)));

            assetsState.deleteAssetFolder(assetFolder1).subscribe();

            expect(assetsState.snapshot.assetFolders.length).toBe(1);
        });
    });
});