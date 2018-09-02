/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
    selector: 'sqx-fastly-action',
    styleUrls: ['./fastly-action.component.scss'],
    templateUrl: './fastly-action.component.html'
})
export class FastlyActionComponent implements OnInit {
    @Input()
    public action: any;

    @Input()
    public actionForm: FormGroup;

    @Input()
    public actionFormSubmitted = false;

    public ngOnInit() {
        this.actionForm.setControl('serviceId',
            new FormControl(this.action.serviceId || '', [
                Validators.required
            ]));

        this.actionForm.setControl('apiKey',
            new FormControl(this.action.apiKey || '', [
                Validators.required
            ]));
    }
}