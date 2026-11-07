/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, computed, input, model, signal } from '@angular/core';
import { CodeEditorComponent, FormValueControlBase, ScriptCompletions, syncExternalValue, Types } from '@app/shared';

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
export class FormattableInputComponent extends FormValueControlBase<string | null> {
    // Ignore the values we have emitted ourselves, because the text is trimmed when it is emitted.
    private readonly valueSetter = syncExternalValue(() => this.value, value => this.applyValue(value));

    /** The value, optionally wrapped with the template mode, e.g. "Liquid(...)". */
    public readonly value = model<string | null>(null);

    /** Renders a single line input for text or a large editor for code. */
    public readonly type = input.required<'Text' | 'Code'>();

    /** The suggestions for autocompletion. */
    public readonly completion = input<ScriptCompletions | undefined | null>();

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

    protected readonly modes: TemplateMode[] = ['Text', 'Script', 'Liquid'];

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
        this.markTouched();
    }

    private applyValue(value: string | null) {
        const parsed = getModeFromValue(value);

        this.text.set(parsed.value ?? null);
        this.mode.set(parsed.mode);
    }

    private emitValue() {
        this.valueSetter(getValueFromMode(this.text(), this.mode()) ?? null);
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
