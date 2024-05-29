/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDropList } from '@angular/cdk/drag-drop';

import { booleanAttribute, ChangeDetectionStrategy, Component, forwardRef, Input, OnInit } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { AssetComponent, AssetDialogComponent, AssetDto, AssetSelectorComponent, ChatDialogComponent, DialogModel, FileDropDirective, HTTP, LocalStoreService, MessageBus, ModalDirective, ResizedDirective, ResolveAssets, Settings, sorted, StatefulControlComponent, Subscriptions, TranslatePipe, Types } from '@app/shared';

export const SQX_ASSETS_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => AssetsEditorComponent), multi: true,
};

class AssetUpdated {
    constructor(
        public readonly asset: AssetDto,
        public readonly source: any,
    ) {
    }
}

interface State {
    // The uploading files.
    assetFiles: ReadonlyArray<HTTP.UploadFile>;

    // The assets to render.
    assetItems: ReadonlyArray<AssetDto>;

    // The asset to edit.
    editAsset?: AssetDto;

    // True when showing the assets as list.
    isListView: boolean;

    // True, when width less than 600 pixels.
    isCompact?: boolean;
}

@Component({
    standalone: true,
    selector: 'sqx-assets-editor',
    styleUrls: ['./assets-editor.component.scss'],
    templateUrl: './assets-editor.component.html',
    providers: [
        SQX_ASSETS_EDITOR_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AssetComponent,
        AssetDialogComponent,
        AssetSelectorComponent,
        CdkDrag,
        CdkDropList,
        ChatDialogComponent,
        FileDropDirective,
        ModalDirective,
        ResizedDirective,
        TranslatePipe,
    ],
})
export class AssetsEditorComponent extends StatefulControlComponent<State, ReadonlyArray<string>> implements OnInit {
    private readonly subscriptions = new Subscriptions();

    @Input()
    public folderId?: string;

    @Input({ required: true, transform: booleanAttribute })
    public hasChatBot = false;

    @Input({ transform: booleanAttribute })
    public isExpanded = false;

    @Input({ transform: booleanAttribute })
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    public chatDialog = new DialogModel();

    public assetsDialog = new DialogModel();

    constructor(localStore: LocalStoreService,
        private readonly assetsResolver: ResolveAssets,
        private readonly messageBus: MessageBus,
    ) {
        super({
            assetItems: [],
            assetFiles: [],
            isListView: localStore.getBoolean(Settings.Local.ASSETS_MODE),
        });

        this.project(x => x.isListView).subscribe(isListView => {
            localStore.setBoolean(Settings.Local.ASSETS_MODE, isListView);
        });
    }

    public writeValue(obj: any) {
        if (Types.isArrayOfString(obj)) {
            if (!Types.equals(obj, this.snapshot.assetItems.map(x => x.id))) {
                const assetIds: string[] = obj;

                this.assetsResolver.resolveMany(obj)
                    .subscribe({
                        next: ({ items }) => {
                            this.setAssets(items);

                            if (this.snapshot.assetItems.length !== assetIds.length) {
                                this.updateValue();
                            }
                        },
                        error: () => {
                            this.setAssets([]);
                        },
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
        this.subscriptions.add(
            this.messageBus.of(AssetUpdated)
                .subscribe(event => {
                    this.setAssets(this.snapshot.assetItems.replacedBy('id', event.asset));
                }));
    }

    public setCompact(isCompact: boolean) {
        this.next({ isCompact });
    }

    public setAssets(assets: ReadonlyArray<AssetDto>) {
        this.next({ assetItems: assets });
    }

    public addBlob(content: string | HTTP.UploadFile | null | undefined) {
        this.chatDialog.hide();

        if (content && !Types.isString(content)) {
            this.addFiles([content]);
        }
    }

    public addAssetFromAI(file: string | HTTP.UploadFile | null | undefined) {
        this.chatDialog.hide();

        if (!Types.isString(file) && file) {
            this.addFiles([file]);
        }
    }

    public addFiles(files: ReadonlyArray<HTTP.UploadFile>) {
        for (const file of files) {
            this.next(s => ({
                ...s,
                assetFiles: [file, ...s.assetFiles],
            }));
        }
    }

    public selectAssets(assets: ReadonlyArray<AssetDto>) {
        this.setAssets([...this.snapshot.assetItems, ...assets]);

        if (assets.length > 0) {
            this.updateValue();
        }

        this.assetsDialog.hide();
    }

    public addAsset(file: HTTP.UploadFile, asset: AssetDto) {
        if (asset && file) {
            this.next(s => ({
                ...s,
                assetFiles: s.assetFiles.removed(file),
                assetItems: [asset, ...s.assetItems],
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
            this.setAssets(this.snapshot.assetItems.removed(asset));

            this.updateValue();
        }
    }

    public removeLoadingAsset(file: HTTP.UploadFile) {
        this.next(s => ({
            ...s,
            assetFiles: s.assetFiles.removed(file),
        }));
    }

    public changeView(isListView: boolean) {
        this.next({ isListView });
    }

    public editStart(asset: AssetDto) {
        this.next({ editAsset: asset });
    }

    public editDone() {
        this.next({ editAsset: undefined });
    }

    private updateValue() {
        const ids = this.snapshot.assetItems.map(x => x.id);

        if (ids.length === 0) {
            this.callChange(null);
        } else {
            this.callChange(ids);
        }

        this.callTouched();
    }
}
