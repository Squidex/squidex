/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ValidatorsEx } from '@app/shared';

@Component({
    selector: 'sqx-usage-trigger',
    styleUrls: ['./usage-trigger.component.scss'],
    templateUrl: './usage-trigger.component.html'
})
export class UsageTriggerComponent implements OnChanges {
    @Input()
    public trigger: any;

    @Input()
    public triggerForm: FormGroup;

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['triggerForm']) {
            this.triggerForm.setControl('limit',
                new FormControl(20000, [
                    Validators.required
                ]));

            this.triggerForm.setControl('numDays',
                new FormControl(3, [
                    ValidatorsEx.between(1, 30)
                ]));
        }

        this.triggerForm.patchValue(this.trigger);
    }
}