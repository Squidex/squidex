/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import { SchemaDetailsDto, SynchronizeSchemaForm } from '@app/shared';

@Component({
    selector: 'sqx-schema-export-form',
    styleUrls: ['./schema-export-form.component.scss'],
    templateUrl: './schema-export-form.component.html'
})
export class SchemaExportFormComponent implements OnChanges {
    @Output()
    public complete = new EventEmitter();

    @Input()
    public schema: SchemaDetailsDto;

    public synchronizeForm = new SynchronizeSchemaForm(this.formBuilder);

    constructor(
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnChanges() {
        this.synchronizeForm.form.get('json')!.setValue(this.schema.export());
    }

    public synchronizeSchema() {
        alert('Sync');
    }

    public emitComplete() {
        this.complete.emit();
    }
}