/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Directive, ElementRef, forwardRef, inject, input, Renderer2 } from '@angular/core';
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
    host: {
        '(click)': 'onClick()',
        '(blur)': 'onTouched()',
    },
})
export class IndeterminateValueDirective implements ControlValueAccessor {
    private readonly element = inject<ElementRef<HTMLInputElement>>(ElementRef);
    private readonly renderer = inject(Renderer2);
    private callChange = (_: any) => { /* NOOP */ };
    private callTouched = () => { /* NOOP */ };
    private isChecked?: boolean | null;

    /** Cycles through checked, unchecked and indeterminate on click instead of only checked and unchecked. */
    public readonly threeStates = input(true, { transform: booleanAttribute });

    protected onClick() {
        let isChecked = this.isChecked;

        if (this.threeStates()) {
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

    protected onTouched() {
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
