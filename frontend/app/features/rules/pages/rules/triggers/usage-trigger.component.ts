/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ValidatorsEx } from '@app/shared';

@Component({
    selector: 'sqx-usage-trigger',
    styleUrls: ['./usage-trigger.component.scss'],
    templateUrl: './usage-trigger.component.html'
})
export class UsageTriggerComponent implements OnInit {
    @Input()
    public trigger: any;

    @Input()
    public triggerForm: FormGroup;

    public ngOnInit() {
        this.triggerForm.setControl('limit',
            new FormControl(this.trigger.limit || 20000, [
                Validators.required
            ]));

        this.triggerForm.setControl('numDays',
            new FormControl(this.trigger.numDays, [
                ValidatorsEx.between(1, 30)
            ]));
    }
}