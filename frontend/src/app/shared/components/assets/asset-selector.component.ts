/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, OnInit, Output } from '@angular/core';
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

    constructor(localStore: LocalStoreService,
        public readonly assetsState: ComponentAssetsState,
    ) {
        super({
            selectedAssets: {},
            selectionCount: 0,
            isListView: localStore.getBoolean(Settings.Local.ASSETS_MODE),
        });

        this.project(x => x.isListView).subscribe(value => {
            localStore.setBoolean(Settings.Local.ASSETS_MODE, value);
        });
    }

    public ngOnInit() {
        this.assetsState.load();
    }

    public reload() {
        this.assetsState.load(true);
    }

    public reloadTotal() {
        this.assetsState.load(true, false);
    }

    public search(query: Query) {
        this.assetsState.search(query);
    }

    public emitClose() {
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
