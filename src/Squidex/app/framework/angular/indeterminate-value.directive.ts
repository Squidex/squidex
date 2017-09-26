/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Directive, forwardRef, ElementRef, HostListener, Renderer } from '@angular/core';
import { ControlValueAccessor,  NG_VALUE_ACCESSOR } from '@angular/forms';

import { Types } from './../utils/types';

export const SQX_INDETERMINATE_VALUE_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => IndeterminateValueDirective), multi: true
};

@Directive({
    selector: '[sqxIndeterminateValue]',
    providers: [SQX_INDETERMINATE_VALUE_CONTROL_VALUE_ACCESSOR]
})
export class IndeterminateValueDirective implements ControlValueAccessor {
    private callChange = (v: any) => { /* NOOP */ };
    private callTouched = () => { /* NOOP */ };

    constructor(
        private readonly renderer: Renderer,
        private readonly element: ElementRef
    ) {
    }

    @HostListener('change', ['$event.target.checked'])
    public onChange(value: any) {
        this.callChange(value);
    }

    @HostListener('blur')
    public onTouched() {
        this.callTouched();
    }

    public writeValue(value: boolean | number | undefined) {
        if (!Types.isBoolean(value)) {
            this.renderer.setElementProperty(this.element.nativeElement, 'indeterminate', true);
        } else {
            this.renderer.setElementProperty(this.element.nativeElement, 'checked', value);
        }
    }

    public setDisabledState(isDisabled: boolean): void {
        this.renderer.setElementProperty(this.element.nativeElement, 'disabled', isDisabled);
    }

    public registerOnChange(fn: any) {
        this.callChange = fn;
    }

    public registerOnTouched(fn: any) {
        this.callTouched = fn;
    }
}