/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable:prefer-for-of

import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, EventEmitter, forwardRef, OnDestroy, Output, ViewChild } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';

import {
    ApiUrlConfig,
    AssetDto,
    AssetUploaderState,
    DialogModel,
    ResourceLoaderService,
    StatefulControlComponent,
    Types,
    UploadCanceled
} from '@app/shared/internal';

declare var tinymce: any;

export const SQX_RICH_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => RichEditorComponent), multi: true
};

const ImageTypes: ReadonlyArray<any> = [
    'image/jpeg',
    'image/png',
    'image/jpg',
    'image/gif'
];

@Component({
    selector: 'sqx-rich-editor',
    styleUrls: ['./rich-editor.component.scss'],
    templateUrl: './rich-editor.component.html',
    providers: [SQX_RICH_EDITOR_CONTROL_VALUE_ACCESSOR],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class RichEditorComponent extends StatefulControlComponent<undefined, string> implements AfterViewInit, OnDestroy {
    private tinyEditor: any;
    private value: string;
    private isDisabled = false;

    @Output()
    public assetPluginClick = new EventEmitter<any>();

    @ViewChild('editor', { static: false })
    public editor: ElementRef;

    public assetsDialog = new DialogModel();

    constructor(changeDetector: ChangeDetectorRef,
        private readonly apiUrl: ApiUrlConfig,
        private readonly assetUploader: AssetUploaderState,
        private readonly resourceLoader: ResourceLoaderService
    ) {
        super(changeDetector, undefined);
    }

    public ngOnDestroy() {
        if (this.tinyEditor) {
            this.tinyEditor.destroy();
            this.tinyEditor = null;
        }
    }

    public ngAfterViewInit() {
        const self = this;

        this.resourceLoader.loadScript('https://cdnjs.cloudflare.com/ajax/libs/tinymce/4.9.4/tinymce.min.js').then(() => {
            tinymce.init(self.getEditorOptions());
        });
    }

    public reset() {
        this.ngOnDestroy();

        setTimeout(() => {
            this.ngAfterViewInit();
        });
    }

    private showSelector = () => {
        if (this.isDisabled) {
            return;
        }

        this.assetsDialog.show();
    }

    private getEditorOptions() {
        const self = this;

        return {
            convert_fonts_to_spans: true,
            convert_urls: false,
            plugins: 'code image media link lists advlist paste',
            min_height: 300,
            max_height: 800,
            removed_menuitems: 'newdocument',
            resize: true,
            toolbar: 'undo redo | styleselect | bold italic | alignleft aligncenter | bullist numlist outdent indent | link image media | assets',

            images_upload_handler: (blob: any, success: (url: string) => void, failed: () => void) => {
                const file = new File([blob.blob()], blob.filename(), { lastModified: new Date().getTime() });

                self.assetUploader.uploadFile(file)
                    .subscribe(asset => {
                        if (Types.is(asset, AssetDto)) {
                            success(asset.fullUrl(self.apiUrl));
                        }
                    }, error => {
                        if (!Types.is(error, UploadCanceled)) {
                            failed();
                        }
                    });
            },

            setup: (editor: any) => {
                self.tinyEditor = editor;
                self.tinyEditor.addButton('assets', {
                    onclick: self.showSelector,
                    icon: 'assets',
                    text: '',
                    tooltip: 'Insert Assets'
                });

                self.tinyEditor.on('init', () => {
                    self.setContent();
                    self.setReadOnly();
                });

                self.tinyEditor.on('change', () => {
                    const value = editor.getContent();

                    if (this.value !== value) {
                        this.value = value;

                        self.callChange(value);
                    }
                });

                self.tinyEditor.on('paste', (event: ClipboardEvent) => {
                    if (event.clipboardData) {
                        for (let i = 0; i < event.clipboardData.items.length; i++) {
                            const file = event.clipboardData.items[i].getAsFile();

                            if (file && ImageTypes.indexOf(file.type) >= 0) {
                                self.uploadFile(file);
                            }
                        }
                    }
                });

                self.tinyEditor.on('drop', (event: DragEvent) => {
                    if (event.dataTransfer) {
                        for (let i = 0; i < event.dataTransfer.files.length; i++) {
                            const file = event.dataTransfer.files.item(i);

                            if (file && ImageTypes.indexOf(file.type) >= 0) {
                                self.uploadFile(file);
                            }
                        }
                    }
                });

                self.tinyEditor.on('blur', () => {
                    self.callTouched();
                });

                self.setReadOnly();
            },

            target: self.editor.nativeElement
        };
    }

    public writeValue(obj: any) {
        this.value = Types.isString(obj) ? obj : '';

        if (this.tinyEditor) {
            this.setContent();
        }
    }

    public setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;

        if (this.tinyEditor) {
            this.setReadOnly();
        }
    }

    private setContent() {
        this.tinyEditor.setContent(this.value || '');
    }

    private setReadOnly() {
        this.tinyEditor.setMode(this.isDisabled ? 'readonly' : 'design');
    }

    public insertAssets(assets: ReadonlyArray<AssetDto>) {
        let content = '';

        for (const asset of assets) {
            if (asset.type === 'Video') {
                content += `<video src="${asset.fullUrl(this.apiUrl)}" />`;
            } else {
                content += `<img src="${asset.fullUrl(this.apiUrl)}" alt="${asset.fileName}" />`;
            }
        }

        if (content.length > 0) {
            this.tinyEditor.execCommand('mceInsertContent', false, content);
        }

        this.assetsDialog.hide();
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
            const content =  this.tinyEditor.getContent().replace(uploadText, replacement);

            this.tinyEditor.setContent(content);
        };

        this.assetUploader.uploadFile(file)
            .subscribe(asset => {
                if (Types.is(asset, AssetDto)) {
                    if (asset.type === 'Video') {
                        replaceText(`<video src="${asset.fullUrl(this.apiUrl)}" />`);
                    } else {
                        replaceText(`<img src="${asset.fullUrl(this.apiUrl)}" alt="${asset.fileName}" />`);
                    }
                }
            }, error => {
                if (!Types.is(error, UploadCanceled)) {
                    replaceText('FAILED');
                }
            });
    }
}