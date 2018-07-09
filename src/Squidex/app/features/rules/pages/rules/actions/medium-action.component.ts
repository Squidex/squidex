/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
    selector: 'sqx-medium-action',
    styleUrls: ['./medium-action.component.scss'],
    templateUrl: './medium-action.component.html'
})
export class MediumActionComponent implements OnInit {
    @Input()
    public action: any;

    @Input()
    public actionForm: FormGroup;

    @Input()
    public actionFormSubmitted = false;

    public ngOnInit() {
        this.actionForm.setControl('accessToken',
            new FormControl(this.action.accessToken || '', [
                Validators.required
            ]));

        this.actionForm.setControl('title',
            new FormControl(this.action.title || '', [
                Validators.required
            ]));

        this.actionForm.setControl('content',
            new FormControl(this.action.content || '', [
                Validators.required
            ]));

        this.actionForm.setControl('canonicalUrl',
            new FormControl(this.action.canonicalUrl || ''));

        this.actionForm.setControl('tags',
            new FormControl(this.action.tags || ''));

        this.actionForm.setControl('isHtml',
            new FormControl(this.action.isHtml || false));
    }
}