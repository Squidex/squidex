/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, forwardRef, HostListener, Renderer2 } from '@angular/core';
import { ControlValueAccessor,  NG_VALUE_ACCESSOR } from '@angular/forms';

import { Types } from '@app/framework/internal';

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
        private readonly element: ElementRef,
        private readonly renderer: Renderer2
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

    public writeValue(obj: any) {
        if (!Types.isBoolean(obj)) {
            this.renderer.setProperty(this.element.nativeElement, 'indeterminate', true);
            this.renderer.setProperty(this.element.nativeElement, 'checked', false);
        } else {
            this.renderer.setProperty(this.element.nativeElement, 'indeterminate', false);
            this.renderer.setProperty(this.element.nativeElement, 'checked', obj);
        }
    }

    public setDisabledState(isDisabled: boolean): void {
        this.renderer.setProperty(this.element.nativeElement, 'disabled', isDisabled);
    }

    public registerOnChange(fn: any) {
        this.callChange = fn;
    }

    public registerOnTouched(fn: any) {
        this.callTouched = fn;
    }
}