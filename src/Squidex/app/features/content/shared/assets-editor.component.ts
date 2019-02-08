/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, Input, OnInit } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';

import {
    AppsState,
    AssetDto,
    AssetsService,
    DialogModel,
    ImmutableArray,
    LocalStoreService,
    MessageBus,
    StatefulControlComponent,
    Types
} from '@app/shared';

export const SQX_ASSETS_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => AssetsEditorComponent), multi: true
};

class AssetUpdated {
    constructor(
        public readonly asset: AssetDto,
        public readonly source: any
    ) {
    }
}

interface State {
    assetFiles: ImmutableArray<File>;

    assets: ImmutableArray<AssetDto>;

    isListView: boolean;
}

@Component({
    selector: 'sqx-assets-editor',
    styleUrls: ['./assets-editor.component.scss'],
    templateUrl: './assets-editor.component.html',
    providers: [SQX_ASSETS_EDITOR_CONTROL_VALUE_ACCESSOR],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetsEditorComponent extends StatefulControlComponent<State, string[]> implements OnInit {
    public assetsDialog = new DialogModel();

    @Input()
    public isCompact = false;

    constructor(changeDetector: ChangeDetectorRef,
        private readonly appsState: AppsState,
        private readonly assetsService: AssetsService,
        private readonly localStore: LocalStoreService,
        private readonly messageBus: MessageBus
    ) {
        super(changeDetector, {
            assets: ImmutableArray.empty(),
            assetFiles: ImmutableArray.empty(),
            isListView: localStore.getBoolean('squidex.assets.list-view')
        });
    }

    public writeValue(obj: any) {
        if (Types.isArrayOfString(obj)) {
            if (!Types.isEquals(obj, this.snapshot.assets.map(x => x.id).values)) {
                const assetIds: string[] = obj;

                this.assetsService.getAssets(this.appsState.appName, 0, 0, undefined, undefined, obj)
                    .subscribe(dtos => {
                        this.setAssets(ImmutableArray.of(assetIds.map(id => dtos.items.find(x => x.id === id)!).filter(a => !!a)));

                        if (this.snapshot.assets.length !== assetIds.length) {
                            this.updateValue();
                        }
                    }, () => {
                        this.setAssets(ImmutableArray.empty());
                    });
            }
        } else {
            this.setAssets(ImmutableArray.empty());
        }
    }

    public notifyOthers(asset: AssetDto) {
        this.messageBus.emit(new AssetUpdated(asset, this));
    }

    public ngOnInit() {
        this.own(
            this.messageBus.of(AssetUpdated)
                .subscribe(event => {
                    if (event.source !== this) {
                        this.setAssets(this.snapshot.assets.replaceBy('id', event.asset));
                    }
                }));
    }

    public setAssets(assets: ImmutableArray<AssetDto>) {
        this.next(s => ({ ...s, assets }));
    }

    public addFiles(files: File[]) {
        for (let file of files) {
            this.next(s => ({ ...s, assetFiles: s.assetFiles.pushFront(file) }));
        }
    }

    public selectAssets(assets: AssetDto[]) {
        this.setAssets(this.snapshot.assets.push(...assets));

        if (assets.length > 0) {
            this.updateValue();
        }

        this.assetsDialog.hide();
    }

    public addAsset(file: File, asset: AssetDto) {
        if (asset && file) {
            this.next(s => ({
                ...s,
                assetFiles: s.assetFiles.remove(file),
                assets: s.assets.pushFront(asset)
            }));

            this.updateValue();
        }
    }

    public sortAssets(assets: AssetDto[]) {
        if (assets) {
            this.setAssets(ImmutableArray.of(assets));

            this.updateValue();
        }
    }

    public removeLoadedAsset(asset: AssetDto) {
        if (asset) {
            this.setAssets(this.snapshot.assets.remove(asset));

            this.updateValue();
        }
    }

    public removeLoadingAsset(file: File) {
        this.next(s => ({ ...s, assetFiles: s.assetFiles.remove(file) }));
    }

    public changeView(isListView: boolean) {
        this.next(s => ({ ...s, isListView }));

        this.localStore.setBoolean('squidex.assets.list-view', isListView);
    }

    private updateValue() {
        let ids: string[] | null = this.snapshot.assets.values.map(x => x.id);

        if (ids.length === 0) {
            ids = null;
        }

        this.callTouched();
        this.callChange(ids);
    }

    public trackByAsset(index: number, asset: AssetDto) {
        return asset.id;
    }
}