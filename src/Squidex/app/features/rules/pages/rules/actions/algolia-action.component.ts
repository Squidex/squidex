/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
    selector: 'sqx-algolia-action',
    styleUrls: ['./algolia-action.component.scss'],
    templateUrl: './algolia-action.component.html'
})
export class AlgoliaActionComponent implements OnInit {
    @Input()
    public action: any;

    @Input()
    public actionForm: FormGroup;

    @Input()
    public actionFormSubmitted = false;

    public ngOnInit() {
        this.actionForm.setControl('appId',
            new FormControl(this.action.appId || '', [
                Validators.required
            ]));

        this.actionForm.setControl('apiKey',
            new FormControl(this.action.apiKey || '', [
                Validators.required
            ]));

        this.actionForm.setControl('indexName',
            new FormControl(this.action.indexName || '$SCHEMA_NAME', [
                Validators.required
            ]));
    }
}