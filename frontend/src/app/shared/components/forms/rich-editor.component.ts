/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, booleanAttribute, ChangeDetectionStrategy, Component, ElementRef, EventEmitter, forwardRef, Input, OnDestroy, Output, ViewChild } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { ContentDto } from '@app/shared';
import { ApiUrlConfig, AssetDto, AssetUploaderState, DialogModel, getContentValue, LanguageDto, ResourceLoaderService, StatefulControlComponent, Types } from '@app/shared/internal';

export const SQX_RICH_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => RichEditorComponent), multi: true,
};

@Component({
    selector: 'sqx-rich-editor',
    styleUrls: ['./rich-editor.component.scss'],
    templateUrl: './rich-editor.component.html',
    providers: [
        SQX_RICH_EDITOR_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RichEditorComponent extends StatefulControlComponent<{}, string> implements AfterViewInit, OnDestroy {
    private editorWrapper: any;
    private value?: string;
    private currentContents?: ResolvablePromise<any>;
    private currentAssets?: ResolvablePromise<any>;

    @Output()
    public assetPluginClick = new EventEmitter<any>();

    @Input()
    public schemaIds?: ReadonlyArray<string>;

    @Input()
    public language!: LanguageDto;

    @Input()
    public languages!: ReadonlyArray<LanguageDto>;

    @Input()
    public folderId = '';

    @Input({ required: true })
    public mode: SquidexEditorMode = 'Html';

    @Input({ transform: booleanAttribute })
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    @ViewChild('editor', { static: false })
    public editor!: ElementRef;

    public assetsDialog = new DialogModel();

    public contentsDialog = new DialogModel();

    constructor(
        private readonly apiUrl: ApiUrlConfig,
        private readonly assetUploader: AssetUploaderState,
        private readonly resourceLoader: ResourceLoaderService,
    ) {
        super({});
    }

    public ngOnDestroy() {
        if (this.editorWrapper) {
            this.editorWrapper.destroy?.();
            this.editorWrapper = null;
        }
    }

    public ngAfterViewInit() {
        this.resourceLoader.loadLocalStyle('editor/squidex-editor.css');
        this.resourceLoader.loadLocalScript('editor/squidex-editor.js').then(() => {
            this.editorWrapper = new SquidexEditorWrapper(this.editor.nativeElement, {
                value: this.value || '',
                isDisabled: this.snapshot.isDisabled,
                onSelectAssets: async () => {
                    if (this.snapshot.isDisabled) {
                        return;
                    }

                    this.currentAssets = new ResolvablePromise<any>();
                    this.assetsDialog.show();

                    return await this.currentAssets.promise;
                },
                onSelectContents: async () => {
                    if (this.snapshot.isDisabled) {
                        return;
                    }

                    this.currentContents = new ResolvablePromise<any>();
                    this.contentsDialog.show();

                    return await this.currentContents.promise;
                },
                onUpload: (requests: UploadRequest[]) => {
                    return this.uploadFiles(requests);
                },
                onChange: (value: string | undefined) => {
                    this.callChange(value);
                },
                canSelectAssets: true,
                canSelectContents: !!this.schemaIds,
                mode: this.mode,
            });
        });
    }

    public reset() {
        this.ngOnDestroy();

        setTimeout(() => {
            this.ngAfterViewInit();
        });
    }

    public writeValue(obj: any) {
        this.editorWrapper?.setValue(obj);
    }

    public onDisabled() {
        this.editorWrapper?.setIsDisabled(this.snapshot.isDisabled);
    }

    public insertAssets(assets: ReadonlyArray<AssetDto>) {
        this.assetsDialog.hide();

        if (!this.currentAssets) {
            return;
        }

        const items = assets.map(a => this.buildAsset(a));

        this.currentAssets.resolve(items);
        this.currentAssets = undefined;
    }

    public insertContents(contents: ReadonlyArray<ContentDto>) {
        this.contentsDialog.hide();
        if (!this.currentContents) {
            return;
        }

        const items = contents.map(c => this.buildContent(c));

        this.currentContents.resolve(items);
        this.currentContents = undefined;
    }

    private uploadFiles(requests: UploadRequest[]) {
        const uploadFile = (request: UploadRequest) => {
            return new Promise<any>((resolve, reject) => {
                this.assetUploader.uploadFile(request.file, this.folderId)
                    .subscribe({
                        next: value => {
                            if (Types.is(value, AssetDto)) {
                                resolve(this.buildAsset(value));
                            } else {
                                request.progress(value / 100);
                            }
                        },
                        error: reject,
                    });
            });
        };

        return requests.map(r => () => uploadFile(r));
    }

    private buildAsset(asset: AssetDto) {
        return { type: asset.mimeType, src: asset.fullUrl(this.apiUrl), fileName: asset.fileName };
    }

    private buildContent(content: ContentDto) {
        return { url: this.apiUrl.buildUrl(content._links['self'].href), name: buildContentName(content, this.language) };
    }
}

function buildContentName(content: ContentDto, language: LanguageDto) {
    const name =
        content.referenceFields
            .map(f => getContentValue(content, language, f, false))
            .map(v => v.formatted)
            .defined()
            .join(', ');

    return name || 'Content';
}

class ResolvablePromise<T> {
    private resolver?: (value: T) => void;

    public readonly promise = new Promise<T>(resolve => {
        this.resolver = resolve;
    });

    public resolve(value: T) {
        this.resolver?.(value);
    }
}