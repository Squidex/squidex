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
    ModalModel,
    StatefulControlComponent
} from '@app/framework/internal';

export const SQX_COLOR_PICKER_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ColorPickerComponent), multi: true
};

interface State {
    value?: string;

    foreground: string;
}

@Component({
    selector: 'sqx-color-picker',
    styleUrls: ['./color-picker.component.scss'],
    templateUrl: './color-picker.component.html',
    providers: [SQX_COLOR_PICKER_CONTROL_VALUE_ACCESSOR],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ColorPickerComponent extends StatefulControlComponent<State, string> {
    @Input()
    public placeholder = '';

    @Input()
    public mode: 'Input' | 'Circle' = 'Input';

    public modal = new ModalModel();

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, { foreground: 'black' });
    }

    public writeValue(obj: any) {
        let foreground = 'black';

        if (MathHelper.toLuminance(MathHelper.parseColor(obj)!) < .5) {
            foreground = 'white';
        }

        this.next(s => ({ ...s, value: obj, foreground }));

        this.callChange(obj);
    }

    public blur() {
        this.callTouched();
    }
}