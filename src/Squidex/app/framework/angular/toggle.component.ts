/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

const NOOP = () => { /* NOOP */ };

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
    private changeCallback: (value: any) => void = NOOP;
    private touchedCallback: () => void = NOOP;

    public isChecked: boolean | undefined = undefined;
    public isDisabled = false;

    public writeValue(value: any) {
        this.isChecked = value;
    }

    public setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;
    }

    public registerOnChange(fn: any) {
        this.changeCallback = fn;
    }

    public registerOnTouched(fn: any) {
        this.touchedCallback = fn;
    }

    public changeState() {
        if (this.isDisabled) {
            return;
        }
        this.isChecked = !(this.isChecked === true);

        this.changeCallback(this.isChecked);
        this.touchedCallback();
    }
}