/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ApiUrlConfig, AppsState, CreateSchemaForm, SchemaDto, SchemasState } from '@app/shared';

@Component({
    selector: 'sqx-schema-form',
    styleUrls: ['./schema-form.component.scss'],
    templateUrl: './schema-form.component.html',
})
export class SchemaFormComponent implements OnInit {
    @Output()
    public complete = new EventEmitter<SchemaDto>();

    @Output()
    public cancel = new EventEmitter();

    @Input()
    public import: any;

    public createForm = new CreateSchemaForm(this.formBuilder);

    public showImport = false;

    constructor(
        public readonly apiUrl: ApiUrlConfig,
        public readonly appsState: AppsState,
        public readonly schemasState: SchemasState,
        private readonly formBuilder: FormBuilder,
    ) {
    }

    public ngOnInit() {
        this.createForm.load({ type: 'Default', ...this.import, name: '' });

        this.showImport = !!this.import;
    }

    public toggleImport() {
        this.showImport = !this.showImport;

        return false;
    }

    public emitComplete(value: SchemaDto) {
        this.complete.emit(value);
    }

    public createSchema() {
        const value = this.createForm.submit();

        if (value) {
            this.schemasState.create(value)
                .subscribe({
                    next: dto => {
                        this.emitComplete(dto);
                    },
                    error: error => {
                        this.createForm.submitFailed(error);
                    },
                });
        }
    }
}
