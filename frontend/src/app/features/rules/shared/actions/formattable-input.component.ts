/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { AfterViewInit, ChangeDetectionStrategy, Component, forwardRef, Input, ViewChild } from '@angular/core';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { CodeEditorComponent, ScriptCompletions, Types } from '@app/shared';

type TemplateMode = 'Text' | 'Script' | 'Liquid';

const TEMPLATE_MODES: ReadonlyArray<TemplateMode> = ['Text', 'Script', 'Liquid'];

export const SQX_FORMATTABLE_INPUT_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => FormattableInputComponent), multi: true,
};

@Component({
    standalone: true,
    selector: 'sqx-formattable-input',
    styleUrls: ['./formattable-input.component.scss'],
    templateUrl: './formattable-input.component.html',
    providers: [
        SQX_FORMATTABLE_INPUT_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CodeEditorComponent,
        FormsModule,
    ],
})
export class FormattableInputComponent implements ControlValueAccessor, AfterViewInit {
    private fnChanged = (_: any) => { /* NOOP */ };
    private fnTouched = () => { /* NOOP */ };
    private value?: string;

    @Input({ required: true })
    public type: 'Text' | 'Code' = 'Text';

    @Input()
    public completion: ScriptCompletions | undefined | null;

    @ViewChild(CodeEditorComponent)
    public codeEditor!: CodeEditorComponent;

    public disabled = false;

    public modes = TEMPLATE_MODES;
    public mode: TemplateMode = 'Text';

    public editorMode = 'ace/mode/text';
    public editorCompletion?: ScriptCompletions | undefined | null;

    public ngAfterViewInit() {
        this.codeEditor.registerOnChange((value: any) => {
            this.value = value;

            this.fnChanged(getValueFromMode(value, this.mode));
        });

        this.codeEditor.registerOnTouched(() => {
            this.fnTouched();
        });

        this.codeEditor.writeValue(this.value);
    }

    public writeValue(obj: any) {
        const { value, mode } = getModeFromValue(obj);

        this.value = value;

        this.setMode(mode, false);
        this.codeEditor?.writeValue(value);
    }

    public setDisabledState(isDisabled: boolean) {
        this.setDisabled(isDisabled);
        this.codeEditor?.setDisabledState?.(isDisabled);
    }

    private setDisabled(isDisabled: boolean) {
        this.disabled = isDisabled;
    }

    public setMode(mode: TemplateMode, emit = true) {
        if (this.mode === mode) {
            return;
        }

        if (mode === 'Script') {
            this.editorMode = 'ace/mode/javascript';
            this.editorCompletion = this.completion;
        } else if (mode === 'Liquid') {
            this.editorMode = 'ace/mode/liquid';
            this.editorCompletion = this.completion?.filter(x => x.type !== 'Function');
        } else {
            this.editorMode = 'ace/mode/text';
            this.editorCompletion = this.completion?.filter(x => x.type !== 'Function');
        }

        this.mode = mode;

        if (emit) {
            this.fnChanged(getValueFromMode(this.value, mode));
            this.fnTouched();
        }
    }

    public registerOnChange(fn: any) {
        this.fnChanged = fn;
    }

    public registerOnTouched(fn: any) {
        this.fnTouched = fn;
    }
}

function getValueFromMode(value: string | undefined, mode: TemplateMode) {
    if (!value) {
        return value;
    }

    value = value.trim();

    switch (mode) {
        case 'Liquid':
            value = `Liquid(${value})`;
            break;
        case 'Script':
            value = `Script(${value})`;
            break;
    }

    return value;
}

function getModeFromValue(value: any): { value: string | undefined; mode: TemplateMode } {
    if (!Types.isString(value) || !value) {
        return { value, mode: 'Text' };
    }

    if (value.endsWith(')')) {
        const lower = value.toLowerCase();

        if (lower.startsWith('liquid(')) {
            value = value.substring(7, value.length - 1);

            return { value, mode: 'Liquid' };
        } else if (lower.startsWith('script(')) {
            value = value.substring(7, value.length - 1);

            return { value, mode: 'Script' };
        }
    }

    return { value, mode: 'Text' };
}
