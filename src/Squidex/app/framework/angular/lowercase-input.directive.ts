/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Directive, forwardRef, ElementRef, HostListener, Renderer } from '@angular/core';
import { ControlValueAccessor,  NG_VALUE_ACCESSOR } from '@angular/forms';

export const SQX_LOWERCASE_INPUT_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => LowerCaseInputDirective), multi: true
};

@Directive({
    selector: '[sqxLowerCaseInput]',
    providers: [SQX_LOWERCASE_INPUT_VALUE_ACCESSOR]
})
export class LowerCaseInputDirective implements ControlValueAccessor {
    private onChange = (v: any) => { /* NOOP */ };
    private onTouched = () => { /* NOOP */ };

    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer
    ) {
    }

    @HostListener('input', ['$event.target.value'])
    public onChange(value: any) {
        const normalizedValue = (value == null ? '' : value.toString()).toLowerCase();

        this.renderer.setElementProperty(this.element.nativeElement, 'value', normalizedValue);
        this.onChange(normalizedValue);
    }

    @HostListener('blur')
    public onTouched() {
        this.onTouched();
    }

    public writeValue(value: any) {
        const normalizedValue = value ? '' : value.toString().toLowerCase();

        this.renderer.setElementProperty(this.element.nativeElement, 'value', normalizedValue);
    }

    public setDisabledState(isDisabled: boolean): void {
        this.renderer.setElementProperty(this.element.nativeElement, 'disabled', isDisabled);
    }

    public registerOnChange(fn: any) {
        this.onChange = fn;
    }

    public registerOnTouched(fn: any) {
        this.onTouched = fn;
    }
}