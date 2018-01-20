/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable:prefer-for-of

import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Subscription } from 'rxjs';

import {
    AppContext,
    AssetDto,
    AssetsService,
    AssetUpdated,
    ImmutableArray,
    Pager
} from 'shared';

@Component({
    selector: 'sqx-assets-page',
    styleUrls: ['./assets-page.component.scss'],
    templateUrl: './assets-page.component.html',
    providers: [
        AppContext
    ]
})
export class AssetsPageComponent implements OnDestroy, OnInit {
    private assetUpdatedSubscription: Subscription;

    public newFiles = ImmutableArray.empty<File>();

    public assetsItems = ImmutableArray.empty<AssetDto>();
    public assetsPager = new Pager(0, 0, 12);
    public assetsFilter = new FormControl();
    public assertQuery = '';

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
                        this.assetsItems = this.assetsItems.replaceBy('id', event.assetDto);
                    }
                });

        this.load();
    }

    public search() {
        this.assetsPager = new Pager(0, 0, 12);
        this.assertQuery = this.assetsFilter.value;

        this.load();
    }

    public load(showInfo = false) {
       this.assetsService.getAssets(this.ctx.appName, this.assetsPager.pageSize, this.assetsPager.skip, this.assertQuery)
            .subscribe(dtos => {
                this.assetsItems = ImmutableArray.of(dtos.items);
                this.assetsPager = this.assetsPager.setCount(dtos.total);

                if (showInfo) {
                    this.ctx.notifyInfo('Assets reloaded.');
                }
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public onAssetDeleting(asset: AssetDto) {
        this.assetsService.deleteAsset(this.ctx.appName, asset.id, asset.version)
            .subscribe(dto => {
                this.assetsItems = this.assetsItems.filter(x => x.id !== asset.id);
                this.assetsPager = this.assetsPager.decrementCount();
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public onAssetLoaded(file: File, asset: AssetDto) {
        this.newFiles = this.newFiles.remove(file);

        this.assetsItems = this.assetsItems.pushFront(asset);
        this.assetsPager = this.assetsPager.incrementCount();
    }

    public onAssetUpdated(asset: AssetDto) {
        this.ctx.bus.emit(new AssetUpdated(asset, this));
    }

    public onAssetFailed(file: File) {
        this.newFiles = this.newFiles.remove(file);
    }

    public goNext() {
        this.assetsPager = this.assetsPager.goNext();

        this.load();
    }

    public goPrev() {
        this.assetsPager = this.assetsPager.goPrev();

        this.load();
    }

    public addFiles(files: FileList) {
        for (let i = 0; i < files.length; i++) {
            this.newFiles = this.newFiles.pushFront(files[i]);
        }
    }
}

