/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
    selector: 'sqx-discourse-action',
    styleUrls: ['./discourse-action.component.scss'],
    templateUrl: './discourse-action.component.html'
})
export class DiscourseActionComponent implements OnInit {
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

        this.actionForm.setControl('apiKey',
            new FormControl(this.action.apiKey || '', [
                Validators.required
            ]));

        this.actionForm.setControl('apiUsername',
            new FormControl(this.action.apiUsername || '', [
                Validators.required
            ]));

        this.actionForm.setControl('text',
            new FormControl(this.action.text || '', [
                Validators.required
            ]));

        this.actionForm.setControl('title',
            new FormControl(this.action.title));

        this.actionForm.setControl('topic',
            new FormControl(this.action.topic));

        this.actionForm.setControl('category',
            new FormControl(this.action.category));
    }
}