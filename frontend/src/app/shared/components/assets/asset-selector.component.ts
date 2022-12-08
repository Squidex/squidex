/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, OnInit, Output } from '@angular/core';
import { distinctUntilChanged, map } from 'rxjs/operators';
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
    selector: 'sqx-asset-selector',
    styleUrls: ['./asset-selector.component.scss'],
    templateUrl: './asset-selector.component.html',
    providers: [
        ComponentAssetsState,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AssetSelectorComponent extends StatefulComponent<State> implements OnInit {
    @Output()
    public select = new EventEmitter<ReadonlyArray<AssetDto>>();

    constructor(changeDector: ChangeDetectorRef, localStore: LocalStoreService,
        public readonly assetsState: ComponentAssetsState,
    ) {
        super(changeDector, {
            selectedAssets: {},
            selectionCount: 0,
            isListView: localStore.getBoolean(Settings.Local.ASSETS_MODE),
        });

        this.changes.pipe(map(x => x.isListView), distinctUntilChanged()).subscribe(value => {
            localStore.setBoolean(Settings.Local.ASSETS_MODE, value);
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
    }
}
