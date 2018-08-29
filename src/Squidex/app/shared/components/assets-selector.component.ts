/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable:prefer-for-of

import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { onErrorResumeNext } from 'rxjs/operators';

import {
    AssetDto,
    AssetsDialogState,
    fadeAnimation
} from '@app/shared/internal';

@Component({
    selector: 'sqx-assets-selector',
    styleUrls: ['./assets-selector.component.scss'],
    templateUrl: './assets-selector.component.html',
    animations: [
        fadeAnimation
    ]
})
export class AssetsSelectorComponent implements OnInit {
    public selectedAssets: { [id: string]: AssetDto } = {};
    public selectionCount = 0;

    @Output()
    public selected = new EventEmitter<AssetDto[]>();

    constructor(
        public readonly state: AssetsDialogState
    ) {
    }

    public ngOnInit() {
        this.state.load().pipe(onErrorResumeNext()).subscribe();
    }

    public reload() {
        this.state.load(true).pipe(onErrorResumeNext()).subscribe();
    }

    public search(query: string) {
        this.state.search(query).pipe(onErrorResumeNext()).subscribe();
    }

    public complete() {
        this.selected.emit([]);
    }

    public select() {
        this.selected.emit(Object.values(this.selectedAssets));
    }

    public selectAsset(asset: AssetDto) {
        if (this.selectedAssets[asset.id]) {
            delete this.selectedAssets[asset.id];
        } else {
            this.selectedAssets[asset.id] = asset;
        }

        this.selectionCount = Object.keys(this.selectedAssets).length;
    }
}

