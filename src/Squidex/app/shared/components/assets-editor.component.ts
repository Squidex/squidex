/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

// tslint:disable:prefer-for-of

import { Component, forwardRef, OnDestroy, OnInit } from '@angular/core';
import { ControlValueAccessor,  NG_VALUE_ACCESSOR } from '@angular/forms';
import { Subscription } from 'rxjs';

import { AppComponentBase } from './app.component-base';

import {
    AppsStoreService,
    AssetDto,
    AssetsService,
    AssetUpdated,
    ImmutableArray,
    MessageBus,
    NotificationService
} from './../declarations-base';

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

    public newAssets = ImmutableArray.empty<File>();
    public oldAssets = ImmutableArray.empty<AssetDto>();

    private changeCallback: (value: any) => void = NOOP;
    private touchedCallback: () => void = NOOP;

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
                        this.oldAssets =
                            this.oldAssets.replaceAll(
                                a => a.id === event.assetDto.id,
                                a => event.assetDto);
                    }
                });
    }

    public ngOnDestroy() {
        this.assetUpdatedSubscription.unsubscribe();
    }

    public writeValue(value: any) {
        this.oldAssets = ImmutableArray.empty<AssetDto>();

        if (value) {
            this.appNameOnce()
                .switchMap(app => this.assetsService.getAssets(app, 10000, 0, undefined, undefined, value))
                .subscribe(dtos => {
                    this.oldAssets = ImmutableArray.of(dtos.items);
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

    public onAssetLoaded(file: File, asset: AssetDto) {
        this.newAssets = this.newAssets.remove(file);
        this.oldAssets = this.oldAssets.pushFront(asset);

        this.updateValue();
    }

    public onAssetDropped(asset: AssetDto) {
        this.oldAssets = this.oldAssets.pushFront(asset);

        this.updateValue();
    }

    public onAssetRemoving(asset: AssetDto) {
        this.oldAssets = this.oldAssets.remove(asset);

        this.updateValue();
    }

    public onAssetUpdated(asset: AssetDto) {
        this.messageBus.publish(new AssetUpdated(asset, this));
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