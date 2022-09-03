/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, EventEmitter, forwardRef, Input, OnDestroy, Output, ViewChild } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { ContentDto } from '@app/shared';
import { ApiUrlConfig, AssetDto, AssetUploaderState, DialogModel, getContentValue, LanguageDto, ResourceLoaderService, StatefulControlComponent, Types, UploadCanceled } from '@app/shared/internal';

declare const tinymce: any;

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
    private tinyEditor: any;
    private value?: string;

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

    @Input()
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    @ViewChild('editor', { static: false })
    public editor!: ElementRef;

    public assetsDialog = new DialogModel();

    public contentsDialog = new DialogModel();

    constructor(changeDetector: ChangeDetectorRef,
        private readonly apiUrl: ApiUrlConfig,
        private readonly assetUploader: AssetUploaderState,
        private readonly resourceLoader: ResourceLoaderService,
    ) {
        super(changeDetector, {});
    }

    public ngOnDestroy() {
        if (this.tinyEditor) {
            this.tinyEditor.destroy();
            this.tinyEditor = null;
        }
    }

    public ngAfterViewInit() {
        this.resourceLoader.loadLocalScript('dependencies/tinymce/tinymce.min.js').then(() => {
            const timer = setInterval(() => {
                const target = this.editor.nativeElement;

                if (document.body.contains(target)) {
                    tinymce.init(this.getEditorOptions(target));

                    clearInterval(timer);
                }
            }, 10);
        });
    }

    public reset() {
        this.ngOnDestroy();

        setTimeout(() => {
            this.ngAfterViewInit();
        });
    }

    private showAssetsSelector = () => {
        if (this.snapshot.isDisabled) {
            return;
        }

        this.assetsDialog.show();
    };

    private showContentsSelector = () => {
        if (this.snapshot.isDisabled) {
            return;
        }

        this.contentsDialog.show();
    };

    private getEditorOptions(target: any): any {
        // eslint-disable-next-line @typescript-eslint/no-this-alias
        const self = this;

        return {
            ...DEFAULT_PROPS,

            images_upload_handler: (blob: any, success: (url: string) => void, failure: (message: string) => void) => {
                const file = new File([blob.blob()], blob.filename(), { lastModified: new Date().getTime() });

                self.assetUploader.uploadFile(file, undefined, this.folderId)
                    .subscribe({
                        next: asset => {
                            if (Types.is(asset, AssetDto)) {
                                success(asset.fullUrl(self.apiUrl));
                            }
                        },
                        error: error => {
                            if (!Types.is(error, UploadCanceled)) {
                                failure('Failed');
                            }
                        },
                    });
            },

            setup: (editor: any) => {
                editor.ui.registry.addButton('assets', {
                    onAction: self.showAssetsSelector,
                    icon: 'gallery',
                    text: '',
                    tooltip: 'Insert Assets',
                });

                if (this.schemaIds && this.schemaIds.length > 0) {
                    editor.ui.registry.addButton('contents', {
                        onAction: self.showContentsSelector,
                        icon: 'duplicate',
                        text: '',
                        tooltip: 'Insert Contents',
                    });
                }

                editor.on('init', () => {
                    self.tinyEditor = editor;

                    self.setContent();
                    self.setReadOnly();
                });

                editor.on('change', () => {
                    self.onValueChanged();
                });

                editor.on('paste', (event: ClipboardEvent) => {
                    let hasFileDropped = false;

                    if (event.clipboardData) {
                        for (let i = 0; i < event.clipboardData.items.length; i++) {
                            const file = event.clipboardData.items[i].getAsFile();

                            if (file) {
                                self.uploadFile(file);

                                hasFileDropped = true;
                            }
                        }
                    }

                    if (!hasFileDropped) {
                        self.onValueChanged();
                    } else {
                        return false;
                    }

                    return undefined;
                });

                editor.on('drop', (event: DragEvent) => {
                    let hasFileDropped = false;

                    if (event.dataTransfer) {
                        for (let i = 0; i < event.dataTransfer.files.length; i++) {
                            const file = event.dataTransfer.files.item(i);

                            if (file) {
                                self.uploadFile(file);

                                hasFileDropped = true;
                            }
                        }
                    }

                    if (!hasFileDropped) {
                        self.onValueChanged();
                    }

                    return false;
                });

                editor.on('blur', () => {
                    self.callTouched();
                });
            },

            target,
        };
    }

    private onValueChanged() {
        const value = this.tinyEditor.getContent();

        if (this.value !== value) {
            this.value = value;

            this.callChange(value);
        }
    }

    public writeValue(obj: any) {
        const newValue = Types.isString(obj) ? obj : '';

        if (newValue == this.value) {
            return;
        }

        this.value = newValue;

        if (this.tinyEditor && this.tinyEditor.initialized) {
            this.setContent();
        }
    }

    public onDisabled() {
        if (this.tinyEditor && this.tinyEditor.initialized) {
            this.setReadOnly();
        }
    }

    private setContent() {
        this.tinyEditor.setContent(this.value || '');
    }

    private setReadOnly() {
        this.tinyEditor.setMode(this.snapshot.isDisabled ? 'readonly' : 'design');
    }

    public insertAssets(assets: ReadonlyArray<AssetDto>) {
        const content = this.buildAssetsMarkup(assets);

        if (content.length > 0) {
            this.tinyEditor.execCommand('mceInsertContent', false, content);
        }

        this.assetsDialog.hide();
    }

    public insertContents(contents: ReadonlyArray<ContentDto>) {
        const content = this.buildContentsMarkup(contents);

        if (content.length > 0) {
            this.tinyEditor.execCommand('mceInsertContent', false, content);
        }

        this.contentsDialog.hide();
    }

    public insertFiles(files: ReadonlyArray<File>) {
        for (const file of files) {
            this.uploadFile(file);
        }
    }

    private uploadFile(file: File) {
        const uploadText = `[Uploading file...${new Date()}]`;

        this.tinyEditor.execCommand('mceInsertContent', false, uploadText);

        const replaceText = (replacement: string) => {
            const content = this.tinyEditor.getContent().replace(uploadText, replacement);

            this.tinyEditor.setContent(content);
        };

        this.assetUploader.uploadFile(file, undefined, this.folderId)
            .subscribe({
                next: asset => {
                    if (Types.is(asset, AssetDto)) {
                        replaceText(this.buildAssetMarkup(asset));
                    }
                },
                error: error => {
                    if (!Types.is(error, UploadCanceled)) {
                        replaceText('FAILED');
                    }
                },
            });
    }

    private buildAssetsMarkup(assets: ReadonlyArray<AssetDto>) {
        let markup = '';

        for (const asset of assets) {
            markup += this.buildAssetMarkup(asset);
        }

        return markup;
    }

    private buildContentsMarkup(contents: ReadonlyArray<ContentDto>) {
        let markup = '';

        for (const content of contents) {
            markup += this.buildContentMarkup(content);
        }

        return markup;
    }

    private buildContentMarkup(content: ContentDto) {
        const name =
            content.referenceFields
                .map(f => getContentValue(content, this.language, f, false))
                .map(v => v.formatted)
                .defined()
                .join(', ')
            || 'content';

        return `<a href="${this.apiUrl.buildUrl(content._links['self'].href)}" alt="${name}">${name}</a>`;
    }

    private buildAssetMarkup(asset: AssetDto) {
        const name = asset.fileNameWithoutExtension;

        if (asset.type === 'Image' || asset.mimeType === 'image/svg+xml' || asset.fileName.endsWith('.svg')) {
            return `<img src="${asset.fullUrl(this.apiUrl)}" alt="${name}" />`;
        } else if (asset.type === 'Video') {
            return `<video src="${asset.fullUrl(this.apiUrl)}" />`;
        } else {
            return `<a href="${asset.fullUrl(this.apiUrl)}" alt="${name}">${name}</a>`;
        }
    }
}

const DEFAULT_PROPS = {
    convert_fonts_to_spans: true,
    convert_urls: false,
    paste_data_images: true,
    plugins: 'code image media link lists advlist paste',
    min_height: 400,
    max_height: 800,
    removed_menuitems: 'newdocument',
    resize: true,
    toolbar: 'undo redo | styleselect | bold italic | alignleft aligncenter | bullist numlist outdent indent | link image media | assets contents',
};
