/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

@Component({
    selector: 'sqx-webhook-action',
    styleUrls: ['./webhook-action.component.scss'],
    templateUrl: './webhook-action.component.html'
})
export class WebhookActionComponent implements OnInit {
    @Input()
    public action: any;

    @Output()
    public actionChanged = new EventEmitter<object>();

    public actionFormSubmitted = false;
    public actionForm =
        this.formBuilder.group({
            url: ['',
                [
                    Validators.required
                ]],
            sharedSecret: ['']
        });

    constructor(
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.action = Object.assign({}, { url: '', sharedSecret: '' }, this.action || {});

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