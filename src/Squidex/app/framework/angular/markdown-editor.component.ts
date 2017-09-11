/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { AfterViewInit, Component, forwardRef, ElementRef, ViewChild } from '@angular/core';
import { ControlValueAccessor,  NG_VALUE_ACCESSOR } from '@angular/forms';

import { Types } from './../utils/types';

import { ResourceLoaderService } from './../services/resource-loader.service';

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

    public ngAfterViewInit() {
        this.resourceLoader.loadScript('https://cdn.jsdelivr.net/simplemde/latest/simplemde.min.js').then(() => {
            this.simplemde = new SimpleMDE({ element: this.editor.nativeElement });
            this.simplemde.value(this.value || '');
            this.simplemde.codemirror.setOption('readOnly', this.isDisabled);

            this.simplemde.codemirror.on('change', () => {
                const value = this.simplemde.value();

                if (this.value !== value) {
                    this.value = value;

                    this.callChange(value);
                }
            });

            this.simplemde.codemirror.on('blur', () => {
                this.callTouched();
            });

            this.simplemde.codemirror.on('refresh', () => {
                this.isFullscreen = this.simplemde.isFullscreenActive();

                if (this.isFullscreen) {
                    document.body.appendChild(this.inner.nativeElement);
                } else {
                    this.container.nativeElement.appendChild(this.inner.nativeElement);
                }
            });
        });
    }
}