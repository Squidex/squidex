/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, forwardRef, Input, Renderer2, ViewChild } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { ApiUrlConfig, AssetDto, AssetUploaderState, DialogModel, ResourceLoaderService, StatefulControlComponent, Types, UploadCanceled } from '@app/shared/internal';
import marked from 'marked';

declare const SimpleMDE: any;

export const SQX_MARKDOWN_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => MarkdownEditorComponent), multi: true,
};

interface State {
    // True, when the editor is shown as fullscreen.
    isFullscreen: boolean;
}

@Component({
    selector: 'sqx-markdown-editor',
    styleUrls: ['./markdown-editor.component.scss'],
    templateUrl: './markdown-editor.component.html',
    providers: [
        SQX_MARKDOWN_EDITOR_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MarkdownEditorComponent extends StatefulControlComponent<State, string> implements AfterViewInit {
    private simplemde: any;
    private value: string;

    @Input()
    public folderId?: string;

    @Input()
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    @ViewChild('editor', { static: false })
    public editor: ElementRef;

    @ViewChild('container', { static: false })
    public container: ElementRef;

    @ViewChild('inner', { static: false })
    public inner: ElementRef;

    public assetsDialog = new DialogModel();

    constructor(changeDetector: ChangeDetectorRef,
        private readonly apiUrl: ApiUrlConfig,
        private readonly assetUploader: AssetUploaderState,
        private readonly renderer: Renderer2,
        private readonly resourceLoader: ResourceLoaderService,
    ) {
        super(changeDetector, {
            isFullscreen: false,
        });
    }

    public writeValue(obj: any) {
        this.value = Types.isString(obj) ? obj : '';

        if (this.simplemde) {
            this.simplemde.value(this.value);
        }
    }

    public onDisabled(isDisabled: boolean) {
        if (this.simplemde) {
            this.simplemde.codemirror.setOption('readOnly', isDisabled);
        }
    }

    private showSelector = () => {
        if (this.snapshot.isDisabled) {
            return;
        }

        this.assetsDialog.show();
    };

    public ngAfterViewInit() {
        Promise.all([
            this.resourceLoader.loadLocalStyle('dependencies/simplemde/simplemde.min.css'),
            this.resourceLoader.loadLocalStyle('dependencies/font-awesome/css/font-awesome.min.css'),
            this.resourceLoader.loadLocalScript('dependencies/simplemde/simplemde.min.js'),
        ]).then(() => {
            this.simplemde = new SimpleMDE({
                previewRender: (text: string) => {
                    return marked(text, { pedantic: true });
                },
                autoDownloadFontAwesome: true,
                spellChecker: false,
                status: ['lines', 'words', 'cursor'],
                toolbar: [
                    {
                        name: 'bold',
                        action: SimpleMDE.toggleBold,
                        className: 'fa fa-bold',
                        title: 'Bold',
                    }, {
                        name: 'italic',
                        action: SimpleMDE.toggleItalic,
                        className: 'fa fa-italic',
                        title: 'Italic',
                    }, {
                        name: 'heading',
                        action: SimpleMDE.toggleHeadingSmaller,
                        className: 'fa fa-header',
                        title: 'Heading',
                    }, {
                        name: 'quote',
                        action: SimpleMDE.toggleBlockquote,
                        className: 'fa fa-quote-left',
                        title: 'Quote',
                    }, {
                        name: 'unordered-list',
                        action: SimpleMDE.toggleUnorderedList,
                        className: 'fa fa-list-ul',
                        title: 'Generic List',
                    }, {
                        name: 'ordered-list',
                        action: SimpleMDE.toggleOrderedList,
                        className: 'fa fa-list-ol',
                        title: 'Numbered List',
                    },
                    '|',
                    {
                        name: 'link',
                        action: SimpleMDE.drawLink,
                        className: 'fa fa-link',
                        title: 'Create Link',
                    }, {
                        name: 'image',
                        action: SimpleMDE.drawImage,
                        className: 'fa fa-picture-o',
                        title: 'Insert Image',
                    },
                    '|',
                    {
                        name: 'preview',
                        action: SimpleMDE.togglePreview,
                        className: 'fa fa-eye no-disable',
                        title: 'Toggle Preview',
                    }, {
                        name: 'fullscreen',
                        action: SimpleMDE.toggleFullScreen,
                        className: 'fa fa-arrows-alt no-disable no-mobile',
                        title: 'Toggle Fullscreen',
                    }, {
                        name: 'side-by-side',
                        action: SimpleMDE.toggleSideBySide,
                        className: 'fa fa-columns no-disable no-mobile',
                        title: 'Toggle Side by Side',
                    },
                    '|',
                    {
                        name: 'guide',
                        action: 'https://simplemde.com/markdown-guide',
                        className: 'fa fa-question-circle',
                        title: 'Markdown Guide',
                    },
                    '|',
                    {
                        name: 'assets',
                        action: this.showSelector,
                        className: 'icon-assets icon-bold',
                        title: 'Insert Assets',
                    },
                ],
                element: this.editor.nativeElement,
            });

            this.simplemde.value(this.value || '');
            this.simplemde.codemirror.setOption('readOnly', this.snapshot.isDisabled);

            this.simplemde.codemirror.on('change', () => {
                const value = this.simplemde.value();

                if (this.value !== value) {
                    this.value = value;

                    this.callChange(value);
                }
            });

            this.simplemde.codemirror.on('refresh', () => {
                const isFullscreen = this.simplemde.isFullscreenActive();

                this.toggleFullscreen(isFullscreen);
            });

            this.simplemde.codemirror.on('blur', () => {
                this.callTouched();
            });
        });
    }

    public insertAssets(assets: ReadonlyArray<AssetDto>) {
        const content = this.buildMarkups(assets);

        if (content.length > 0) {
            this.simplemde.codemirror.replaceSelection(content);
        }

        this.assetsDialog.hide();
    }

    public insertFiles(files: ReadonlyArray<File>) {
        const doc = this.simplemde.codemirror.getDoc();

        for (const file of files) {
            this.uploadFile(doc, file);
        }
    }

    private uploadFile(doc: any, file: File) {
        if (this.snapshot.isDisabled) {
            return;
        }

        const uploadCursor = doc.getCursor();
        const uploadText = `![Uploading file...${new Date()}]()`;

        doc.replaceSelection(uploadText);

        const replaceText = (replacement: string) => {
            const cursor = doc.getCursor();

            const text = doc.getValue().replace(uploadText, replacement);

            doc.setValue(text);

            if (uploadCursor && uploadCursor.line === cursor.line) {
                const offset = replacement.length - uploadText.length;

                doc.setCursor({ line: cursor.line, ch: cursor.ch + offset });
            } else {
                doc.setCursor(cursor);
            }
        };

        this.assetUploader.uploadFile(file, undefined, this.folderId)
            .subscribe({
                next: asset => {
                    if (Types.is(asset, AssetDto)) {
                        replaceText(this.buildMarkup(asset));
                    }
                },
                error: error => {
                    if (!Types.is(error, UploadCanceled)) {
                        replaceText('FAILED');
                    }
                },
            });
    }

    private toggleFullscreen(isFullscreen: boolean) {
        let target = this.container.nativeElement;

        if (isFullscreen) {
            target = document.body;
        }

        this.renderer.appendChild(target, this.inner.nativeElement);

        this.next({ isFullscreen });
    }

    private buildMarkups(assets: readonly AssetDto[]) {
        let content = '';

        for (const asset of assets) {
            content += this.buildMarkup(asset);
        }

        return content;
    }

    private buildMarkup(asset: AssetDto) {
        const name = asset.fileNameWithoutExtension;

        if (asset.type === 'Image' || asset.mimeType === 'image/svg+xml' || asset.fileName.endsWith('.svg')) {
            return `![${name}](${asset.fullUrl(this.apiUrl)} '${name}')`;
        } else if (asset.type === 'Video') {
            return `[${name}](${asset.fullUrl(this.apiUrl)}')`;
        } else {
            return `[${name}](${asset.fullUrl(this.apiUrl)}')`;
        }
    }
}
