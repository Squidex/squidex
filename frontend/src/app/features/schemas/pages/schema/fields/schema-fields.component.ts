/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { AppsState, DialogModel, FieldDto, fieldTypes, LanguagesState, SchemaDto, SchemasState } from '@app/shared';

@Component({
    selector: 'sqx-schema-fields[schema]',
    styleUrls: ['./schema-fields.component.scss'],
    templateUrl: './schema-fields.component.html',
})
export class SchemaFieldsComponent implements OnInit {
    @Input()
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
