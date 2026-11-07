/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, computed, effect, forwardRef, input, signal, viewChild } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { CodeEditorComponent, ScriptCompletions, SignalControlComponent, Types } from '@app/shared';

type TemplateMode = 'Text' | 'Script' | 'Liquid';

export const SQX_FORMATTABLE_INPUT_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => FormattableInputComponent), multi: true,
};

@Component({
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
export class FormattableInputComponent extends SignalControlComponent<string> {
    private readonly codeEditor = viewChild(CodeEditorComponent);
    private value?: string;

    /** Renders a single line input for text or a large editor for code. */
    public readonly type = input.required<'Text' | 'Code'>();

    /** The suggestions for autocompletion. */
    public readonly completion = input<ScriptCompletions | undefined | null>();

    protected readonly modes: TemplateMode[] = ['Text', 'Script', 'Liquid'];

    protected readonly mode = signal<TemplateMode>('Text');

    protected readonly editorMode = computed(() => {
        switch (this.mode()) {
            case 'Script':
                return 'ace/mode/javascript';
            case 'Liquid':
                return 'ace/mode/liquid';
            default:
                return 'ace/mode/text';
        }
    });

    protected readonly editorCompletion = computed(() => {
        const completion = this.completion();
        if (this.mode() === 'Script') {
            return completion;
        }

        return completion?.filter(x => x.type !== 'Function');
    });

    constructor() {
        super();

        effect(() => {
            const codeEditor = this.codeEditor();
            if (!codeEditor) {
                return;
            }

            codeEditor.registerOnChange((value: any) => {
                this.value = value;

                this.callChange(getValueFromMode(value, this.mode()));
            });

            codeEditor.registerOnTouched(() => {
                this.callTouched();
            });

            codeEditor.writeValue(this.value);
        });
    }

    public writeValue(obj: any) {
        const { value, mode } = getModeFromValue(obj);

        this.value = value;

        this.setMode(mode, false);
        this.codeEditor()?.writeValue(value);
    }

    protected setMode(mode: TemplateMode, emit = true) {
        if (this.mode() === mode) {
            return;
        }

        this.mode.set(mode);

        if (emit) {
            this.callChange(getValueFromMode(this.value, mode));
            this.callTouched();
        }
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
