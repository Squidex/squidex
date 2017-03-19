/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { AfterViewInit, Component, forwardRef, ElementRef, OnDestroy, ViewChild } from '@angular/core';
import { ControlValueAccessor,  NG_VALUE_ACCESSOR } from '@angular/forms';

import { ResourceLoaderService } from './../services/resource-loader.service';

declare var tinymce: any;

const NOOP = () => { /* NOOP */ };

export const SQX_RICH_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => RichEditorComponent), multi: true
};

@Component({
    selector: 'sqx-rich-editor',
    styleUrls: ['./rich-editor.component.scss'],
    templateUrl: './rich-editor.component.html',
    providers: [SQX_RICH_EDITOR_CONTROL_VALUE_ACCESSOR]
})
export class RichEditorComponent implements ControlValueAccessor, AfterViewInit, OnDestroy {
    private changeCallback: (value: any) => void = NOOP;
    private touchedCallback: () => void = NOOP;
    private tinyEditor: any;
    private value: any;
    private isDisabled = false;

    @ViewChild('editor')
    public editor: ElementRef;

    constructor(
        private readonly resourceLoader: ResourceLoaderService
    ) {
    }

    public writeValue(value: any) {
        this.value = value;

        if (this.tinyEditor) {
            this.tinyEditor.setContent(value || '');
        }
    }

    public setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;

        if (this.tinyEditor) {
            this.tinyEditor.setMode(isDisabled ? 'readonly' : 'design');
        }
    }

    public registerOnChange(fn: any) {
        this.changeCallback = fn;
    }

    public registerOnTouched(fn: any) {
        this.touchedCallback = fn;
    }

    public ngAfterViewInit() {
        const self = this;

        this.resourceLoader.loadScript('https://cdnjs.cloudflare.com/ajax/libs/tinymce/4.5.4/tinymce.min.js').then(() => {
            tinymce.init({
                setup: (editor: any) => {
                    self.tinyEditor = editor;
                    self.tinyEditor.setMode(this.isDisabled ? 'readonly' : 'design');

                    self.tinyEditor.on('change', () => {
                        const value = editor.getContent();

                        self.changeCallback(value);
                    });

                    self.tinyEditor.on('blur', () => {
                        self.touchedCallback();
                    });

                    setTimeout(() => {
                        self.tinyEditor.setContent(this.value || '');
                    }, 500);
                },
                target: this.editor.nativeElement
            });
        });
    }

    public ngOnDestroy() {
        tinymce.remove(this.editor);
    }
}