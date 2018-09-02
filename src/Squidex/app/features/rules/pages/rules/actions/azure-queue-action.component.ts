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
    selector: 'sqx-azure-queue-action',
    styleUrls: ['./azure-queue-action.component.scss'],
    templateUrl: './azure-queue-action.component.html'
})
export class AzureQueueActionComponent implements OnInit {
    @Input()
    public action: any;

    @Input()
    public actionForm: FormGroup;

    @Input()
    public actionFormSubmitted = false;

    public ngOnInit() {
        this.actionForm.setControl('connectionString',
            new FormControl(this.action.connectionString || '', [
                Validators.required
            ]));

        this.actionForm.setControl('queue',
            new FormControl(this.action.queue || 'squidex', [
                Validators.required,
                ValidatorsEx.pattern('[a-z][a-z0-9]{2,}(\-[a-z0-9]+)*', 'Name must be a valid azure queue name.')
            ]));
    }
}