/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, onErrorResumeNextWith, throwError } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { ErrorDto } from '@app/framework';
import { AnnotateAssetDto, AssetFoldersDto, AssetsDto, AssetsService, AssetsState, CreateAssetFolderDto, DialogService, MathHelper, MoveAssetDto, MoveAssetFolderDto, RenameAssetFolderDto, RenameTagDto, versioned } from '@app/shared/internal';
import { createAsset, createAssetFolder } from '../services/assets.service.spec';
import { TestValues } from './_test-helpers';

describe('AssetsState', () => {
    const {
        app,
        appsState,
        newVersion,
    } = TestValues;

    const asset1 = createAsset(1, ['tag1', 'shared']);
    const asset2 = createAsset(2, ['tag2', 'shared']);

    const assetFolder1 = createAssetFolder(1);
    const assetFolder2 = createAssetFolder(2);

    const newAssetFolder = createAssetFolder(0, '_new');

    let dialogs: IMock<DialogService>;
    let assetsService: IMock<AssetsService>;
    let assetsState: AssetsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        assetsService = Mock.ofType<AssetsService>();
        assetsService.setup(x => x.getTags(app))
            .returns(() => of({ tag1: 1, shared: 2, tag2: 1 }));

        assetsState = new AssetsState(appsState.object, assetsService.object, dialogs.object);
    });

    afterEach(() => {
        assetsService.verifyAll();
    });

    describe('Loading', () => {
        beforeEach(() => {
            assetsService.setup(x => x.getAssetFolders(app, MathHelper.EMPTY_GUID, 'PathAndItems'))
                .returns(() => of(new AssetFoldersDto({ items: [assetFolder1, assetFolder2], total: 2, path: [], _links: {} }))).verifiable(Times.atLeastOnce());
        });

        it('should load assets', () => {
            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 0, parentId: MathHelper.EMPTY_GUID, noSlowTotal: true }))
                .returns(() => of(new AssetsDto({ items: [asset1, asset2], total: 200, _links: {} }))).verifiable();

            assetsState.load().subscribe();

            expect(assetsState.snapshot.assets).toEqual([asset1, asset2]);
            expect(assetsState.snapshot.isLoaded).toBeTruthy();
            expect(assetsState.snapshot.isLoading).toBeFalsy();
            expect(assetsState.snapshot.total).toEqual(200);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should show notification on load if reload is true', () => {
            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 0, parentId: MathHelper.EMPTY_GUID, noSlowTotal: true }))
                .returns(() => of(new AssetsDto({ items: [asset1, asset2], total: 200, _links: {} }))).verifiable();

            assetsState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

        it('should load with total', () => {
            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 0, parentId: MathHelper.EMPTY_GUID, noSlowTotal: false }))
                .returns(() => of(new AssetsDto({ items: [asset1, asset2], total: 200, _links: {} }))).verifiable();

            assetsState.load(true, false).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

        it('should load without tags if tag untoggled', () => {
            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 0, tags: ['tag1'], noSlowTotal: true }))
            .returns(() => of(new AssetsDto({ items: [], total: 0, _links: {} }))).verifiable();

            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 0, parentId: MathHelper.EMPTY_GUID, noSlowTotal: true }))
                .returns(() => of(new AssetsDto({ items: [], total: 0, _links: {} }))).verifiable();

            assetsState.toggleTag('tag1').subscribe();
            assetsState.toggleTag('tag1').subscribe();

            expect(assetsState.snapshot.tagsSelected).toEqual({});
        });

        it('should load without tags if tags reset', () => {
            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 0, parentId: MathHelper.EMPTY_GUID, noSlowTotal: true }))
                .returns(() => of(new AssetsDto({ items: [], total: 0, _links: {} }))).verifiable();

            assetsState.resetTags().subscribe();

            expect(assetsState.snapshot.tagsSelected).toEqual({});
        });

        it('should load with new pagination if paging', () => {
            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 30, parentId: MathHelper.EMPTY_GUID, noSlowTotal: true }))
            .returns(() => of(new AssetsDto({ items: [], total: 200, _links: {} }))).verifiable();

            assetsState.page({ page: 1, pageSize: 30 }).subscribe();

            expect().nothing();
        });

        it('should skip page size if loaded before', () => {
            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 0, parentId: MathHelper.EMPTY_GUID, noSlowTotal: true }))
            .returns(() => of(new AssetsDto({ items: [asset1, asset2], total: 200, _links: {} }))).verifiable();

            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 30, parentId: MathHelper.EMPTY_GUID, noSlowTotal: true, noTotal: true }))
                .returns(() => of(new AssetsDto({ items: [], total: 200, _links: {} }))).verifiable();

            assetsState.load().subscribe();
            assetsState.page({ page: 1, pageSize: 30 }).subscribe();

            expect().nothing();
        });
    });

    describe('Navigating', () => {
        it('should load with parent id', () => {
            assetsService.setup(x => x.getAssetFolders(app, '123', 'PathAndItems'))
                .returns(() => of(new AssetFoldersDto({ items: [assetFolder1, assetFolder2], total: 2, path: [], _links: {} }))).verifiable();

            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 0, parentId: '123', noSlowTotal: true }))
                .returns(() => of(new AssetsDto({ items: [], total: 200, _links: {} }))).verifiable();

            assetsState.navigate('123').subscribe();

            expect().nothing();
        });
    });

    describe('Searching', () => {
        it('should load with tags if tag toggled', () => {
            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 0, tags: ['tag1'], noSlowTotal: true }))
                .returns(() => of(new AssetsDto({ items: [], total: 0, _links: {} }))).verifiable();

            assetsState.toggleTag('tag1').subscribe();

            expect(assetsState.snapshot.tagsSelected).toEqual({ tag1: true });
        });

        it('should load with tags if tags selected', () => {
            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 0, tags: ['tag1', 'tag2'], noSlowTotal: true }))
                .returns(() => of(new AssetsDto({ items: [], total: 0, _links: {} }))).verifiable();

            assetsState.selectTags(['tag1', 'tag2']).subscribe();

            expect(assetsState.snapshot.tagsSelected).toEqual({ tag1: true, tag2: true });
        });

        it('should load with query if searching', () => {
            const query = { fullText: 'my-query' };

            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 0, query, noSlowTotal: true }))
                .returns(() => of(new AssetsDto({ items: [], total: 0, _links: {} }))).verifiable();

            assetsState.search(query).subscribe();

            expect(assetsState.snapshot.query).toEqual(query);
        });

        it('should unset ref when searching', () => {
            const query = { fullText: 'my-query' };

            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 0, query, noSlowTotal: true }))
                .returns(() => of(new AssetsDto({ items: [], total: 0, _links: {} }))).verifiable();

            assetsState.next({ ref: '1' });
            assetsState.search(query).subscribe();

            expect(assetsState.snapshot.query).toEqual(query);
            expect(assetsState.snapshot.ref).toBeNull();
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            assetsService.setup(x => x.getAssetFolders(app, MathHelper.EMPTY_GUID, 'PathAndItems'))
                .returns(() => of(new AssetFoldersDto({ items: [assetFolder1, assetFolder2], total: 2, path: [], _links: {} })));

            assetsService.setup(x => x.getAssets(app, { take: 30, skip: 0, parentId: MathHelper.EMPTY_GUID, noSlowTotal: true }))
                .returns(() => of(new AssetsDto({ items: [asset1, asset2], total: 200, _links: {} }))).verifiable();

            assetsService.setup(x => x.getAssets(app, { take: 2, skip: 0, parentId: MathHelper.EMPTY_GUID, noSlowTotal: true }))
                .returns(() => of(new AssetsDto({ items: [asset1, asset2], total: 200, _links: {} })));

            assetsState.load(true).subscribe();
        });

        it('should add asset to snapshot', () => {
            const newAsset = createAsset(3, ['new']);

            assetsState.addAsset(newAsset);

            expect(assetsState.snapshot.assets).toEqual([newAsset, asset1, asset2]);
            expect(assetsState.snapshot.total).toBe(201);
        });

        it('should not add asset to snapshot if it already exist', () => {
            const newAsset = createAsset(1, ['new']);

            assetsState.addAsset(newAsset);

            expect(assetsState.snapshot.assets).toEqual([asset1, asset2]);
            expect(assetsState.snapshot.total).toBe(200);
        });

        it('should truncate assets if page size reached', () => {
            const newAsset = createAsset(3, ['new']);

            assetsState.page({ page: 0, pageSize: 2 }).subscribe();
            assetsState.addAsset(newAsset);

            expect(assetsState.snapshot.assets).toEqual([newAsset, asset1]);
            expect(assetsState.snapshot.total).toBe(201);
        });

        it('should not add asset to snapshot if parent id is not the same', () => {
            const newAsset = createAsset(3, ['new'], '_new', 'other-parent');

            assetsState.addAsset(newAsset);

            expect(assetsState.snapshot.assets).toEqual([asset1, asset2]);
            expect(assetsState.snapshot.total).toBe(200);
        });

        it('should increment tags if asset added', () => {
            assetsState.addAsset(createAsset(5, ['new']));
            assetsState.addAsset(createAsset(6, ['new']));

            expect(assetsState.snapshot.tagsAvailable).toEqual({ tag1: 1, tag2: 1, shared: 2, new: 2 });
        });

        it('should replace asset in snapshot', () => {
            const newAsset = createAsset(2, ['new']);

            assetsState.replaceAsset(newAsset);

            expect(assetsState.snapshot.assets).toEqual([asset1, newAsset]);
            expect(assetsState.snapshot.total).toBe(200);
        });

        it('should not replace asset in snapshot if it does not exist', () => {
            const newAsset = createAsset(3, ['new']);

            assetsState.replaceAsset(newAsset);

            expect(assetsState.snapshot.assets).toEqual([asset1, asset2]);
            expect(assetsState.snapshot.total).toBe(200);
        });

        it('should add asset folder if created', () => {
            const request = new CreateAssetFolderDto({ folderName: 'New Folder', parentId: MathHelper.EMPTY_GUID });

            assetsService.setup(x => x.postAssetFolder(app, request))
                .returns(() => of(newAssetFolder));

            assetsState.createAssetFolder(request.folderName);

            expect(assetsState.snapshot.folders).toEqual([newAssetFolder, assetFolder1, assetFolder2]);
        });

        it('should add asset folder if path has changed', () => {
            const otherPath = createAssetFolder(3, '_new', 'otherParent');

            const request = new CreateAssetFolderDto({ folderName: 'New Folder', parentId: MathHelper.EMPTY_GUID });

            assetsService.setup(x => x.postAssetFolder(app, request))
                .returns(() => of(otherPath));

            assetsState.createAssetFolder(request.folderName);

            expect(assetsState.snapshot.folders).toEqual([assetFolder1, assetFolder2]);
        });

        it('should update asset if updated', () => {
            const updated = createAsset(1, ['new'], '_new');

            const request = new AnnotateAssetDto({ fileName: 'New Name' });

            assetsService.setup(x => x.putAsset(app, asset1, request, asset1.version))
                .returns(() => of(updated));

            assetsState.updateAsset(asset1, request);

            expect(assetsState.snapshot.assets).toEqual([updated, asset2]);
            expect(assetsState.snapshot.tagsAvailable).toEqual({ tag2: 1, shared: 1, new: 1 });
        });

        it('should update asset folder if updated', () => {
            const updated = createAssetFolder(1, '_new');

            const request = new RenameAssetFolderDto({ folderName: 'New Name' });

            assetsService.setup(x => x.putAssetFolder(app, assetFolder1, request, assetFolder1.version))
                .returns(() => of(updated));

            assetsState.updateAssetFolder(assetFolder1, request);

            expect(assetsState.snapshot.folders).toEqual([updated, assetFolder2]);
        });

        it('should remove asset from snapshot if moved to other folder', () => {
            const updated = createAsset(1, ['new'], '_new');

            const request = new MoveAssetDto({ parentId: 'newParent' });

            assetsService.setup(x => x.putAssetParent(app, asset1, It.isValue(request), asset1.version))
                .returns(() => of(updated));

            assetsState.moveAsset(asset1, request.parentId).subscribe();

            expect(assetsState.snapshot.assets.length).toBe(1);
            expect(assetsState.snapshot.total).toBe(199);
        });

        it('should add asset to snapshot if moved to current folder', () => {
            const asset3 = createAsset(3, undefined, undefined, 'oldParent');

            const request = new MoveAssetDto({ parentId: assetsState.snapshot.parentId });

            assetsService.setup(x => x.putAssetParent(app, asset3, It.isValue(request), asset3.version))
                .returns(() => of(asset3));

            assetsState.moveAsset(asset3, request.parentId).subscribe();

            expect(assetsState.snapshot.assets).toEqual([asset3, asset1, asset2]);
            expect(assetsState.snapshot.total).toBe(201);
        });

        it('should not do anything if moving asset to same parent', () => {
            const request = { parentId: MathHelper.EMPTY_GUID };

            assetsState.moveAsset(asset1, request.parentId).pipe(onErrorResumeNextWith()).subscribe();

            expect(assetsState.snapshot.assets.length).toBe(2);
            expect(assetsState.snapshot.total).toBe(200);
        });

        it('should move asset back to snapshot if moving via api failed', () => {
            const request = new MoveAssetDto({ parentId: 'newParent' });

            assetsService.setup(x => x.putAssetParent(app, asset1, It.isValue(request), asset1.version))
                .returns(() => throwError(() => 'Service Error'));

            assetsState.moveAsset(asset1, request.parentId).pipe(onErrorResumeNextWith()).subscribe();

            expect(assetsState.snapshot.assets.length).toBe(2);
            expect(assetsState.snapshot.total).toBe(200);
        });

        it('should remove asset folder from snapshot if moved to other folder', () => {
            const updated = createAssetFolder(1, '_new');

            const request = new MoveAssetFolderDto({ parentId: 'newParent' });

            assetsService.setup(x => x.putAssetFolderParent(app, assetFolder1, It.isValue(request), assetFolder1.version))
                .returns(() => of(updated));

            assetsState.moveAssetFolder(assetFolder1, request.parentId).subscribe();

            expect(assetsState.snapshot.folders).toEqual([assetFolder2]);
        });

        it('should add asset folder to snapshot if moved to current folder', () => {
            const assetFolder3 = createAssetFolder(3, undefined, 'oldParent');

            const request = new MoveAssetFolderDto({ parentId: assetsState.snapshot.parentId });

            assetsService.setup(x => x.putAssetFolderParent(app, assetFolder3, It.isValue(request), assetFolder3.version))
                .returns(() => of(assetFolder3));

            assetsState.moveAssetFolder(assetFolder3, request.parentId).subscribe();

            expect(assetsState.snapshot.folders).toEqual([assetFolder1, assetFolder2, assetFolder3]);
        });

        it('should not do anything if moving asset folder to itself', () => {
            const request = new MoveAssetFolderDto({ parentId: assetFolder1.id });

            assetsState.moveAssetFolder(assetFolder1, request.parentId).pipe(onErrorResumeNextWith()).subscribe();

            expect(assetsState.snapshot.assets.length).toBe(2);
        });

        it('should not do anything if moving asset folder to current parent', () => {
            const request = new MoveAssetFolderDto({ parentId: MathHelper.EMPTY_GUID });

            assetsState.moveAssetFolder(assetFolder1, request.parentId).pipe(onErrorResumeNextWith()).subscribe();

            expect(assetsState.snapshot.assets.length).toBe(2);
        });

        it('should move asset folder back to snapshot if moving via api failed', () => {
            const request = new MoveAssetFolderDto({ parentId: 'newParent' });

            assetsService.setup(x => x.putAssetFolderParent(app, assetFolder1, It.isValue(request), assetFolder1.version))
                .returns(() => throwError(() => 'Service Error'));

            assetsState.moveAssetFolder(assetFolder1, request.parentId).pipe(onErrorResumeNextWith()).subscribe();

            expect(assetsState.snapshot.assets.length).toBe(2);
        });

        it('should remove asset from snapshot if deleted', () => {
            assetsService.setup(x => x.deleteAssetItem(app, asset1, true, asset1.version))
                .returns(() => of(versioned(newVersion)));

            assetsState.deleteAsset(asset1).subscribe();

            expect(assetsState.snapshot.assets).toEqual([asset2]);
            expect(assetsState.snapshot.total).toBe(199);
            expect(assetsState.snapshot.tagsAvailable).toEqual({ shared: 1, tag2: 1 });
        });

        it('should remove asset from snapshot if when referenced and not confirmed', () => {
            assetsService.setup(x => x.deleteAssetItem(app, asset1, false, asset1.version))
                .returns(() => throwError(() => new ErrorDto(404, 'Referenced', 'OBJECT_REFERENCED')));

            assetsService.setup(x => x.deleteAssetItem(app, asset1, true, asset1.version))
                .returns(() => of(versioned(newVersion)));

            dialogs.setup(x => x.confirm(It.isAnyString(), It.isAnyString(), It.isAnyString()))
                .returns(() => of(true));

            assetsState.deleteAsset(asset1).subscribe();

            expect(assetsState.snapshot.assets).toEqual([asset2]);
            expect(assetsState.snapshot.total).toBe(199);
            expect(assetsState.snapshot.tagsAvailable).toEqual({ shared: 1, tag2: 1 });
        });

        it('should not remove asset if referenced and not confirmed', () => {
            assetsService.setup(x => x.deleteAssetItem(app, asset1, true, asset1.version))
                .returns(() => throwError(() => new ErrorDto(404, 'Referenced', 'OBJECT_REFERENCED')));

            dialogs.setup(x => x.confirm(It.isAnyString(), It.isAnyString(), It.isAnyString()))
                .returns(() => of(false));

            assetsState.deleteAsset(asset1).pipe(onErrorResumeNextWith()).subscribe();

            expect(assetsState.snapshot.assets.length).toBe(2);
        });

        it('should remove asset folder from snapshot if deleted', () => {
            assetsService.setup(x => x.deleteAssetItem(app, assetFolder1, false, assetFolder1.version))
                .returns(() => of(versioned(newVersion)));

            assetsState.deleteAssetFolder(assetFolder1).subscribe();

            expect(assetsState.snapshot.folders.length).toBe(1);
        });

        it('should replace tags if renamed', () => {
            const request = new RenameTagDto({ tagName: 'new-name' });

            assetsService.setup(x => x.putTag(app, 'old-name', It.isValue(request)))
                .returns(() => of({ 'new-name': 1 }));

            assetsState.renameTag('old-name', 'new-name').subscribe();

            expect(assetsState.snapshot.tagsAvailable).toEqual({ 'new-name': 1 });
        });
    });
});
