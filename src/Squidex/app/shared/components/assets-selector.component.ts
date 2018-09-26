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
    fadeAnimation,
    LocalStoreService
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

    public isListView = false;

    @Output()
    public selected = new EventEmitter<AssetDto[]>();

    constructor(
        public readonly state: AssetsDialogState,
        private readonly localStore: LocalStoreService
    ) {
        this.isListView = this.localStore.get('assetView') === 'List';
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

    public selectTags(tags: string[]) {
        this.state.selectTags(tags).pipe(onErrorResumeNext()).subscribe();
    }

    public selectAsset(asset: AssetDto) {
        if (this.selectedAssets[asset.id]) {
            delete this.selectedAssets[asset.id];
        } else {
            this.selectedAssets[asset.id] = asset;
        }

        this.selectionCount = Object.keys(this.selectedAssets).length;
    }

    public changeView(isListView: boolean) {
        this.localStore.set('assetView', isListView ? 'List' : 'Grid');

        this.isListView = isListView;
    }
}

