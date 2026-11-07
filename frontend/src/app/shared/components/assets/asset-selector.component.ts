/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, effect, inject, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ListViewComponent, ModalDialogComponent, PagerComponent, TagEditorComponent, TooltipDirective, TranslatePipe } from '@app/framework';
import { AssetDto, ComponentAssetsState, LocalStoreService, Query, Settings } from '@app/shared/internal';
import { SearchFormComponent } from '../search/search-form.component';
import { AssetsListComponent } from './assets-list.component';

@Component({
    selector: 'sqx-asset-selector',
    styleUrls: ['./asset-selector.component.scss'],
    templateUrl: './asset-selector.component.html',
    providers: [
        ComponentAssetsState,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AssetsListComponent,
        FormsModule,
        ListViewComponent,
        ModalDialogComponent,
        PagerComponent,
        SearchFormComponent,
        TagEditorComponent,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class AssetSelectorComponent {
    private readonly localStore = inject(LocalStoreService);

    /** Emits the selected assets, or an empty array when the dialog is closed. */
    public readonly assetSelect = output<ReadonlyArray<AssetDto>>();

    protected readonly selectedAssets = signal<Record<string, AssetDto>>({});

    protected readonly selectionCount = computed(() => {
        return Object.keys(this.selectedAssets()).length;
    });

    protected readonly isListView = signal(this.localStore.getBoolean(Settings.Local.ASSETS_MODE));

    constructor(
        protected readonly assetsState: ComponentAssetsState,
    ) {
        effect(() => {
            this.localStore.setBoolean(Settings.Local.ASSETS_MODE, this.isListView());
        });

        this.assetsState.load();
    }

    protected reload() {
        this.assetsState.load(true);
    }

    protected reloadTotal() {
        this.assetsState.load(true, false);
    }

    protected search(query: Query) {
        this.assetsState.search(query);
    }

    protected emitClose() {
        this.assetSelect.emit([]);
    }

    protected emitSelect() {
        this.assetSelect.emit(Object.values(this.selectedAssets()));
    }

    protected selectTags(tags: ReadonlyArray<string>) {
        this.assetsState.selectTags(tags);
    }

    protected selectAsset(asset: AssetDto) {
        this.selectedAssets.update(s => {
            const selectedAssets = { ...s };

            if (selectedAssets[asset.id]) {
                delete selectedAssets[asset.id];
            } else {
                selectedAssets[asset.id] = asset;
            }

            return selectedAssets;
        });
    }

    protected changeView(isListView: boolean) {
        this.isListView.set(isListView);
    }
}
