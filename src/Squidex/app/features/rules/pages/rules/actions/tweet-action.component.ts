/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
    selector: 'sqx-tweet-action',
    styleUrls: ['./tweet-action.component.scss'],
    templateUrl: './tweet-action.component.html'
})
export class TweetActionComponent implements OnInit {
    @Input()
    public action: any;

    @Input()
    public actionForm: FormGroup;

    @Input()
    public actionFormSubmitted = false;

    public ngOnInit() {
        this.actionForm.setControl('pinCode',
            new FormControl(this.action.pinCode || '', [
                Validators.required
            ]));

        this.actionForm.setControl('text',
            new FormControl(this.action.text || '', [
                Validators.required
            ]));
    }
}