/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, Input } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

import { Types } from '@app/framework/internal';
import { MathHelper } from '@app/shared';

export const SQX_CHECKBOX_GROUP_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => CheckboxGroupComponent), multi: true
};

@Component({
    selector: 'sqx-checkbox-group',
    styleUrls: ['./checkbox-group.component.scss'],
    templateUrl: './checkbox-group.component.html',
    providers: [SQX_CHECKBOX_GROUP_CONTROL_VALUE_ACCESSOR],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CheckboxGroupComponent implements ControlValueAccessor {
    private callChange = (v: any) => { /* NOOP */ };
    private callTouched = () => { /* NOOP */ };
    private checkedValues: string[] = [];

    @Input()
    public values: string[] = [];

    public isDisabled = false;

    public control = MathHelper.guid();

    constructor(
        private readonly changeDetector: ChangeDetectorRef
    ) {
    }

    public writeValue(obj: any) {
        this.checkedValues = Types.isArrayOfString(obj) ? obj.filter(x => this.values.indexOf(x) >= 0) : [];

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

    public blur() {
        this.callTouched();
    }

    public check(isChecked: boolean, value: string) {
        if (isChecked) {
            this.checkedValues = [value, ...this.checkedValues];
        } else {
            this.checkedValues = this.checkedValues.filter(x => x !== value);
        }

        this.callChange(this.checkedValues);
    }

    public isChecked(value: string) {
        return this.checkedValues.indexOf(value) >= 0;
    }
}
