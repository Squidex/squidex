/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
    selector: 'sqx-slack-action',
    styleUrls: ['./slack-action.component.scss'],
    templateUrl: './slack-action.component.html'
})
export class SlackActionComponent implements OnInit {
    @Input()
    public action: any;

    @Input()
    public actionForm: FormGroup;

    @Input()
    public actionFormSubmitted = false;

    public ngOnInit() {
        this.actionForm.setControl('webhookUrl',
            new FormControl(this.action.webhookUrl || '', [
                Validators.required
            ]));

        this.actionForm.setControl('text',
            new FormControl(this.action.text || '', [
                Validators.required
            ]));
    }
}