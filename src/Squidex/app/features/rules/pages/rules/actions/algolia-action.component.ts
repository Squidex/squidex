/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

@Component({
    selector: 'sqx-algolia-action',
    styleUrls: ['./algolia-action.component.scss'],
    templateUrl: './algolia-action.component.html'
})
export class AlgoliaActionComponent implements OnInit {
    @Input()
    public action: any;

    @Output()
    public actionChanged = new EventEmitter<object>();

    public actionFormSubmitted = false;
    public actionForm =
        this.formBuilder.group({
            appId: ['',
                [
                    Validators.required
                ]],
            apiKey: ['',
                [
                    Validators.required
                ]],
            indexName: ['$SCHEMA_NAME',
                [
                    Validators.required
                ]]
        });

    constructor(
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.action = Object.assign({}, { appId: '', apiKey: '', indexName: '$SCHEMA_NAME' }, this.action || {});

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