/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgIf } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { AppsState, DialogModel, FieldDto, fieldTypes, LanguagesState, ModalDirective, SchemaDto, SchemasState, TourStepDirective, TranslatePipe } from '@app/shared';
import { FieldWizardComponent } from './field-wizard.component';
import { SortableFieldListComponent } from './sortable-field-list.component';

@Component({
    selector: 'sqx-schema-fields',
    styleUrls: ['./schema-fields.component.scss'],
    templateUrl: './schema-fields.component.html',
    standalone: true,
    imports: [
        NgIf,
        TourStepDirective,
        SortableFieldListComponent,
        ModalDirective,
        FieldWizardComponent,
        AsyncPipe,
        TranslatePipe,
    ],
})
export class SchemaFieldsComponent implements OnInit {
    @Input({ required: true })
    public schema!: SchemaDto;

    public fieldTypes = fieldTypes;
    public fieldWizard = new DialogModel();

    constructor(
        public readonly appsState: AppsState,
        public readonly schemasState: SchemasState,
        public readonly languageState: LanguagesState,
    ) {
    }

    public ngOnInit() {
        this.languageState.load();
    }

    public sortFields(fields: ReadonlyArray<FieldDto>) {
        this.schemasState.orderFields(this.schema, fields).subscribe();
    }
}
