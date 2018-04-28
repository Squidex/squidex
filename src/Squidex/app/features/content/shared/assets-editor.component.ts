/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable:prefer-for-of

import { Component, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

import {
    AppsState,
    AssetDto,
    AssetsService,
    ImmutableArray,
    ModalView,
    Types
} from '@app/shared';

export const SQX_ASSETS_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => AssetsEditorComponent), multi: true
};

@Component({
    selector: 'sqx-assets-editor',
    styleUrls: ['./assets-editor.component.scss'],
    templateUrl: './assets-editor.component.html',
    providers: [
        SQX_ASSETS_EDITOR_CONTROL_VALUE_ACCESSOR
    ]
})
export class AssetsEditorComponent implements ControlValueAccessor {
    private callChange = (v: any) => { /* NOOP */ };
    private callTouched = () => { /* NOOP */ };

    public selectorModal = new ModalView();

    public newAssets = ImmutableArray.empty<File>();
    public oldAssets = ImmutableArray.empty<AssetDto>();

    public isDisabled = false;

    constructor(
        private readonly appsState: AppsState,
        private readonly assetsService: AssetsService
    ) {
    }

    public writeValue(value: string[]) {
        if (Types.isArrayOfString(value) && !Types.isEquals(value, this.oldAssets.map(x => x.id).values)) {
            const assetIds: string[] = value;

            this.assetsService.getAssets(this.appsState.appName, 0, 0, undefined, value)
                .subscribe(dtos => {
                    this.oldAssets = ImmutableArray.of(assetIds.map(id => dtos.items.find(x => x.id === id)).filter(a => !!a).map(a => a!));

                    if (this.oldAssets.length !== assetIds.length) {
                        this.updateValue();
                    }
                }, () => {
                    this.oldAssets = ImmutableArray.empty<AssetDto>();
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

    public onAssetsSelected(assets: AssetDto[]) {
        for (let asset of assets) {
            this.oldAssets = this.oldAssets.push(asset);
        }

        if (assets.length > 0) {
            this.updateValue();
        }

        this.selectorModal.hide();
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