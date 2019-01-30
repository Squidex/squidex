/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, Input } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

import {
    MathHelper,
    StatefulComponent,
    Types
} from '@app/framework/internal';

export const SQX_CHECKBOX_GROUP_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => CheckboxGroupComponent), multi: true
};

interface State {
    checkedValues: string[];
    controlId: string;
    isDisabled: boolean;
}

@Component({
    selector: 'sqx-checkbox-group',
    styleUrls: ['./checkbox-group.component.scss'],
    templateUrl: './checkbox-group.component.html',
    providers: [SQX_CHECKBOX_GROUP_CONTROL_VALUE_ACCESSOR],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CheckboxGroupComponent extends StatefulComponent<State> implements ControlValueAccessor {
    private callChange = (v: any) => { /* NOOP */ };
    private callTouched = () => { /* NOOP */ };

    @Input()
    public values: string[] = [];

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, {
            controlId: MathHelper.guid(),
            checkedValues: [],
            isDisabled: false
        });
    }

    public writeValue(obj: any) {
        this.next({ checkedValues: Types.isArrayOfString(obj) ? obj.filter(x => this.values.indexOf(x) >= 0) : [] });
    }

    public setDisabledState(isDisabled: boolean): void {
        this.next({ isDisabled });
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
        let checkedValues = this.snapshot.checkedValues;

        if (isChecked) {
            checkedValues = [value, ...checkedValues];
        } else {
            checkedValues = checkedValues.filter(x => x !== value);
        }

        this.next({ checkedValues });

        this.callChange(checkedValues);
        }

    public isChecked(value: string) {
        return this.snapshot.checkedValues.indexOf(value) >= 0;
    }
}
