/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, HostBinding, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormControl } from '@angular/forms';
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
    StatefulComponent,
    TagAssetDto,
    Types,
    Versioned
} from '@app/shared/internal';

interface State {
    isTagging: boolean;
    isRenaming: boolean;

    progress: number;
}

@Component({
    selector: 'sqx-asset',
    styleUrls: ['./asset.component.scss'],
    templateUrl: './asset.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetComponent extends StatefulComponent<State> implements OnChanges, OnInit {
    @Input()
    public initFile: File;

    @Input()
    public asset: AssetDto;

    @Input()
    public removeMode = false;

    @Input()
    public isCompact = false;

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

    public renameForm = new RenameAssetForm(this.formBuilder);

    public tagInput = new FormControl();

    constructor(changeDetector: ChangeDetectorRef,
        private readonly appsState: AppsState,
        private readonly assetsService: AssetsService,
        private readonly authState: AuthService,
        private readonly dialogs: DialogService,
        private readonly formBuilder: FormBuilder
    ) {
        super(changeDetector, {
            isRenaming: false,
            isTagging: false,
            progress: 0
        });
    }

    public ngOnInit() {
        const initFile = this.initFile;

        if (initFile) {
            this.setProgress(1);

            this.assetsService.uploadFile(this.appsState.appName, initFile, this.authState.user!.token, DateTime.now())
                .subscribe(dto => {
                    if (Types.is(dto, AssetDto)) {
                        this.emitLoaded(dto);
                    } else {
                        this.setProgress(dto);
                    }
                }, error => {
                    this.dialogs.notifyError(error);

                    this.emitFailed(error);
                });
        }

        this.own(
            this.tagInput.valueChanges.pipe(
                distinctUntilChanged(),
                debounceTime(2000)
            ).subscribe(tags => {
                this.tagAsset(tags);
            }));
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['asset'] && this.asset) {
            this.tagInput.setValue(this.asset.tags, { emitEvent: false });
        }
    }

    public updateFile(files: FileList) {
        if (files.length === 1) {
            this.setProgress(1);

            this.assetsService.replaceFile(this.appsState.appName, this.asset.id, files[0], this.asset.version)
                .subscribe(dto => {
                    if (Types.is(dto, Versioned)) {
                        this.updateAsset(this.asset.update(dto.payload, this.authState.user!.token, dto.version), true);
                    } else {
                        this.setProgress(dto);
                    }
                }, error => {
                    this.dialogs.notifyError(error);

                    this.setProgress(0);
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

            this.next(s => ({ ...s, isRenaming: true }));
        }
    }

    public renameCancel() {
        this.renameForm.submitCompleted();

        this.next(s => ({ ...s, isRenaming: false }));
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

    private setProgress(progress: number) {
        this.next(s => ({ ...s, progress }));
    }

    private updateAsset(asset: AssetDto, emitEvent: boolean) {
        this.asset = asset;

        this.tagInput.setValue(asset.tags, { emitEvent: false });

        if (emitEvent) {
            this.emitUpdated(asset);
        }

        this.renameCancel();

        this.next(s => ({ ...s, progress: 0 }));
    }
}