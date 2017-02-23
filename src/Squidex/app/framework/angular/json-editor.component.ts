/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { AfterViewInit, Component, forwardRef, ElementRef, ViewChild } from '@angular/core';
import { ControlValueAccessor,  NG_VALUE_ACCESSOR } from '@angular/forms';
import { ReplaySubject } from 'rxjs';

declare var ace: any;

/* tslint:disable:no-empty */
const NOOP = () => { };

export const SQX_JSON_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR,
    useExisting: forwardRef(() => JsonEditorComponent),
    multi: true
};

@Component({
    selector: 'sqx-json-editor',
    styleUrls: ['./json-editor.component.scss'],
    templateUrl: './json-editor.component.html',
    providers: [SQX_JSON_EDITOR_CONTROL_VALUE_ACCESSOR]
})
export class JsonEditorComponent implements ControlValueAccessor, AfterViewInit {
    private static loaderCallback: ReplaySubject<any>;
    private static isLoaded: boolean;

    private changeCallback: (value: any) => void = NOOP;
    private touchedCallback: () => void = NOOP;
    private aceEditor: any;
    private value: any;
    private isDisabled = false;

    @ViewChild('editor')
    public editor: ElementRef;

    public writeValue(value: any) {
        this.value = value;

        if (this.aceEditor) {
            this.setValue(value);
        }
    }

    public setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;

        if (this.aceEditor) {
            this.aceEditor.setReadOnly(isDisabled);
        }
    }

    public registerOnChange(fn: any) {
        this.changeCallback = fn;
    }

    public registerOnTouched(fn: any) {
        this.touchedCallback = fn;
    }

    public ngAfterViewInit() {
        JsonEditorComponent.loadScript(() => {
            this.aceEditor = ace.edit(this.editor.nativeElement);

            this.aceEditor.getSession().setMode('ace/mode/javascript');
            this.aceEditor.setReadOnly(this.isDisabled);
            this.aceEditor.setFontSize(14);

            this.setValue(this.value);

            this.aceEditor.on('blur', () => {
                const isValid = this.aceEditor.getSession().getAnnotations().length === 0;

                if (!isValid) {
                    this.changeCallback(null);
                } else {
                    try {
                        const value = JSON.parse(this.aceEditor.getValue());

                        this.changeCallback(value);
                    } catch (e) {
                        this.changeCallback(null);
                    }
                }

                this.touchedCallback();
            });
        });
    }

    private setValue(value: any) {
        if (!value) {
            value = {};
        }

        const jsonString = JSON.stringify(value, undefined, 4);

        this.aceEditor.setValue(jsonString);
    }

    private static loadScript(callback: () => void) {
        if (JsonEditorComponent.isLoaded) {
            callback();

            return;
        }

        if (JsonEditorComponent.loaderCallback) {
            JsonEditorComponent.loaderCallback.subscribe(callback);

            return;
        }

        JsonEditorComponent.loaderCallback = new ReplaySubject(1);
        JsonEditorComponent.loaderCallback.subscribe(callback);

        const url = 'https://cdnjs.cloudflare.com/ajax/libs/ace/1.2.6/ace.js';

        const script = document.createElement('script');
        script.src = url;
        script.async = true;
        script.onload = () => {
            JsonEditorComponent.loaderCallback.next(null);
            JsonEditorComponent.loaderCallback = null;
            JsonEditorComponent.isLoaded = true;
        };

        const node = document.getElementsByTagName('script')[0];

        node.parentNode.insertBefore(script, node);
    }
}