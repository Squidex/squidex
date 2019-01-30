/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, Component, forwardRef, Input } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

import { Types } from '@app/framework/internal';

import { StatefulControlComponent } from '../stateful.component';

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
export class ToggleComponent extends StatefulControlComponent<State, boolean | null> implements ControlValueAccessor {
    @Input()
    public threeStates = false;

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, {
            isChecked: null
        });
    }

    public writeValue(obj: any) {
        this.next({ isChecked: Types.isBoolean(obj) ? obj : null });
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

        this.next({ isChecked });

        this.callChange(isChecked);
        this.callTouched();
    }
}