/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, computed, effect, input, model, signal } from '@angular/core';
import { FormValueControl } from '@angular/forms/signals';
import { CodeEditorComponent, ScriptCompletions, Types } from '@app/shared';

type TemplateMode = 'Text' | 'Script' | 'Liquid';

@Component({
    selector: 'sqx-formattable-input',
    styleUrls: ['./formattable-input.component.scss'],
    templateUrl: './formattable-input.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CodeEditorComponent,
    ],
})
export class FormattableInputComponent implements FormValueControl<string | null> {
    private currentValue: string | null = null;

    /** The value, optionally wrapped with the template mode, e.g. "Liquid(...)". */
    public readonly value = model<string | null>(null);

    /** Disables the control. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    /** Indicates whether the control has been touched. */
    public readonly touched = model(false);

    /** Renders a single line input for text or a large editor for code. */
    public readonly type = input.required<'Text' | 'Code'>();

    /** The suggestions for autocompletion. */
    public readonly completion = input<ScriptCompletions | undefined | null>();

    protected readonly modes: TemplateMode[] = ['Text', 'Script', 'Liquid'];
    protected readonly mode = signal<TemplateMode>('Text');
    protected readonly text = signal<string | null>(null);

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
        effect(() => {
            const value = this.value();

            // Ignore the values we have emitted ourselves, because the text is trimmed when it is emitted.
            if (value === this.currentValue) {
                return;
            }

            const parsed = getModeFromValue(value);

            this.currentValue = value;
            this.text.set(parsed.value ?? null);
            this.mode.set(parsed.mode);
        });
    }

    protected setText(text: string | null) {
        this.text.set(text);
        this.emitValue();
    }

    protected setMode(mode: TemplateMode) {
        if (this.mode() === mode) {
            return;
        }

        this.mode.set(mode);
        this.emitValue();
        this.touched.set(true);
    }

    private emitValue() {
        this.currentValue = getValueFromMode(this.text(), this.mode()) ?? null;
        this.value.set(this.currentValue);
    }
}

function getValueFromMode(value: string | null | undefined, mode: TemplateMode) {
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
