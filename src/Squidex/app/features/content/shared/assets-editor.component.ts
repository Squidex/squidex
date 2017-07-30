/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

// tslint:disable:prefer-for-of

import { Component, forwardRef, OnDestroy, OnInit } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Subscription } from 'rxjs';

import {
    AppComponentBase,
    AppsStoreService,
    AssetDto,
    AssetsService,
    AssetUpdated,
    ImmutableArray,
    MessageBus,
    NotificationService
} from 'shared';

const NOOP = () => { /* NOOP */ };

export const SQX_ASSETS_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => AssetsEditorComponent), multi: true
};

@Component({
    selector: 'sqx-assets-editor',
    styleUrls: ['./assets-editor.component.scss'],
    templateUrl: './assets-editor.component.html',
    providers: [SQX_ASSETS_EDITOR_CONTROL_VALUE_ACCESSOR]
})
export class AssetsEditorComponent extends AppComponentBase implements ControlValueAccessor, OnDestroy, OnInit {
    private assetUpdatedSubscription: Subscription;
    private changeCallback: (value: any) => void = NOOP;
    private touchedCallback: () => void = NOOP;

    public newAssets = ImmutableArray.empty<File>();
    public oldAssets = ImmutableArray.empty<AssetDto>();

    public isDisabled = false;

    constructor(apps: AppsStoreService, notifications: NotificationService,
        private readonly assetsService: AssetsService,
        private readonly messageBus: MessageBus
    ) {
        super(notifications, apps);
    }

    public ngOnInit() {
        this.assetUpdatedSubscription =
            this.messageBus.of(AssetUpdated)
                .subscribe(event => {
                    if (event.sender !== this) {
                        this.oldAssets = this.oldAssets.replaceBy('id', event.assetDto);
                    }
                });
    }

    public ngOnDestroy() {
        this.assetUpdatedSubscription.unsubscribe();
    }

    public writeValue(value: any) {
        this.oldAssets = ImmutableArray.empty<AssetDto>();

        if (value && value.length > 0) {
            const assetIds: string[] = value;

            this.appNameOnce()
                .switchMap(app => this.assetsService.getAssets(app, 10000, 0, undefined, undefined, value))
                .subscribe(dtos => {
                    this.oldAssets = ImmutableArray.of(assetIds.map(id => dtos.items.find(x => x.id === id)));
                });
        }
    }

    public setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;
    }

    public registerOnChange(fn: any) {
        this.changeCallback = fn;
    }

    public registerOnTouched(fn: any) {
        this.touchedCallback = fn;
    }

    public addFiles(files: FileList) {
        for (let i = 0; i < files.length; i++) {
            this.newAssets = this.newAssets.pushFront(files[i]);
        }
    }

    public canDrop() {
        const component = this;

        return (dragData: any) => {
            return dragData instanceof AssetDto && !component.oldAssets.find(a => a.id === dragData.id);
        };
    }

    public onAssetDropped(asset: AssetDto) {
        if (asset) {
            this.oldAssets = this.oldAssets.pushFront(asset);

            this.updateValue();
        }
    }

    public onAssetRemoving(asset: AssetDto) {
        if (asset) {
            this.oldAssets = this.oldAssets.remove(asset);

            this.updateValue();
        }
    }

    public onAssetLoaded(file: File, asset: AssetDto) {
        this.newAssets = this.newAssets.remove(file);
        this.oldAssets = this.oldAssets.pushFront(asset);

        this.updateValue();
    }

    public onAssetUpdated(asset: AssetDto) {
        this.messageBus.emit(new AssetUpdated(asset, this));
    }

    public onAssetFailed(file: File) {
        this.newAssets = this.newAssets.remove(file);
    }

    private updateValue() {
        let ids: string[] | null = this.oldAssets.values.map(x => x.id);

        if (ids.length === 0) {
            ids = null;
        }

        this.touchedCallback();
        this.changeCallback(ids);
    }
}