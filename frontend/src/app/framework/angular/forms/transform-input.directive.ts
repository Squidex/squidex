/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed, Directive, ElementRef, forwardRef, inject, input, Renderer2 } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { generateSlug, Types } from '@app/framework/internal';

type Transform = (value: string) => string;

export const TransformNoop: Transform = value => value;
export const TransformLowerCase: Transform = value => value.toLowerCase();
export const TransformSlugify: Transform = value => generateSlug(value, { preserveTrailingDash: true });
export const TransformUpperCase: Transform = value => value.toUpperCase();

export const SQX_TRANSFORM_INPUT_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => TransformInputDirective), multi: true,
};

@Directive({
    selector: '[sqxTransformInput]',
    providers: [
        SQX_TRANSFORM_INPUT_VALUE_ACCESSOR,
    ],
    host: {
        '(input)': 'onChange($event)',
        '(blur)': 'onTouched()',
    },
})
export class TransformInputDirective implements ControlValueAccessor {
    private readonly element = inject(ElementRef);
    private readonly renderer = inject(Renderer2);
    private callChange = (_: any) => { /* NOOP */ };
    private callTouched = () => { /* NOOP */ };

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

    protected onChange(event: Event): void {
        const target = event.target as HTMLInputElement;
        if (target?.value === undefined) {
            return;
        }

        const normalizedValue = this.transformValue(target.value);

        this.renderer.setProperty(this.element.nativeElement, 'value', normalizedValue);
        this.callChange(normalizedValue);
    }

    protected onTouched() {
        this.callTouched();
    }

    public writeValue(obj: any) {
        const normalizedValue = this.transformValue(Types.isString(obj) ? obj : '');

        this.renderer.setProperty(this.element.nativeElement, 'value', normalizedValue);
    }

    public setDisabledState(isDisabled: boolean) {
        this.renderer.setProperty(this.element.nativeElement, 'disabled', isDisabled);
    }

    public registerOnChange(fn: any) {
        this.callChange = fn;
    }

    public registerOnTouched(fn: any) {
        this.callTouched = fn;
    }

    private transformValue(value: any): string {
        return Types.isString(value) ? this.transformer()(value) : '';
    }
}
