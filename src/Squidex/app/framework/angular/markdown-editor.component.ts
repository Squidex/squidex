/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { AfterViewInit, Component, forwardRef, ElementRef, ViewChild } from '@angular/core';
import { ControlValueAccessor,  NG_VALUE_ACCESSOR } from '@angular/forms';

import { ResourceLoaderService } from './../services/resource-loader.service';

declare var SimpleMDE: any;

/* tslint:disable:no-empty */
const NOOP = () => { };

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
    private changeCallback: (value: any) => void = NOOP;
    private touchedCallback: () => void = NOOP;
    private simplemde: any;
    private value: any;
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

    public writeValue(value: any) {
        this.value = value;

        if (this.simplemde) {
            this.simplemde.value(this.value || '');
        }
    }

    public setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;

        if (this.simplemde) {
            this.simplemde.codemirror.setOption('readOnly', isDisabled);
        }
    }

    public registerOnChange(fn: any) {
        this.changeCallback = fn;
    }

    public registerOnTouched(fn: any) {
        this.touchedCallback = fn;
    }

    public ngAfterViewInit() {
        this.resourceLoader.loadScript('https://cdn.jsdelivr.net/simplemde/latest/simplemde.min.js').then(() => {
            this.simplemde = new SimpleMDE({ element: this.editor.nativeElement });
            this.simplemde.value(this.value || '');

            this.simplemde.codemirror.on('change', () => {
                const value = this.simplemde.value();

                this.changeCallback(value);
            });

            this.simplemde.codemirror.on('blur', () => {
                this.touchedCallback();
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