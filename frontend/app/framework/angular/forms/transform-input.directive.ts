/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, forwardRef, HostListener, Input, Renderer2 } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Types } from '@app/framework/internal';
import slugify from 'slugify';

type Transform = (value: string) => string;

export const TransformNoop: Transform = value => value;
export const TransformLowerCase: Transform = value => value.toLowerCase();
export const TransformSlugify: Transform = value => slugify(value, { lower: true });
export const TransformSlugifyCased: Transform = value => slugify(value, { lower: false });
export const TransformUpperCase: Transform = value => value.toUpperCase();

export const SQX_TRANSFORM_INPUT_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => TransformInputDirective), multi: true,
};

@Directive({
    selector: '[sqxTransformInput]',
    providers: [
        SQX_TRANSFORM_INPUT_VALUE_ACCESSOR,
    ],
})
export class TransformInputDirective implements ControlValueAccessor {
    private callChange = (_: any) => { /* NOOP */ };
    private callTouched = () => { /* NOOP */ };
    private transformer: Transform;

    @Input('sqxTransformInput')
    public set transform(value: Transform | string) {
        if (Types.isString(value)) {
            if (value === 'LowerCase') {
                this.transformer = TransformLowerCase;
            } else if (value === 'Slugify') {
                this.transform = TransformSlugify;
            } else if (value === 'SlugifyCased') {
                this.transform = TransformSlugifyCased;
            } else if (value === 'UpperCase') {
                this.transform = TransformUpperCase;
            }
        } else {
            this.transformer = value || TransformNoop;
        }
    }

    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer2,
    ) {
    }

    @HostListener('input', ['$event.target.value'])
    public onChange(value: any) {
        const normalizedValue = this.transformValue(value);

        this.renderer.setProperty(this.element.nativeElement, 'value', normalizedValue);

        this.callChange(normalizedValue);
    }

    @HostListener('blur')
    public onTouched() {
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
        return Types.isString(value) ? this.transformer(value) : '';
    }
}
