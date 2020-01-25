/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, Input, OnInit } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';

import {
    AppsState,
    AssetDto,
    AssetsService,
    DialogModel,
    LocalStoreService,
    MessageBus,
    sorted,
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
    assetFiles: ReadonlyArray<File>;

    assets: ReadonlyArray<AssetDto>;

    isListView: boolean;
}

@Component({
    selector: 'sqx-assets-editor',
    styleUrls: ['./assets-editor.component.scss'],
    templateUrl: './assets-editor.component.html',
    providers: [
        SQX_ASSETS_EDITOR_CONTROL_VALUE_ACCESSOR
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetsEditorComponent extends StatefulControlComponent<State, ReadonlyArray<string>> implements OnInit {
    @Input()
    public isCompact = false;

    public assetsDialog = new DialogModel();

    constructor(changeDetector: ChangeDetectorRef,
        private readonly appsState: AppsState,
        private readonly assetsService: AssetsService,
        private readonly localStore: LocalStoreService,
        private readonly messageBus: MessageBus
    ) {
        super(changeDetector, {
            assets: [],
            assetFiles: [],
            isListView: localStore.getBoolean('squidex.assets.list-view')
        });
    }

    public writeValue(obj: any) {
        if (Types.isArrayOfString(obj)) {
            if (!Types.equals(obj, this.snapshot.assets.map(x => x.id))) {
                const assetIds: string[] = obj;

                this.assetsService.getAssets(this.appsState.appName, 0, 0, undefined, undefined, obj)
                    .subscribe(dtos => {
                        this.setAssets(assetIds.map(id => dtos.items.find(x => x.id === id)!).filter(a => !!a));

                        if (this.snapshot.assets.length !== assetIds.length) {
                            this.updateValue();
                        }
                    }, () => {
                        this.setAssets([]);
                    });
            }
        } else {
            this.setAssets([]);
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

    public setAssets(assets: ReadonlyArray<AssetDto>) {
        this.next(s => ({ ...s, assets }));
    }

    public addFiles(files: ReadonlyArray<File>) {
        for (const file of files) {
            this.next(s => ({ ...s, assetFiles: [file, ...s.assetFiles] }));
        }
    }

    public selectAssets(assets: ReadonlyArray<AssetDto>) {
        this.setAssets([...this.snapshot.assets, ...assets]);

        if (assets.length > 0) {
            this.updateValue();
        }

        this.assetsDialog.hide();
    }

    public addAsset(file: File, asset: AssetDto) {
        if (asset && file) {
            this.next(s => ({
                ...s,
                assetFiles: s.assetFiles.removed(file),
                assets: [asset, ...s.assets]
            }));

            this.updateValue();
        }
    }

    public sortAssets(event: CdkDragDrop<ReadonlyArray<AssetDto>>) {
        if (event) {
            this.setAssets(sorted(event));

            this.updateValue();
        }
    }

    public removeLoadedAsset(asset: AssetDto) {
        if (asset) {
            this.setAssets(this.snapshot.assets.removed(asset));

            this.updateValue();
        }
    }

    public removeLoadingAsset(file: File) {
        this.next(s => ({ ...s, assetFiles: s.assetFiles.removed(file) }));
    }

    public changeView(isListView: boolean) {
        this.next(s => ({ ...s, isListView }));

        this.localStore.setBoolean('squidex.assets.list-view', isListView);
    }

    private updateValue() {
        const ids = this.snapshot.assets.map(x => x.id);

        if (ids.length === 0) {
            this.callChange(null);
        } else {
            this.callChange(ids);
        }

        this.callTouched();
    }

    public trackByAsset(index: number, asset: AssetDto) {
        return asset.id;
    }
}