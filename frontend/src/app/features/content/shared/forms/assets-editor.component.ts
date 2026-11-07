/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDropList } from '@angular/cdk/drag-drop';

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, effect, inject, input, model, signal, untracked } from '@angular/core';
import { FormValueControl } from '@angular/forms/signals';
import { AssetComponent, AssetDialogComponent, AssetDto, AssetSelectorComponent, ChatDialogComponent, DialogModel, FileDropDirective, HTTP, LocalStoreService, MessageBus, ModalDirective, ResizedDirective, ResolveAssets, Settings, sorted, TranslatePipe, Types } from '@app/shared';

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
export class AssetsEditorComponent implements FormValueControl<ReadonlyArray<string> | null> {
    private readonly assetsResolver = inject(ResolveAssets);
    private readonly localStore = inject(LocalStoreService);
    private readonly messageBus = inject(MessageBus);

    /** The value of the control. */
    public readonly value = model<ReadonlyArray<string> | null>(null);

    /** Disables the control. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    /** Indicates whether the control has been touched. */
    public readonly touched = model(false);

    /** The folder to upload new assets to. */
    public readonly folderId = input<string>();

    /** Enables the AI features to generate images. */
    public readonly hasChatBot = input.required<boolean, unknown>({ transform: booleanAttribute });

    /** Renders the assets in fullscreen mode. */
    public readonly isExpanded = input(false, { transform: booleanAttribute });

    protected readonly assetFiles = signal<ReadonlyArray<HTTP.UploadFile>>([]);

    protected readonly assetItems = signal<ReadonlyArray<AssetDto>>([]);

    protected readonly assetEdit = signal<AssetDto | undefined>(undefined);

    protected readonly isCompact = signal(false);

    protected readonly isTileView = this.localStore.boolean(Settings.Local.ASSETS_MODE);

    protected readonly isListView = computed(() => {
        return !this.isTileView();
    });

    protected readonly chatDialog = new DialogModel();

    protected readonly assetsDialog = new DialogModel();

    constructor() {
        this.messageBus.on(AssetUpdated, event => {
            this.setAssets(this.assetItems().replacedBy('id', event.asset));
        });

        effect(onCleanup => {
            const assetIds = this.value();

            untracked(() => {
                if (!Types.isArrayOfString(assetIds)) {
                    this.setAssets([]);
                    return;
                }

                if (Types.equals(assetIds, this.assetItems().map(x => x.id))) {
                    return;
                }

                const subscription =
                    this.assetsResolver.resolveMany(assetIds)
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

                onCleanup(() => subscription.unsubscribe());
            });
        });
    }

    protected notifyOthers(asset: AssetDto) {
        this.messageBus.emit(new AssetUpdated(asset, this));
    }

    protected setCompact(isCompact: boolean) {
        this.isCompact.set(isCompact);
    }

    private setAssets(assets: ReadonlyArray<AssetDto>) {
        this.assetItems.set(assets);
    }

    public addBlob(content: string | HTTP.UploadFile | null | undefined) {
        this.chatDialog.hide();

        if (content && !Types.isString(content)) {
            this.addFiles([content]);
        }
    }

    protected addAssetFromAI(file: string | HTTP.UploadFile | null | undefined) {
        this.chatDialog.hide();

        if (!Types.isString(file) && file) {
            this.addFiles([file]);
        }
    }

    protected addFiles(files: ReadonlyArray<HTTP.UploadFile>) {
        for (const file of files) {
            this.assetFiles.update(x => [file, ...x]);
        }
    }

    protected selectAssets(assets: ReadonlyArray<AssetDto>) {
        this.setAssets([...this.assetItems(), ...assets]);

        if (assets.length > 0) {
            this.updateValue();
        }

        this.assetsDialog.hide();
    }

    protected addAsset(file: HTTP.UploadFile, asset: AssetDto) {
        if (asset && file) {
            this.assetFiles.update(x => x.removed(file));
            this.assetItems.update(x => [asset, ...x]);

            this.updateValue();
        }
    }

    protected sortAssets(event: CdkDragDrop<ReadonlyArray<AssetDto>>) {
        if (event) {
            this.setAssets(sorted(event));

            this.updateValue();
        }
    }

    protected removeLoadedAsset(asset: AssetDto) {
        if (asset) {
            this.setAssets(this.assetItems().removed(asset));

            this.updateValue();
        }
    }

    protected removeLoadingAsset(file: HTTP.UploadFile) {
        this.assetFiles.update(x => x.removed(file));
    }

    protected changeView(isListView: boolean) {
        this.isTileView.set(!isListView);
    }

    protected editStart(asset: AssetDto) {
        this.assetEdit.set(asset);
    }

    protected editDone() {
        this.assetEdit.set(undefined);
    }

    private updateValue() {
        const ids = this.assetItems().map(x => x.id);

        if (ids.length === 0) {
            this.value.set(null);
        } else {
            this.value.set(ids);
        }

        this.touched.set(true);
    }
}
