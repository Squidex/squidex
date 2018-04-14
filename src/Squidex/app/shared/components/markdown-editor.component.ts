/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Component, ElementRef, forwardRef, ViewChild } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

import {
    AssetDto,
    ModalView,
    ResourceLoaderService,
    Types
} from '@app/shared/internal';

declare var SimpleMDE: any;

export const SQX_MARKDOWN_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => MarkdownEditorComponent), multi: true
};

@Component({
    selector: 'sqx-markdown-editor',
    styleUrls: ['./markdown-editor.component.scss'],
    templateUrl: './markdown-editor.component.html',
    providers: [SQX_MARKDOWN_EDITOR_CONTROL_VALUE_ACCESSOR]
})
export class MarkdownEditorComponent implements ControlValueAccessor, AfterViewInit {
    private callChange = (v: any) => { /* NOOP */ };
    private callTouched = () => { /* NOOP */ };
    private simplemde: any;
    private value: string;
    private isDisabled = false;

    public selectorModal = new ModalView();

    @ViewChild('editor')
    public editor: ElementRef;

    @ViewChild('container')
    public container: ElementRef;

    @ViewChild('inner')
    public inner: ElementRef;

    public isFullscreen = false;

    constructor(
        private readonly resourceLoader: ResourceLoaderService
    ) {
        this.resourceLoader.loadStyle('https://cdn.jsdelivr.net/simplemde/latest/simplemde.min.css');
    }

    public writeValue(value: string) {
        this.value = Types.isString(value) ? value : '';

        if (this.simplemde) {
            this.simplemde.value(this.value);
        }
    }

    public setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;

        if (this.simplemde) {
            this.simplemde.codemirror.setOption('readOnly', isDisabled);
        }
    }

    public registerOnChange(fn: any) {
        this.callChange = fn;
    }

    public registerOnTouched(fn: any) {
        this.callTouched = fn;
    }

    private showSelector = () => {
        this.selectorModal.show();
    }

    public ngAfterViewInit() {
        this.resourceLoader.loadScript('https://cdn.jsdelivr.net/simplemde/latest/simplemde.min.js').then(() => {
            this.simplemde = new SimpleMDE({
                toolbar: [
                    {
                        name: 'bold',
                        action: SimpleMDE.toggleBold,
                        className: 'fa fa-bold',
                        title: 'Bold'
                    }, {
                        name: 'italic',
                        action: SimpleMDE.toggleItalic,
                        className: 'fa fa-italic',
                        title: 'Italic'
                    }, {
                        name: 'heading',
                        action: SimpleMDE.toggleHeadingSmaller,
                        className: 'fa fa-header',
                        title: 'Heading'
                    }, {
                        name: 'quote',
                        action: SimpleMDE.toggleBlockquote,
                        className: 'fa fa-quote-left',
                        title: 'Quote'
                    }, {
                        name: 'unordered-list',
                        action: SimpleMDE.toggleUnorderedList,
                        className: 'fa fa-list-ul',
                        title: 'Generic List'
                    }, {
                        name: 'ordered-list',
                        action: SimpleMDE.toggleOrderedList,
                        className: 'fa fa-list-ol',
                        title: 'Numbered List'
                    },
                    '|',
                    {
                        name: 'link',
                        action: SimpleMDE.drawLink,
                        className: 'fa fa-link',
                        title: 'Create Link'
                    }, {
                        name: 'image',
                        action: SimpleMDE.drawImage,
                        className: 'fa fa-picture-o',
                        title: 'Insert Image'
                    },
                    '|',
                    {
                        name: 'preview',
                        action: SimpleMDE.togglePreview,
                        className: 'fa fa-eye no-disable',
                        title: 'Toggle Preview'
                    }, {
                        name: 'fullscreen',
                        action: SimpleMDE.toggleFullScreen,
                        className: 'fa fa-arrows-alt no-disable no-mobile',
                        title: 'Toggle Fullscreen'
                    }, {
                        name: 'side-by-side',
                        action: SimpleMDE.toggleSideBySide,
                        className: 'fa fa-columns no-disable no-mobile',
                        title: 'Toggle Side by Side'
                    },
                    '|',
                    {
                        name: 'guide',
                        action: 'https://simplemde.com/markdown-guide',
                        className: 'fa fa-question-circle',
                        title: 'Markdown Guide'
                    },
                    '|',
                    {
                        name: 'assets',
                        action: this.showSelector,
                        className: 'icon-assets icon-bold',
                        title: 'Insert Assets'
                    }
                ],
                element: this.editor.nativeElement
            });

            this.simplemde.value(this.value || '');
            this.simplemde.codemirror.setOption('readOnly', this.isDisabled);

            this.simplemde.codemirror.on('change', () => {
                const value = this.simplemde.value();

                if (this.value !== value) {
                    this.value = value;

                    this.callChange(value);
                }
            });

            this.simplemde.codemirror.on('refresh', () => {
                this.isFullscreen = this.simplemde.isFullscreenActive();

                if (this.isFullscreen) {
                    document.body.appendChild(this.inner.nativeElement);
                } else {
                    this.container.nativeElement.appendChild(this.inner.nativeElement);
                }
            });

            this.simplemde.codemirror.on('blur', () => {
                this.callTouched();
            });
        });
    }

    public onAssetsSelected(assets: AssetDto[]) {
        let content = '';

        for (let asset of assets) {
            content += `![${asset.fileName}](${asset.url} '${asset.fileName}')`;
        }

        if (content.length > 0) {
            this.simplemde.codemirror.replaceSelection(content);
        }

        this.selectorModal.hide();
    }
}