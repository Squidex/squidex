/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, Input } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';

import {
    MathHelper,
    StatefulControlComponent,
    Types
} from '@app/framework/internal';

export const SQX_CHECKBOX_GROUP_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => CheckboxGroupComponent), multi: true
};

interface State {
    checkedValues: string[];
}

@Component({
    selector: 'sqx-checkbox-group',
    styleUrls: ['./checkbox-group.component.scss'],
    templateUrl: './checkbox-group.component.html',
    providers: [SQX_CHECKBOX_GROUP_CONTROL_VALUE_ACCESSOR],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CheckboxGroupComponent extends StatefulControlComponent<State, string[]> {
    public readonly controlId = MathHelper.guid();

    @Input()
    public values: string[] = [];

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, {
            checkedValues: []
        });
    }

    public writeValue(obj: any) {
        const checkedValues = Types.isArrayOfString(obj) ? obj.filter(x => this.values.indexOf(x) >= 0) : [];

        this.next(s => ({ ...s, checkedValues }));
    }

    public check(isChecked: boolean, value: string) {
        let checkedValues = this.snapshot.checkedValues;

        if (isChecked) {
            checkedValues = [value, ...checkedValues];
        } else {
            checkedValues = checkedValues.filter(x => x !== value);
        }

        this.next(s => ({ ...s, checkedValues }));

        this.callChange(checkedValues);
    }

    public isChecked(value: string) {
        return this.snapshot.checkedValues.indexOf(value) >= 0;
    }
}
