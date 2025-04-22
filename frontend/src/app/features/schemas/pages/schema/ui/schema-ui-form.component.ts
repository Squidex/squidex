/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ConfigureUIFieldsDto, SchemaDto, SchemasState, TranslatePipe } from '@app/shared';
import { FieldListComponent } from './field-list.component';

@Component({
    standalone: true,
    selector: 'sqx-schema-ui-form',
    styleUrls: ['./schema-ui-form.component.scss'],
    templateUrl: './schema-ui-form.component.html',
    imports: [
        FieldListComponent,
        FormsModule,
        TranslatePipe,
    ],
})
export class SchemaUIFormComponent {
    @Input()
    public schema!: SchemaDto;

    public selectedTab = 0;

    public isEditable = false;

    public fieldsInLists: string[] = [];
    public fieldsInReferences: string[] = [];

    constructor(
        private readonly schemasState: SchemasState,
    ) {
    }

    public ngOnChanges() {
        this.isEditable = this.schema.canUpdate;

        this.fieldsInLists = this.schema.fieldsInLists;
        this.fieldsInReferences = this.schema.fieldsInReferences;
    }

    public setFieldsInLists(names: string[]) {
        this.fieldsInLists = names;
    }

    public setFieldsInReferences(names: string[]) {
        this.fieldsInReferences = names;
    }

    public selectTab(tab: number) {
        this.selectedTab = tab;
    }

    public saveSchema() {
        if (!this.isEditable) {
            return;
        }

        const request = new ConfigureUIFieldsDto({
            fieldsInLists: this.fieldsInLists,
            fieldsInReferences: this.fieldsInReferences,
        });

        this.schemasState.configureUIFields(this.schema, request);
    }
}
