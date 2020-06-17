/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, forwardRef, Input, ViewChild } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { ResourceLoaderService, StatefulControlComponent } from '@app/framework/internal';
import { Subject } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { FocusComponent } from './../forms-helper';

declare var ace: any;

export const SQX_JSON_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => JsonEditorComponent), multi: true
};

@Component({
    selector: 'sqx-json-editor',
    styleUrls: ['./json-editor.component.scss'],
    templateUrl: './json-editor.component.html',
    providers: [
        SQX_JSON_EDITOR_CONTROL_VALUE_ACCESSOR
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class JsonEditorComponent extends StatefulControlComponent<{}, string> implements AfterViewInit, FocusComponent {
    private valueChanged = new Subject();
    private aceEditor: any;
    private value: any;
    private valueString: string;

    @ViewChild('editor', { static: false })
    public editor: ElementRef<HTMLDivElement>;

    @Input()
    public noBorder = false;

    @Input()
    public height = 0;

    constructor(changeDetector: ChangeDetectorRef,
        private readonly resourceLoader: ResourceLoaderService
    ) {
        super(changeDetector, {});
    }

    public ngAfterViewInit() {
        this.valueChanged.pipe(
                debounceTime(500))
            .subscribe(() => {
                this.changeValue();
            });

        if (this.height) {
            this.editor.nativeElement.style.height = `${this.height}px`;
        }

        this.resourceLoader.loadLocalScript('dependencies/ace/ace.js').then(() => {
            this.aceEditor = ace.edit(this.editor.nativeElement);

            this.aceEditor.getSession().setMode('ace/mode/javascript');
            this.aceEditor.setReadOnly(this.snapshot.isDisabled);
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
        super.setDisabledState(isDisabled);

        if (this.aceEditor) {
            this.aceEditor.setReadOnly(isDisabled);
        }
    }

    public focus() {
        if (this.aceEditor) {
            this.aceEditor.focus();
        }
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