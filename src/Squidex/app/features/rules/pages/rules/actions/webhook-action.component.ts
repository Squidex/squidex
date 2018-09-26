/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
    selector: 'sqx-webhook-action',
    styleUrls: ['./webhook-action.component.scss'],
    templateUrl: './webhook-action.component.html'
})
export class WebhookActionComponent implements OnInit {
    @Input()
    public action: any;

    @Input()
    public actionForm: FormGroup;

    @Input()
    public actionFormSubmitted = false;

    public ngOnInit() {
        this.actionForm.setControl('url',
            new FormControl(this.action.url || '', [
                Validators.required
            ]));

        this.actionForm.setControl('sharedSecret',
            new FormControl(this.action.sharedSecret || ''));
    }
}