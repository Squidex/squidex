/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import { ValidatorsEx } from 'shared';

@Component({
    selector: 'sqx-azure-queue-action',
    styleUrls: ['./azure-queue-action.component.scss'],
    templateUrl: './azure-queue-action.component.html'
})
export class AzureQueueActionComponent implements OnInit {
    @Input()
    public action: any;

    @Output()
    public actionChanged = new EventEmitter<object>();

    public actionFormSubmitted = false;
    public actionForm =
        this.formBuilder.group({
            connectionString: ['',
                [
                    Validators.required
                ]],
            queue: ['squidex',
                [
                    Validators.required,
                    ValidatorsEx.pattern('[a-z][a-z0-9]{2,}(\-[a-z0-9]+)*', 'Name must be a valid azure queue name.')
                ]]
        });

    constructor(
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.action = Object.assign({}, { connectionString: '', queue: 'squidex' }, this.action || {});

        this.actionFormSubmitted = false;
        this.actionForm.reset();
        this.actionForm.setValue(this.action);
    }

    public save() {
        this.actionFormSubmitted = true;

        if (this.actionForm.valid) {
            const action = this.actionForm.value;

            this.actionChanged.emit(action);
        }
    }
}