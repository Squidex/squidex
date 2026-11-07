/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed, Directive, inject, input } from '@angular/core';
import { FieldTree, FormField } from '@angular/forms/signals';
import { generateSlug, Types } from '@app/framework/internal';

type Transform = (value: string) => string;

export const TransformNoop: Transform = value => value;
export const TransformLowerCase: Transform = value => value.toLowerCase();
export const TransformSlugify: Transform = value => generateSlug(value, { preserveTrailingDash: true });
export const TransformUpperCase: Transform = value => value.toUpperCase();

@Directive({
    selector: 'input[sqxTransformInput][formField]',
    host: {
        '(input)': 'onInput($event)',
    },
})
export class TransformInputDirective {
    private readonly formField = inject(FormField);

    /** The transformation that is applied to the entered text, either a function or LowerCase, Slugify or UpperCase. */
    public readonly transform = input<Transform | string | undefined | null>(undefined, { alias: 'sqxTransformInput' });

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
        if (!Types.isString(target?.value)) {
            return;
        }

        const transformed = this.transformer()(target.value);
        if (transformed === target.value) {
            return;
        }

        // Update the element and the field, regardless of whether the form field has already processed the event.
        target.value = transformed;

        (this.formField.formField() as FieldTree<string>)().value.set(transformed);
    }
}
