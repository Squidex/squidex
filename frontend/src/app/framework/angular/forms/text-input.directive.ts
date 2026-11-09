/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed, Directive, input, model } from '@angular/core';
import { generateSlug, Types } from '@app/framework/internal';
import { FormValueControlBase } from './form-value-control';

type Transform = (value: string) => string;

export const TransformNoop: Transform = value => value;
export const TransformLowerCase: Transform = value => value.toLowerCase();
export const TransformSlugify: Transform = value => generateSlug(value, { preserveTrailingDash: true });
export const TransformUpperCase: Transform = value => value.toUpperCase();

@Directive({
    selector: 'input[sqxTextInput]',
    host: {
        '[disabled]': 'disabled()',
        '[value]': 'coercedValue()',
        '(blur)': 'markTouched()',
        '(input)': 'onInput($event)',
    },
})
export class TextInputDirective extends FormValueControlBase<string | null> {
    /** The text, where null is rendered as empty input. */
    public readonly value = model<string | null>(null);

    /** The transformation that is applied to the entered text, either a function or LowerCase, Slugify or UpperCase. */
    public readonly transform = input<Transform | string | undefined | null>(undefined, { alias: 'sqxTextInput' });

    protected readonly coercedValue = computed(() => {
        // Native text inputs parse the text as number when the value is null, therefore nullable strings need a custom control.
        return this.value() ?? '';
    });

    protected readonly transformer = computed(() => {
        const value = this.transform();
        if (!Types.isString(value)) {
            return value || TransformNoop;
        }

        switch (value) {
            case 'LowerCase':
                return TransformLowerCase;
            case 'Slugify':
                return TransformSlugify;
            case 'UpperCase':
                return TransformUpperCase;
            default:
                return TransformNoop;
        }
    });

    protected onInput(event: Event) {
        const target = event.target as HTMLInputElement;

        const transformed = this.transformer()(target.value);

        // The host binding does not update the element if the value has not changed, e.g. when an invalid character is removed again.
        target.value = transformed;
        this.value.set(transformed);
    }
}
