/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { AfterViewInit, Component, forwardRef, ElementRef, ViewChild } from '@angular/core';
import { ControlValueAccessor,  NG_VALUE_ACCESSOR } from '@angular/forms';
import { Subject } from 'rxjs';

import { Types } from './../utils/types';

import { ResourceLoaderService } from './../services/resource-loader.service';

declare var ace: any;

export const SQX_JSCRIPT_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => JscriptEditorComponent), multi: true
};

@Component({
    selector: 'sqx-jscript-editor',
    styleUrls: ['./jscript-editor.component.scss'],
    templateUrl: './jscript-editor.component.html',
    providers: [SQX_JSCRIPT_EDITOR_CONTROL_VALUE_ACCESSOR]
})
export class JscriptEditorComponent implements ControlValueAccessor, AfterViewInit {
    private callChange = (v: any) => { /* NOOP */ };
    private callTouched = () => { /* NOOP */ };
    private valueChanged = new Subject();
    private aceEditor: any;
    private value: string;
    private isDisabled = false;

    @ViewChild('editor')
    public editor: ElementRef;

    constructor(
        private readonly resourceLoader: ResourceLoaderService
    ) {
    }

    public writeValue(value: string) {
        this.value = Types.isString(value) ? value : '';

        if (this.aceEditor) {
            this.setValue(this.value);
        }
    }

    public setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;

        if (this.aceEditor) {
            this.aceEditor.setReadOnly(isDisabled);
        }
    }

    public registerOnChange(fn: any) {
        this.callChange = fn;
    }

    public registerOnTouched(fn: any) {
        this.callTouched = fn;
    }

    public ngAfterViewInit() {
        this.valueChanged.debounceTime(500)
            .subscribe(() => {
                this.changeValue();
            });

        this.resourceLoader.loadScript('https://cdnjs.cloudflare.com/ajax/libs/ace/1.2.6/ace.js').then(() => {
            this.aceEditor = ace.edit(this.editor.nativeElement);

            this.aceEditor.getSession().setMode('ace/mode/javascript');
            this.aceEditor.setReadOnly(this.isDisabled);
            this.aceEditor.setFontSize(14);

            this.setValue(this.value);

            this.aceEditor.on('blur', () => {
                this.changeValue();
                this.callTouched();
            });

            this.aceEditor.on('change', () => {
                this.valueChanged.next();
            });
        });
    }

    private changeValue() {
        const newValue = this.aceEditor.getValue();

        if (this.value !== newValue) {
            this.callChange(newValue);
        }

        this.value = newValue;
    }

    private setValue(value: string) {
        this.aceEditor.setValue(value || '');
        this.aceEditor.clearSelection();
    }
}