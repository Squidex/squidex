/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, forwardRef, ViewChild } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime } from 'rxjs/operators';

import { ResourceLoaderService, StatefulControlComponent } from '@app/framework/internal';

declare var ace: any;

export const SQX_JSON_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => JsonEditorComponent), multi: true
};

@Component({
    selector: 'sqx-json-editor',
    styleUrls: ['./json-editor.component.scss'],
    templateUrl: './json-editor.component.html',
    providers: [SQX_JSON_EDITOR_CONTROL_VALUE_ACCESSOR],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class JsonEditorComponent extends StatefulControlComponent<{}, string> implements AfterViewInit {
    private valueChanged = new Subject();
    private aceEditor: any;
    private value: any;
    private valueString: string;
    private isDisabled = false;

    @ViewChild('editor', { static: false })
    public editor: ElementRef<HTMLDivElement>;

    constructor(changeDetector: ChangeDetectorRef,
        private readonly resourceLoader: ResourceLoaderService
    ) {
        super(changeDetector, {});
    }

    public writeValue(obj: any) {
        this.value = obj;

        try {
            this.valueString = JSON.stringify(obj);
        } catch (e) {
            this.valueString = '';
        }

        if (this.aceEditor) {
            this.setValue(obj);
        }
    }

    public setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;

        if (this.aceEditor) {
            this.aceEditor.setReadOnly(isDisabled);
        }
    }

    public ngAfterViewInit() {
        this.valueChanged.pipe(
                debounceTime(500))
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

            this.detach();
        });
    }

    private changeValue() {
        const isValid = this.aceEditor.getSession().getAnnotations().length === 0;

        let newValue: any = null;

        if (isValid) {
            try {
                newValue = JSON.parse(this.aceEditor.getValue());
            } catch (e) {
                newValue = null;
            }
        }

        const newValueString = JSON.stringify(newValue);

        if (this.valueString !== newValueString) {
            this.callChange(newValue);
        }

        this.value = newValue;
        this.valueString = newValueString;
    }

    private setValue(value: any) {
        if (value) {
            const jsonString = JSON.stringify(value, undefined, 4);

            this.aceEditor.setValue(jsonString);
        } else {
            this.aceEditor.setValue('');
        }

        this.aceEditor.clearSelection();
    }
}