/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

@Component({
    selector: 'sqx-elastic-search-action',
    styleUrls: ['./elastic-search-action.component.scss'],
    templateUrl: './elastic-search-action.component.html'
})
export class ElasticSearchActionComponent implements OnInit {
    @Input()
    public action: any;

    @Output()
    public actionChanged = new EventEmitter<object>();

    public actionFormSubmitted = false;
    public actionForm =
        this.formBuilder.group({
            host: ['',
                [
                    Validators.required
                ]],
            indexName: ['$APP_NAME',
                [
                    Validators.required
                ]],
            indexType: ['$SCHEMA_NAME',
                [
                    // Validators.required
                ]],
            username: '',
            password: ''
        });

    constructor(
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.action = Object.assign({}, {
            host: '',
            indexName: '$APP_NAME',
            indexType: '$SCHEMA_NAME',
            username: '',
            password: ''
        }, this.action || {});

        this.actionFormSubmitted = false;
        this.actionForm.reset();
        this.actionForm.setValue(this.action);
    }

    public save() {
        this.actionFormSubmitted = true;

        if (this.actionForm.valid) {
            const action = this.actionForm.value;

            this.actionChanged.emit(action);
        }
    }
}