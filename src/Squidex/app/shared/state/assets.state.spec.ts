/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Observable } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    AppsState,
    AssetDto,
    AssetsDto,
    AssetsService,
    AssetsState,
    DateTime,
    DialogService,
    Version,
    Versioned
 } from '@app/shared';

describe('AssetsState', () => {
    const app = 'my-app';
    const creation = DateTime.today();
    const creator = 'not-me';
    const modified = DateTime.now();
    const modifier = 'me';
    const version = new Version('1');
    const newVersion = new Version('2');

    const oldAssets = [
        new AssetDto('id1', creator, creator, creation, creation, 'name1', 'type1', 1, 1, 'mime1', false, null, null, 'url1', version),
        new AssetDto('id2', creator, creator, creation, creation, 'name2', 'type2', 2, 2, 'mime2', false, null, null, 'url2', version)
    ];

    let dialogs: IMock<DialogService>;
    let appsState: IMock<AppsState>;
    let assetsService: IMock<AssetsService>;
    let assetsState: AssetsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        appsState = Mock.ofType<AppsState>();

        appsState.setup(x => x.appName)
            .returns(() => app);

        assetsService = Mock.ofType<AssetsService>();

        assetsService.setup(x => x.getAssets(app, 30, 0, undefined))
            .returns(() => Observable.of(new AssetsDto(200, oldAssets)));

        assetsState = new AssetsState(appsState.object, assetsService.object, dialogs.object);
        assetsState.load().subscribe();
    });

    it('should load assets', () => {
        assetsState.load().subscribe();

        expect(assetsState.snapshot.assets.values).toEqual(oldAssets);
        expect(assetsState.snapshot.assetsPager.numberOfItems).toEqual(200);
        expect(assetsState.snapshot.isLoaded).toBeTruthy();

        assetsService.verify(x => x.getAssets(app, 30, 0, undefined), Times.exactly(2));

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
    });

    it('should show notification on load when reload is true', () => {
        assetsState.load(true).subscribe();

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
    });

    it('should add asset to snapshot when created', () => {
        const newAsset = new AssetDto('id3', creator, creator, creation, creation, 'name3', 'type3', 3, 3, 'mime3', true, 0, 0, 'url3', version);

        assetsState.add(newAsset);

        expect(assetsState.snapshot.assets.values).toEqual([newAsset, ...oldAssets]);
        expect(assetsState.snapshot.assetsPager.numberOfItems).toBe(201);
    });

    it('should update properties when updated', () => {
        const newAsset = new AssetDto('id1', modifier, modifier, modified, modified, 'name3', 'type3', 3, 3, 'mime3', true, 0, 0, 'url3', version);

        assetsState.update(newAsset);

        const asset_1 = assetsState.snapshot.assets.at(0);

        expect(asset_1).toBe(newAsset);
    });

    it('should remove asset from snapshot when deleted', () => {
        assetsService.setup(x => x.deleteAsset(app, oldAssets[0].id, version))
            .returns(() => Observable.of(new Versioned<any>(newVersion, {})));

        assetsState.delete(oldAssets[0]).subscribe();

        expect(assetsState.snapshot.assets.values.length).toBe(1);
        expect(assetsState.snapshot.assetsPager.numberOfItems).toBe(199);
    });

    it('should load next page and prev page when paging', () => {
        assetsService.setup(x => x.getAssets(app, 30, 30, undefined))
            .returns(() => Observable.of(new AssetsDto(200, [])));

        assetsState.goNext().subscribe();
        assetsState.goPrev().subscribe();

        assetsService.verify(x => x.getAssets(app, 30, 30, undefined), Times.once());
        assetsService.verify(x => x.getAssets(app, 30,  0, undefined), Times.exactly(2));
    });

    it('should load with query when searching', () => {
        assetsService.setup(x => x.getAssets(app, 30, 0, 'my-query'))
            .returns(() => Observable.of(new AssetsDto(0, [])));

        assetsState.search('my-query').subscribe();

        expect(assetsState.snapshot.assetsQuery).toEqual('my-query');

        assetsService.verify(x => x.getAssets(app, 30, 0, 'my-query'), Times.once());
    });
});