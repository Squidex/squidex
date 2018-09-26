/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, HostBinding, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { FormBuilder, FormControl } from '@angular/forms';
import { Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

import {
    AppsState,
    AssetDto,
    AssetsService,
    AuthService,
    DateTime,
    DialogService,
    fadeAnimation,
    RenameAssetDto,
    RenameAssetForm,
    TagAssetDto,
    Types,
    Versioned
} from '@app/shared/internal';

@Component({
    selector: 'sqx-asset',
    styleUrls: ['./asset.component.scss'],
    templateUrl: './asset.component.html',
    animations: [
        fadeAnimation
    ]
})
export class AssetComponent implements OnDestroy, OnInit {
    private tagSubscription: Subscription;

    @Input()
    public initFile: File;

    @Input()
    public asset: AssetDto;

    @Input()
    public removeMode = false;

    @Input()
    public isDisabled = false;

    @Input()
    public isSelected = false;

    @Input()
    public isSelectable = false;

    @Input() @HostBinding('class.isListView')
    public isListView = false;

    @Input()
    public allTags: string[];

    @Output()
    public loaded = new EventEmitter<AssetDto>();

    @Output()
    public removing = new EventEmitter<AssetDto>();

    @Output()
    public updated = new EventEmitter<AssetDto>();

    @Output()
    public deleting = new EventEmitter<AssetDto>();

    @Output()
    public selected = new EventEmitter<AssetDto>();

    @Output()
    public failed = new EventEmitter();

    public isTagging = false;

    public renaming = false;
    public renameForm = new RenameAssetForm(this.formBuilder);

    public tagInput = new FormControl();

    public progress = 0;

    constructor(
        private readonly appsState: AppsState,
        private readonly assetsService: AssetsService,
        private readonly authState: AuthService,
        private readonly dialogs: DialogService,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        const initFile = this.initFile;

        if (initFile) {
            this.assetsService.uploadFile(this.appsState.appName, initFile, this.authState.user!.token, DateTime.now())
                .subscribe(dto => {
                    if (Types.is(dto, AssetDto)) {
                        this.emitLoaded(dto);
                    } else {
                        this.progress = dto;
                    }
                }, error => {
                    this.dialogs.notifyError(error);

                    this.emitFailed(error);
                });
        } else {
            this.updateAsset(this.asset, false);
        }

        if (this.isDisabled) {
            this.tagInput.disable();
        }

        this.tagSubscription =
            this.tagInput.valueChanges.pipe(
                distinctUntilChanged(),
                debounceTime(2000)
            ).subscribe(tags => {
                this.tagAsset(tags);
            });
    }

    public ngOnDestroy() {
        this.tagSubscription.unsubscribe();
    }

    public updateFile(files: FileList) {
        if (files.length === 1) {
            this.assetsService.replaceFile(this.appsState.appName, this.asset.id, files[0], this.asset.version)
                .subscribe(dto => {
                    if (Types.is(dto, Versioned)) {
                        this.updateAsset(this.asset.update(dto.payload, this.authState.user!.token, dto.version), true);
                    } else {
                        this.setProgress(dto);
                    }
                }, error => {
                    this.dialogs.notifyError(error);

                    this.setProgress();
                });
        }
    }

    public renameAsset() {
        const value = this.renameForm.submit(this.asset);

        if (value) {
            const requestDto = new RenameAssetDto(value.name);

            this.assetsService.putAsset(this.appsState.appName, this.asset.id, requestDto, this.asset.version)
                .subscribe(dto => {
                    this.updateAsset(this.asset.rename(requestDto.fileName, this.authState.user!.token, dto.version), true);

                    this.renameCancel();
                }, error => {
                    this.dialogs.notifyError(error);

                    this.renameForm.submitFailed(error);
                });
        }
    }

    public tagAsset(tags: string[]) {
        if (tags) {
            const requestDto = new TagAssetDto(tags);

            this.assetsService.putAsset(this.appsState.appName, this.asset.id, requestDto, this.asset.version)
                .subscribe(dto => {
                    this.updateAsset(this.asset.tag(tags, this.authState.user!.token, dto.version), true);
                }, error => {
                    this.dialogs.notifyError(error);
                });
        }
    }

    public renameStart() {
        if (!this.isDisabled) {
            this.renameForm.load(this.asset);
            this.renaming = true;
        }
    }

    public renameCancel() {
        this.renameForm.submitCompleted();
        this.renaming = false;
    }

    private setProgress(progress = 0) {
        this.progress = progress;
    }

    private emitFailed(error: any) {
        this.failed.emit(error);
    }

    private emitLoaded(asset: AssetDto) {
        this.loaded.emit(asset);
    }

    private emitUpdated(asset: AssetDto) {
        this.updated.emit(asset);
    }

    private updateAsset(asset: AssetDto, emitEvent: boolean) {
        this.asset = asset;
        this.progress = 0;

        this.tagInput.setValue(asset.tags);

        if (emitEvent) {
            this.emitUpdated(asset);
        }

        this.renameCancel();
    }
}