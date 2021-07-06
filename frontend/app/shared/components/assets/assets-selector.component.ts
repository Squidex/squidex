/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AssetDto, ComponentAssetsState, LocalStoreService, Query, Settings, StatefulComponent } from '@app/shared/internal';

interface State {
    // The selected assets.
    selectedAssets: { [id: string]: AssetDto };

    // The number of selected items.
    selectionCount: number;

    // True, when rendering the assets as list.
    isListView: boolean;
}

@Component({
    selector: 'sqx-assets-selector',
    styleUrls: ['./assets-selector.component.scss'],
    templateUrl: './assets-selector.component.html',
    providers: [
        ComponentAssetsState,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AssetsSelectorComponent extends StatefulComponent<State> implements OnInit {
    @Output()
    public select = new EventEmitter<ReadonlyArray<AssetDto>>();

    constructor(changeDector: ChangeDetectorRef,
        public readonly assetsState: ComponentAssetsState,
        public readonly localStore: LocalStoreService,
    ) {
        super(changeDector, {
            selectedAssets: {},
            selectionCount: 0,
            isListView: localStore.getBoolean(Settings.Local.ASSETS_MODE),
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

    public selectTags(tags: ReadonlyArray<string>) {
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
        this.next({ isListView });

        this.localStore.setBoolean(Settings.Local.ASSETS_MODE, isListView);
    }
}
