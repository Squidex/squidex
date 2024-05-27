/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { AfterViewInit, booleanAttribute, ChangeDetectionStrategy, Component, ElementRef, EventEmitter, forwardRef, Input, OnDestroy, Output, ViewChild } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { BehaviorSubject, catchError, of, switchMap } from 'rxjs';
import { HTTP, ModalDirective, TypedSimpleChanges } from '@app/framework';
import { ApiUrlConfig, AppsState, AssetDto, AssetsService, AssetUploaderState, ContentDto, DialogModel, getContentValue, LanguageDto, ResourceLoaderService, StatefulControlComponent, Types } from '@app/shared/internal';
import { AssetDialogComponent } from '../assets/asset-dialog.component';
import { AssetSelectorComponent } from '../assets/asset-selector.component';
import { ChatDialogComponent } from '../chat-dialog.component';
import { ContentSelectorComponent } from '../references/content-selector.component';

export const SQX_RICH_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => RichEditorComponent), multi: true,
};

@Component({
    standalone: true,
    selector: 'sqx-rich-editor',
    styleUrls: ['./rich-editor.component.scss'],
    templateUrl: './rich-editor.component.html',
    providers: [
        SQX_RICH_EDITOR_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AssetDialogComponent,
        AssetSelectorComponent,
        AsyncPipe,
        ChatDialogComponent,
        ContentSelectorComponent,
        ModalDirective,
    ],
})
export class RichEditorComponent extends StatefulControlComponent<{}, EditorValue> implements AfterViewInit, OnDestroy {
    private readonly assetId = new BehaviorSubject<string | null>(null);
    private editorWrapper?: SquidexEditorWrapper;
    private value?: string;
    private currentContents?: ResolvablePromise<any>;
    private currentAssets?: ResolvablePromise<any>;
    private currentChat?: ResolvablePromise<string | undefined | null>;

    @Output()
    public assetPluginClick = new EventEmitter<any>();

    @Output()
    public annotationsCreate = new EventEmitter<AnnotationSelection>();

    @Output()
    public annotationsUpdate = new EventEmitter<ReadonlyArray<Annotation>>();

    @Output()
    public annotationsSelect = new EventEmitter<ReadonlyArray<string>>();

    @Input({ required: true })
    public hasChatBot = false;

    @Input()
    public hasAnnotations = false;

    @Input()
    public annotations?: ReadonlyArray<Annotation> | null;

    @Input()
    public schemaIds?: ReadonlyArray<string>;

    @Input()
    public language!: LanguageDto;

    @Input()
    public languages!: ReadonlyArray<LanguageDto>;

    @Input()
    public folderId = '';

    @Input({ required: true })
    public classNames?: ReadonlyArray<string>;

    @Input({ required: true })
    public mode: SquidexEditorMode = 'Html';

    @Input({ transform: booleanAttribute })
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    @ViewChild('editor', { static: false })
    public editor!: ElementRef;

    public chatDialog = new DialogModel();

    public assetsDialog = new DialogModel();
    public assetToEdit = this.assetId.pipe(
        switchMap(id => {
            if (id) {
                return this.assetService.getAsset(this.appsState.appName, id);
            } else {
                return of<AssetDto | null>(null);
            }
        }),
        catchError(() => of<AssetDto | null>(null)));

    public contentsDialog = new DialogModel();

    constructor(
        private readonly apiUrl: ApiUrlConfig,
        private readonly appsState: AppsState,
        private readonly assetUploader: AssetUploaderState,
        private readonly assetService: AssetsService,
        private readonly resourceLoader: ResourceLoaderService,
    ) {
        super({});
    }

    public ngOnDestroy() {
        if (this.editorWrapper) {
            this.editorWrapper.destroy?.();
            this.editorWrapper = undefined;
        }
    }

    public ngOnChanges(changes: TypedSimpleChanges<RichEditorComponent>) {
        if (changes.annotations) {
            this.editorWrapper?.setAnnotations(this.annotations);
        }
    }

    public async ngAfterViewInit() {
        await Promise.all([
            this.resourceLoader.loadLocalStyle('editor/squidex-editor.css'),
            this.resourceLoader.loadLocalScript('editor/squidex-editor.js'),
        ]);

        this.editorWrapper = new SquidexEditorWrapper(this.editor.nativeElement, {
            onSelectAIText: async () => {
                if (this.snapshot.isDisabled) {
                    return;
                }

                this.currentChat = new ResolvablePromise<string | undefined | null>();
                this.chatDialog.show();

                return await this.currentChat.promise;
            },
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
            onChange: (value: EditorValue) => {
                this.callChange(value);
            },
            onEditAsset: id => {
                this.assetId.next(id);
            },
            onAnnotationCreate: event => {
                this.annotationsCreate.emit(event);
            },
            onAnnotationsUpdate: event => {
                this.annotationsUpdate.emit(event);
            },
            onAnnotationsFocus: event => {
                this.annotationsSelect.emit(event);
            },
            onEditContent: (schemaName, id) => {
                const url = this.apiUrl.buildUrl(`/app/${this.appsState.appName}/content/${schemaName}/${id}`);

                window.open(url, '_blank');
            },
            mode: this.mode,
            annotations: this.annotations,
            appName: this.appsState.appName,
            baseUrl: this.apiUrl.buildUrl(''),
            canAddAnnotation: this.hasAnnotations,
            canSelectAIText: this.hasChatBot,
            canSelectAssets: true,
            canSelectContents: !!this.schemaIds,
            classNames: this.classNames,
            isDisabled: this.snapshot.isDisabled,
            value: this.value || '',
        });
    }

    public reset() {
        this.ngOnDestroy();

        setTimeout(() => {
            this.ngAfterViewInit();
        });
    }

    public writeValue(obj: any) {
        if (this.editorWrapper) {
            this.editorWrapper?.setValue(obj);
        } else {
            this.value = obj;
        }
    }

    public onDisabled() {
        if (this.editorWrapper) {
            this.editorWrapper?.setIsDisabled(this.snapshot.isDisabled);
        }
    }

    public insertText(content: string | HTTP.UploadFile | undefined | null) {
        this.chatDialog.hide();

        if (!this.currentChat || !Types.isString(content)) {
            return;
        }

        this.currentChat.resolve(content);
        this.currentChat = undefined;
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

    private buildAsset(asset: AssetDto): Asset {
        return { ...asset, src: asset.fullUrl(this.apiUrl) };
    }

    private buildContent(content: ContentDto): Content {
        return { ...content, title: buildContentTitle(content, this.language) };
    }

    public closeAssetDialog() {
        this.assetId.next(null);
    }
}

function buildContentTitle(content: ContentDto, language: LanguageDto) {
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
