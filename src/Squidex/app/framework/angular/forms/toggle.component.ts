/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

import { Types } from '@app/framework/internal';

export const SQX_TOGGLE_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ToggleComponent), multi: true
};

@Component({
    selector: 'sqx-toggle',
    styleUrls: ['./toggle.component.scss'],
    templateUrl: './toggle.component.html',
    providers: [SQX_TOGGLE_CONTROL_VALUE_ACCESSOR],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ToggleComponent implements ControlValueAccessor {
    private callChange = (v: any) => { /* NOOP */ };
    private callTouched = () => { /* NOOP */ };

    public isChecked: boolean | null = null;
    public isDisabled = false;

    constructor(
        private readonly changeDetector: ChangeDetectorRef
    ) {
    }

    public writeValue(obj: any) {
        this.isChecked = Types.isBoolean(obj) ? obj : null;

        this.changeDetector.markForCheck();
    }

    public setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;

        this.changeDetector.markForCheck();
    }

    public registerOnChange(fn: any) {
        this.callChange = fn;
    }

    public registerOnTouched(fn: any) {
        this.callTouched = fn;
    }

    public changeState(event: MouseEvent) {
        if (this.isDisabled) {
            return;
        }

        if (event.ctrlKey || event.shiftKey) {
            if (this.isChecked) {
                this.isChecked = null;
            } else if (this.isChecked === null) {
                this.isChecked = false;
            } else {
                this.isChecked = true;
            }
        } else {
            this.isChecked = !(this.isChecked === true);
        }

        this.callChange(this.isChecked);
        this.callTouched();
    }
}