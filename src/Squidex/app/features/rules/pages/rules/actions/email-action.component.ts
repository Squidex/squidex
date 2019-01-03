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
        this.actionForm.setControl('serverHost',
            new FormControl(this.action.serverHost || 'smtp.gmail.com', [
                Validators.required
            ]));

        this.actionForm.setControl('serverPort',
            new FormControl(this.action.serverPort || 465, [
                Validators.required
            ]));

        this.actionForm.setControl('serverUseSsl',
            new FormControl(this.action.serverUseSsl || true));

        this.actionForm.setControl('serverUsername',
            new FormControl(this.action.serverUsername || '', [
                Validators.required
            ]));

        this.actionForm.setControl('serverPassword',
            new FormControl(this.action.serverPassword || '', [
                Validators.required
            ]));

        this.actionForm.setControl('messageFrom',
            new FormControl(this.action.messageFrom || '', [
                Validators.required
            ]));

        this.actionForm.setControl('messageTo',
            new FormControl(this.action.messageTo || '', [
                Validators.required
            ]));

        this.actionForm.setControl('messageSubject',
            new FormControl(this.action.messageSubject || '', [
                Validators.required
            ]));

        this.actionForm.setControl('messageBody',
            new FormControl(this.action.messageBody || '', [
                Validators.required
            ]));
    }
}