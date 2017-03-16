/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Directive, forwardRef, ElementRef, Renderer } from '@angular/core';
import { ControlValueAccessor,  NG_VALUE_ACCESSOR } from '@angular/forms';

const NOOP = () => { /* NOOP */ };

export const SQX_INDETERMINATE_VALUE_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => IndeterminateValueDirective), multi: true
};

@Directive({
    selector: '[sqxIndeterminateValue]',
    providers: [SQX_INDETERMINATE_VALUE_CONTROL_VALUE_ACCESSOR],
    host: {
        '(change)': 'onChange($event.target.checked)', '(blur)': 'onTouched()'
    }
})
export class IndeterminateValueDirective implements ControlValueAccessor {
    private changeCallback: (value: any) => void = NOOP;
    private touchedCallback: () => void = NOOP;

    constructor(
        private readonly renderer: Renderer,
        private readonly elementRef: ElementRef
    ) {
    }

    public writeValue(value: any) {
        if (value === undefined || value === null) {
            this.renderer.setElementProperty(this.elementRef.nativeElement, 'indeterminate', true);
        } else {
            this.renderer.setElementProperty(this.elementRef.nativeElement, 'checked', value);
        }
    }

    public setDisabledState(isDisabled: boolean): void {
        this.renderer.setElementProperty(this.elementRef.nativeElement, 'disabled', isDisabled);
    }

    public registerOnChange(fn: any) {
        this.changeCallback = fn;
    }

    public registerOnTouched(fn: any) {
        this.touchedCallback = fn;
    }

    public onChange(value: any) {
        this.changeCallback(value);
    }

    public onTouched() {
        this.touchedCallback();
    }
}