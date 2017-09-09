/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

import { Types } from './../utils/types';

export const SQX_TOGGLE_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ToggleComponent), multi: true
};

@Component({
    selector: 'sqx-toggle',
    styleUrls: ['./toggle.component.scss'],
    templateUrl: './toggle.component.html',
    providers: [SQX_TOGGLE_CONTROL_VALUE_ACCESSOR]
})
export class ToggleComponent implements ControlValueAccessor {
    private callChange = (v: any) => { /* NOOP */ };
    private callTouched = () => { /* NOOP */ };

    public isChecked: boolean | null = null;
    public isDisabled = false;

    public writeValue(value: boolean | null | undefined) {
        this.isChecked = Types.isBoolean(value) ? value || null : null;
    }

    public setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;
    }

    public registerOnChange(fn: any) {
        this.callChange = fn;
    }

    public registerOnTouched(fn: any) {
        this.callTouched = fn;
    }

    public changeState() {
        if (this.isDisabled) {
            return;
        }

        this.isChecked = !(this.isChecked === true);

        this.callChange(this.isChecked);
        this.callTouched();
    }
}