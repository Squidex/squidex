/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDropList } from '@angular/cdk/drag-drop';

import { booleanAttribute, ChangeDetectionStrategy, Component, effect, forwardRef, input, signal } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { AssetComponent, AssetDialogComponent, AssetDto, AssetSelectorComponent, ChatDialogComponent, DialogModel, FileDropDirective, HTTP, LocalStoreService, MessageBus, ModalDirective, ResizedDirective, ResolveAssets, Settings, SignalControlComponent, sorted, TranslatePipe, Types } from '@app/shared';

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

@Component({
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
    host: {
        '[class.expanded]': 'isExpanded()',
    },
})
export class AssetsEditorComponent extends SignalControlComponent<ReadonlyArray<string>> {
    /** The folder to upload new assets to. */
    public readonly folderId = input<string>();

    /** Enables the AI features to generate images. */
    public readonly hasChatBot = input.required<boolean, unknown>({ transform: booleanAttribute });

    /** Renders the assets in fullscreen mode. */
    public readonly isExpanded = input(false, { transform: booleanAttribute });

    public readonly assetFiles = signal<ReadonlyArray<HTTP.UploadFile>>([]);
    public readonly assetItems = signal<ReadonlyArray<AssetDto>>([]);
    public readonly assetEdit = signal<AssetDto | undefined>(undefined);

    public readonly isListView = signal(!this.localStore.getBoolean(Settings.Local.ASSETS_MODE));
    public readonly isCompact = signal(false);

    public readonly chatDialog = new DialogModel();

    public readonly assetsDialog = new DialogModel();

    constructor(
        private readonly localStore: LocalStoreService,
        private readonly assetsResolver: ResolveAssets,
        private readonly messageBus: MessageBus,
    ) {
        super();

        effect(onCleanup => {
            const subscription =
                this.messageBus.of(AssetUpdated)
                    .subscribe(event => {
                        this.setAssets(this.assetItems().replacedBy('id', event.asset));
                    });

            onCleanup(() => subscription.unsubscribe());
        });
    }

    public writeValue(obj: any) {
        if (!Types.isArrayOfString(obj)) {
            this.setAssets([]);
            return;
        }

        if (Types.equals(obj, this.assetItems().map(x => x.id))) {
            return;
        }

        const assetIds: string[] = obj;

        this.assetsResolver.resolveMany(obj)
            .subscribe({
                next: ({ items }) => {
                    this.setAssets(items);

                    if (this.assetItems().length !== assetIds.length) {
                        this.updateValue();
                    }
                },
                error: () => {
                    this.setAssets([]);
                },
            });
    }

    public notifyOthers(asset: AssetDto) {
        this.messageBus.emit(new AssetUpdated(asset, this));
    }

    public setCompact(isCompact: boolean) {
        this.isCompact.set(isCompact);
    }

    public setAssets(assets: ReadonlyArray<AssetDto>) {
        this.assetItems.set(assets);
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
            this.assetFiles.update(x => [file, ...x]);
        }
    }

    public selectAssets(assets: ReadonlyArray<AssetDto>) {
        this.setAssets([...this.assetItems(), ...assets]);

        if (assets.length > 0) {
            this.updateValue();
        }

        this.assetsDialog.hide();
    }

    public addAsset(file: HTTP.UploadFile, asset: AssetDto) {
        if (asset && file) {
            this.assetFiles.update(x => x.removed(file));
            this.assetItems.update(x => [asset, ...x]);

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
            this.setAssets(this.assetItems().removed(asset));

            this.updateValue();
        }
    }

    public removeLoadingAsset(file: HTTP.UploadFile) {
        this.assetFiles.update(x => x.removed(file));
    }

    public changeView(isListView: boolean) {
        this.isListView.set(isListView);

        this.localStore.setBoolean(Settings.Local.ASSETS_MODE, !isListView);
    }

    public editStart(asset: AssetDto) {
        this.assetEdit.set(asset);
    }

    public editDone() {
        this.assetEdit.set(undefined);
    }

    private updateValue() {
        const ids = this.assetItems().map(x => x.id);

        if (ids.length === 0) {
            this.callChange(null);
        } else {
            this.callChange(ids);
        }

        this.callTouched();
    }
}
