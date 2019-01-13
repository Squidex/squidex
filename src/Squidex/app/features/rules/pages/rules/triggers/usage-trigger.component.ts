/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';

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

    @Input()
    public triggerFormSubmitted = false;

    public ngOnInit() {
        this.triggerForm.setControl('limit',
            new FormControl(this.trigger.limit || 20000));
    }
}