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
        this.actionForm.setControl('host',
            new FormControl(this.action.host || 'smtp.gmail.com', [
                Validators.required
            ]));

        this.actionForm.setControl('port',
            new FormControl(this.action.port || 465, [
                Validators.required,
                Validators.pattern(/\d{2,6}/)
            ]));

        this.actionForm.setControl('enableSsl',
            new FormControl(this.action.enableSsl || true));

        this.actionForm.setControl('username',
            new FormControl(this.action.username || '', [
                Validators.required
            ]));

        this.actionForm.setControl('password',
            new FormControl(this.action.password || '', [
                Validators.required
            ]));

        this.actionForm.setControl('from',
            new FormControl(this.action.from || '', [
                Validators.required
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