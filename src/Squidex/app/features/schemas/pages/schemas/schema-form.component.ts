/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import {
    ApiUrlConfig,
    AppsState,
    ValidatorsEx
} from '@app/shared';

import { SchemasState } from './../../state/schemas.state';

const FALLBACK_NAME = 'my-schema';

@Component({
    selector: 'sqx-schema-form',
    styleUrls: ['./schema-form.component.scss'],
    templateUrl: './schema-form.component.html'
})
export class SchemaFormComponent implements OnInit {
    @Output()
    public completed = new EventEmitter();

    @Input()
    public import: any;

    public showImport = false;

    public createFormError = '';
    public createFormSubmitted = false;
    public createForm =
        this.formBuilder.group({
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'Name can contain lower case letters (a-z), numbers and dashes only (not at the end).')
                ]],
            import: [{}]
        });

    public schemaName =
        this.createForm.controls['name'].valueChanges.map(n => n || FALLBACK_NAME)
            .startWith(FALLBACK_NAME);

    constructor(
        public readonly apiUrl: ApiUrlConfig,
        public readonly appsState: AppsState,
        private readonly schemasState: SchemasState,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.createForm.controls['import'].setValue(this.import || {});

        this.showImport = !!this.import;
    }

    public toggleImport() {
        this.showImport = !this.showImport;

        return false;
    }

    public complete() {
        this.completed.emit();
    }

    public createSchema() {
        this.createFormSubmitted = true;

        if (this.createForm.valid) {
            this.createForm.disable();

            const schemaName = this.createForm.controls['name'].value;
            const schemaDto = Object.assign(this.createForm.controls['import'].value || {}, { name: schemaName });

            this.schemasState.create(schemaDto)
                .subscribe(dto => {
                    this.complete();
                }, error => {
                    this.enableCreateForm(error.displayMessage);
                });
        }
    }

    private enableCreateForm(message: string) {
        this.createForm.enable();
        this.createFormError = message;
    }
}