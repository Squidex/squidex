/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, Component, forwardRef, Input, ViewChild } from '@angular/core';
import { ControlValueAccessor, DefaultValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { CodeEditorComponent, Types } from '@app/framework';

export const SQX_FORMATTABLE_INPUT_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => FormattableInputComponent), multi: true,
};

type TemplateMode = 'Text' | 'Script' | 'Liquid';

const MODES: ReadonlyArray<TemplateMode> = ['Text', 'Script', 'Liquid'];

@Component({
    selector: 'sqx-formattable-input[type]',
    styleUrls: ['./formattable-input.component.scss'],
    templateUrl: './formattable-input.component.html',
    providers: [
        SQX_FORMATTABLE_INPUT_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FormattableInputComponent implements ControlValueAccessor, AfterViewInit {
    private fnChanged = (_: any) => { /* NOOP */ };
    private fnTouched = () => { /* NOOP */ };
    private value?: string;

    @Input()
    public type: 'Text' | 'Code';

    @Input()
    public formattable = true;

    @ViewChild(DefaultValueAccessor)
    public inputEditor: DefaultValueAccessor;

    @ViewChild(CodeEditorComponent)
    public codeEditor: CodeEditorComponent;

    public disabled = false;

    public get valueAccessor(): ControlValueAccessor {
        return this.codeEditor || this.inputEditor;
    }

    public modes = MODES;
    public mode: TemplateMode = 'Text';

    public aceMode = 'ace/editor/text';

    public ngAfterViewInit() {
        this.valueAccessor.registerOnChange((value: any) => {
            this.value = value;

            this.fnChanged(this.convertValue(value));
        });

        this.valueAccessor.registerOnTouched(() => {
            this.fnTouched();
        });

        this.valueAccessor.writeValue(this.value);
    }

    public writeValue(obj: any) {
        let mode: TemplateMode = 'Text';

        if (Types.isString(obj)) {
            this.value = obj;

            if (obj.endsWith(')')) {
                const lower = obj.toLowerCase();

                if (lower.startsWith('liquid(')) {
                    this.value = obj.substr(7, obj.length - 8);

                    mode = 'Liquid';
                } else if (lower.startsWith('script(')) {
                    this.value = obj.substr(7, obj.length - 8);

                    mode = 'Script';
                }
            }
        } else {
            this.value = undefined;
        }

        this.setMode(mode, false);

        this.valueAccessor?.writeValue(this.value);
    }

    public setDisabledState(isDisabled: boolean) {
        this.disabled = isDisabled;

        this.valueAccessor?.setDisabledState?.(isDisabled);
    }

    public setMode(mode: TemplateMode, emit = true) {
        if (this.mode !== mode) {
            this.mode = mode;

            if (mode === 'Script') {
                this.aceMode = 'ace/mode/javascript';
            } else if (mode === 'Liquid') {
                this.aceMode = 'ace/mode/liquid';
            } else {
                this.aceMode = 'ace/editor/text';
            }

            if (emit) {
                this.fnChanged(this.convertValue(this.value));
                this.fnTouched();
            }
        }
    }

    public registerOnChange(fn: any) {
        this.fnChanged = fn;
    }

    public registerOnTouched(fn: any) {
        this.fnTouched = fn;
    }

    private convertValue(value: string | undefined) {
        if (!value) {
            return value;
        }

        value = value.trim();

        switch (this.mode) {
            case 'Liquid': {
                return `Liquid(${value})`;
            }
            case 'Script': {
                return `Script(${value})`;
            }
        }

        return value;
    }
}
