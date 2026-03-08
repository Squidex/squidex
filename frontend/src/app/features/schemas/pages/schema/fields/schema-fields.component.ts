/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, forwardRef, Input } from '@angular/core';
import { AppLanguageDto, AppsState, DialogModel, FieldDto, fieldTypes, ModalDirective, SchemaDto, SchemasState, TourStepDirective, TranslatePipe } from '@app/shared';
import { FieldWizardComponent } from './field-wizard.component';
import { SortableFieldListComponent } from './sortable-field-list.component';

@Component({
    selector: 'sqx-schema-fields',
    styleUrls: ['./schema-fields.component.scss'],
    templateUrl: './schema-fields.component.html',
    imports: [
        AsyncPipe,
        FieldWizardComponent,
        ModalDirective,
        TourStepDirective,
        TranslatePipe,
        forwardRef(() => SortableFieldListComponent),
    ],
})
export class SchemaFieldsComponent {
    @Input({ required: true })
    public schema!: SchemaDto;

    @Input({ required: true })
    public languages!: AppLanguageDto[];

    public fieldTypes = fieldTypes;
    public fieldWizard = new DialogModel();

    constructor(
        public readonly appsState: AppsState,
        public readonly schemasState: SchemasState,
    ) {
    }

    public sortFields(fields: ReadonlyArray<FieldDto>) {
        this.schemasState.orderFields(this.schema, fields).subscribe();
    }
}
