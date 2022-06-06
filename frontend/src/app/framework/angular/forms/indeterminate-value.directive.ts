/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, forwardRef, HostListener, Input, Renderer2 } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Types } from '@app/framework/internal';

export const SQX_INDETERMINATE_VALUE_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => IndeterminateValueDirective), multi: true,
};

@Directive({
    selector: '[sqxIndeterminateValue]',
    providers: [
        SQX_INDETERMINATE_VALUE_CONTROL_VALUE_ACCESSOR,
    ],
})
export class IndeterminateValueDirective implements ControlValueAccessor {
    private callChange = (_: any) => { /* NOOP */ };
    private callTouched = () => { /* NOOP */ };
    private isChecked?: boolean | null;

    @Input()
    public threeStates = true;

    constructor(
        private readonly element: ElementRef<HTMLInputElement>,
        private readonly renderer: Renderer2,
    ) {
    }

    @HostListener('click')
    public onClick() {
        let isChecked = this.isChecked;

        if (this.threeStates) {
            if (isChecked) {
                isChecked = null;
            } else {
                isChecked = isChecked !== null;
            }
        } else {
            isChecked = !(isChecked === true);
        }

        if (isChecked !== this.isChecked) {
            this.callChange(isChecked);
        }

        this.writeValue(isChecked);
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

        this.isChecked = obj;
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
}
