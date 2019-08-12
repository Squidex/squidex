/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, OnInit, Output } from '@angular/core';

import {
    AssetDto,
    AssetsState,
    fadeAnimation,
    LocalStoreService,
    Query,
    StatefulComponent
} from '@app/shared/internal';

interface State {
    selectedAssets: { [id: string]: AssetDto };
    selectionCount: number;

    isListView: boolean;
}

@Component({
    selector: 'sqx-assets-selector',
    styleUrls: ['./assets-selector.component.scss'],
    templateUrl: './assets-selector.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetsSelectorComponent extends StatefulComponent<State> implements OnInit {
    @Output()
    public select = new EventEmitter<AssetDto[]>();

    constructor(changeDector: ChangeDetectorRef,
        public readonly assetsState: AssetsState,
        public readonly localStore: LocalStoreService
    ) {
        super(changeDector, {
            selectedAssets: {},
            selectionCount: 0,
            isListView: localStore.getBoolean('squidex.assets.list-view')
        });
    }

    public ngOnInit() {
        this.assetsState.load();
    }

    public reload() {
        this.assetsState.load(true);
    }

    public search(query: Query) {
        this.assetsState.search(query);
    }

    public emitComplete() {
        this.select.emit([]);
    }

    public emitSelect() {
        this.select.emit(Object.values(this.snapshot.selectedAssets));
    }

    public selectTags(tags: string[]) {
        this.assetsState.selectTags(tags);
    }

    public selectAsset(asset: AssetDto) {
        this.next(s => {
            const selectedAssets = { ...s.selectedAssets };

            if (selectedAssets[asset.id]) {
                delete selectedAssets[asset.id];
            } else {
                selectedAssets[asset.id] = asset;
            }

            const selectionCount = Object.keys(selectedAssets).length;

            return { ...s, selectedAssets, selectionCount };
        });
    }

    public changeView(isListView: boolean) {
        this.next(s => ({ ...s, isListView }));

        this.localStore.setBoolean('squidex.assets.list-view', isListView);
    }
}

