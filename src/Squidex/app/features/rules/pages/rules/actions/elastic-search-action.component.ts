/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
    selector: 'sqx-elastic-search-action',
    styleUrls: ['./elastic-search-action.component.scss'],
    templateUrl: './elastic-search-action.component.html'
})
export class ElasticSearchActionComponent implements OnInit {
    @Input()
    public action: any;

    @Input()
    public actionForm: FormGroup;

    @Input()
    public actionFormSubmitted = false;

    public ngOnInit() {
        this.actionForm.setControl('host',
            new FormControl(this.action.host || '', [
                Validators.required
            ]));

        this.actionForm.setControl('indexName',
            new FormControl(this.action.indexName || '$APP_NAME', [
                Validators.required
            ]));

        this.actionForm.setControl('indexType',
            new FormControl(this.action.indexType || '$SCHEMA_NAME'));

        this.actionForm.setControl('username',
            new FormControl(this.action.username));

        this.actionForm.setControl('password',
            new FormControl(this.action.password));
    }
}