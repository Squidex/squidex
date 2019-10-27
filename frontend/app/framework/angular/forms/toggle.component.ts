/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, Component, forwardRef, Input } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';

import { StatefulControlComponent, Types } from '@app/framework/internal';

export const SQX_TOGGLE_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ToggleComponent), multi: true
};

interface State {
    isChecked: boolean | null;
}

@Component({
    selector: 'sqx-toggle',
    styleUrls: ['./toggle.component.scss'],
    templateUrl: './toggle.component.html',
    providers: [SQX_TOGGLE_CONTROL_VALUE_ACCESSOR]
})
export class ToggleComponent extends StatefulControlComponent<State, boolean | null> {
    @Input()
    public threeStates = false;

    public set disabled(value: boolean) {
        this.setDisabledState(value);
    }

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, {
            isChecked: null
        });
    }

    public writeValue(obj: any) {
        const isChecked = Types.isBoolean(obj) ? obj : null;

        this.next(s => ({ ...s, isChecked  }));
    }

    public changeState(event: MouseEvent) {
        let { isDisabled, isChecked } = this.snapshot;

        if (isDisabled) {
            return;
        }

        if (this.threeStates && (event.ctrlKey || event.shiftKey)) {
            if (isChecked) {
                isChecked = null;
            } else if (isChecked === null) {
                isChecked = false;
            } else {
                isChecked = true;
            }
        } else {
            isChecked = !(isChecked === true);
        }

        this.next(s => ({ ...s, isChecked }));

        this.callChange(isChecked);
        this.callTouched();
    }
}