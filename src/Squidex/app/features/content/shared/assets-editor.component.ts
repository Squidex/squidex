/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable:prefer-for-of

import { Component, forwardRef, OnDestroy, OnInit } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Subscription } from 'rxjs';

import {
    AppContext,
    AssetDto,
    AssetsService,
    AssetUpdated,
    ImmutableArray,
    Types
} from 'shared';

export const SQX_ASSETS_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => AssetsEditorComponent), multi: true
};

@Component({
    selector: 'sqx-assets-editor',
    styleUrls: ['./assets-editor.component.scss'],
    templateUrl: './assets-editor.component.html',
    providers: [
        AppContext,
        SQX_ASSETS_EDITOR_CONTROL_VALUE_ACCESSOR
    ]
})
export class AssetsEditorComponent implements ControlValueAccessor, OnDestroy, OnInit {
    private assetUpdatedSubscription: Subscription;
    private callChange = (v: any) => { /* NOOP */ };
    private callTouched = () => { /* NOOP */ };

    public newAssets = ImmutableArray.empty<File>();
    public oldAssets = ImmutableArray.empty<AssetDto>();

    public isDisabled = false;

    constructor(public readonly ctx: AppContext,
        private readonly assetsService: AssetsService
    ) {
    }

    public ngOnDestroy() {
        this.assetUpdatedSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.assetUpdatedSubscription =
            this.ctx.bus.of(AssetUpdated)
                .subscribe(event => {
                    if (event.sender !== this) {
                        this.oldAssets = this.oldAssets.replaceBy('id', event.assetDto);
                    }
                });
    }

    public writeValue(value: string[]) {
        this.oldAssets = ImmutableArray.empty<AssetDto>();

        if (Types.isArrayOfString(value) && value.length > 0) {
            const assetIds: string[] = value;

            this.assetsService.getAssets(this.ctx.appName, 0, 0, undefined, value)
                .subscribe(dtos => {
                    this.oldAssets = ImmutableArray.of(assetIds.map(id => dtos.items.find(x => x.id === id)).filter(a => !!a).map(a => a!));
                });
        }
    }

    public setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;
    }

    public registerOnChange(fn: any) {
        this.callChange = fn;
    }

    public registerOnTouched(fn: any) {
        this.callTouched = fn;
    }

    public addFiles(files: FileList) {
        for (let i = 0; i < files.length; i++) {
            this.newAssets = this.newAssets.pushFront(files[i]);
        }
    }

    public canDrop() {
        const component = this;

        return (dragData: any) => {
            return !component.isDisabled && dragData instanceof AssetDto && !component.oldAssets.find(a => a.id === dragData.id);
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
        this.ctx.bus.emit(new AssetUpdated(asset, this));
    }

    public onAssetFailed(file: File) {
        this.newAssets = this.newAssets.remove(file);
    }

    private updateValue() {
        let ids: string[] | null = this.oldAssets.values.map(x => x.id);

        if (ids.length === 0) {
            ids = null;
        }

        this.callTouched();
        this.callChange(ids);
    }
}