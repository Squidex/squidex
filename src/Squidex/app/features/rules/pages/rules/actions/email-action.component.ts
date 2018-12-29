/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
    selector: 'sqx-email-action',
    styleUrls: ['./email-action.component.scss'],
    templateUrl: './email-action.component.html'
})
export class EmailActionComponent implements OnInit {
    @Input()
    public action: any;

    @Input()
    public actionForm: FormGroup;

    @Input()
    public actionFormSubmitted = false;

    public ngOnInit() {
        this.actionForm.setControl('from',
            new FormControl(this.action.from || '', [
                Validators.required,
                Validators.email
            ]));

        this.actionForm.setControl('to',
            new FormControl(this.action.to || '', [
                Validators.required
            ]));

        this.actionForm.setControl('subject',
            new FormControl(this.action.subject || '', [
                Validators.required
            ]));

        this.actionForm.setControl('body',
            new FormControl(this.action.body || '', [
                Validators.required
            ]));
    }
}