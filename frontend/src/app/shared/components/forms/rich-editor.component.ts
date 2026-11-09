/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { afterNextRender, booleanAttribute, Component, DestroyRef, effect, ElementRef, inject, input, model, output, signal, viewChild } from '@angular/core';
import { FormValueControlBase, HTTP, ModalDirective, safeResource, syncExternalValue, ThemeService } from '@app/framework';
import { ApiUrlConfig, AppLanguageDto, AppsState, AssetDto, AssetsService, AssetUploaderState, ContentDto, DialogModel, getContentValue, ResourceLoaderService, Types } from '@app/shared/internal';
import { AssetDialogComponent } from '../assets/asset-dialog.component';
import { AssetSelectorComponent } from '../assets/asset-selector.component';
import { ChatDialogComponent } from '../chat-dialog.component';
import { ContentSelectorComponent } from '../references/content-selector.component';

@Component({
    selector: 'sqx-rich-editor',
    styleUrls: ['./rich-editor.component.scss'],
    templateUrl: './rich-editor.component.html',
    imports: [
        AssetDialogComponent,
        AssetSelectorComponent,
        ChatDialogComponent,
        ContentSelectorComponent,
        ModalDirective,
    ],
})
export class RichEditorComponent extends FormValueControlBase<EditorValue> {
    private readonly apiUrl = inject(ApiUrlConfig);
    private readonly appsState = inject(AppsState);
    private readonly assetService = inject(AssetsService);
    private readonly assetUploader = inject(AssetUploaderState);
    private readonly destroyRef = inject(DestroyRef);
    private readonly resourceLoader = inject(ResourceLoaderService);
    private readonly themeService = inject(ThemeService);
    private readonly valueSetter = syncExternalValue(() => this.value, value => this.editorWrapper?.setValue(value));
    private currentAssets?: ResolvablePromise<any>;
    private currentChat?: ResolvablePromise<string | undefined | null>;
    private currentContents?: ResolvablePromise<any>;
    private editorWrapper?: SquidexEditorWrapper;
    private isDestroyed = false;

    /** The value of the editor. */
    public readonly value = model<EditorValue>(null);

    /** Emits when the asset plugin is clicked. */
    public readonly assetPluginClick = output<any>();

    /** Emits when the user creates an annotation for the selected text. */
    public readonly annotationsCreate = output<AnnotationSelection>();

    /** Emits when the annotations have been moved or removed by editing the text. */
    public readonly annotationsUpdate = output<ReadonlyArray<Annotation>>();

    /** Emits the IDs of the annotations at the cursor position. */
    public readonly annotationsSelect = output<ReadonlyArray<string>>();

    /** Allows the user to generate text with the chat bot. */
    public readonly hasChatBot = input(false, { transform: booleanAttribute });

    /** Allows the user to create annotations. */
    public readonly hasAnnotations = input(false, { transform: booleanAttribute });

    /** The annotations to highlight in the text. */
    public readonly annotations = input<ReadonlyArray<Annotation> | null>();

    /** The IDs of the schemas to select contents from. Contents cannot be selected when not defined. */
    public readonly schemaIds = input<ReadonlyArray<string>>();

    /** The language to build the titles of selected contents. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app, for the content selector. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** The ID of the folder to upload assets to. */
    public readonly folderId = input('');

    /** The CSS classes the user can apply to elements. */
    public readonly classNames = input.required<ReadonlyArray<string> | undefined>();

    /** The format of the value. */
    public readonly mode = input.required<SquidexEditorMode>();

    protected readonly editor = viewChild.required<ElementRef>('editor');

    protected readonly assetId = signal<string | null>(null);

    protected readonly assetToEdit = safeResource({
        params: () => this.assetId() ?? undefined,
        stream: ({ params }) => this.assetService.getAsset(this.appsState.appName, params),
        defaultValue: null,
    });

    protected readonly chatDialog = new DialogModel();

    protected readonly assetsDialog = new DialogModel();

    protected readonly contentsDialog = new DialogModel();

    constructor() {
        super();

        const destroyRef = this.destroyRef;

        effect(() => {
            const annotations = this.annotations();

            this.editorWrapper?.setAnnotations(annotations);
        });

        effect(() => {
            const isDisabled = this.disabled();

            this.editorWrapper?.setIsDisabled(isDisabled);
        });

        effect(() => {
            this.themeService.theme();
            this.editorWrapper?.update(this.editorColors());
        });

        afterNextRender(() => {
            this.createEditor();
        });

        destroyRef.onDestroy(() => {
            this.isDestroyed = true;
            this.destroyEditor();
        });
    }

    private async createEditor() {
        await Promise.all([
            this.resourceLoader.loadLocalScript('editor/squidex-editor.js'),
        ]);

        if (this.isDestroyed) {
            return;
        }

        this.editorWrapper = new SquidexEditorWrapper(this.editor().nativeElement, {
            onSelectAIText: async () => {
                if (this.disabled()) {
                    return;
                }

                this.currentChat = new ResolvablePromise<string | undefined | null>();
                this.chatDialog.show();

                return await this.currentChat.promise;
            },
            onSelectAssets: async () => {
                if (this.disabled()) {
                    return;
                }

                this.currentAssets = new ResolvablePromise<any>();
                this.assetsDialog.show();

                return await this.currentAssets.promise;
            },
            onSelectContents: async () => {
                if (this.disabled()) {
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
                this.valueSetter(value ?? null);
            },
            onEditAsset: id => {
                this.assetId.set(id);
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
            mode: this.mode(),
            annotations: this.annotations(),
            appName: this.appsState.appName,
            baseUrl: this.apiUrl.buildUrl(''),
            canAddAnnotation: this.hasAnnotations(),
            canSelectAIText: this.hasChatBot(),
            canSelectAssets: true,
            canSelectContents: !!this.schemaIds(),
            classNames: this.classNames(),
            ...this.editorColors(),
            isDisabled: this.disabled(),
            value: this.value() || '',
        });
    }

    private destroyEditor() {
        this.editorWrapper?.destroy?.();
        this.editorWrapper = undefined;
    }

    private editorColors() {
        return {
            accentColor: this.themeService.getColor('editor-accent'),
            baseColor: this.themeService.getColor('editor-base'),
            borderColor: this.themeService.getColor('editor-border'),
        };
    }

    public reset() {
        this.destroyEditor();

        setTimeout(() => {
            this.createEditor();
        });
    }

    protected insertText(content: string | HTTP.UploadFile | undefined | null) {
        this.chatDialog.hide();

        if (!this.currentChat || !Types.isString(content)) {
            return;
        }

        this.currentChat.resolve(content);
        this.currentChat = undefined;
    }

    protected insertAssets(assets: ReadonlyArray<AssetDto>) {
        this.assetsDialog.hide();

        if (!this.currentAssets) {
            return;
        }

        const items = assets.map(a => this.buildAsset(a));

        this.currentAssets.resolve(items);
        this.currentAssets = undefined;
    }

    protected insertContents(contents: ReadonlyArray<ContentDto>) {
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
                this.assetUploader.uploadFile(request.file, this.folderId())
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
        const language = this.language();

        const title =
            content.referenceFields
                .map(f => getContentValue(content, language, f, false))
                .map(v => v.formatted)
                .defined()
                .join(', ');

        return { ...content, title: title || 'Content' };
    }

    protected closeAssetDialog() {
        this.assetId.set(null);
    }
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
