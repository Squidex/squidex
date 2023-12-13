/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SchemaDto, SchemasState, TranslatePipe } from '@app/shared';
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

    public fieldsInLists: ReadonlyArray<string> = [];
    public fieldsInReferences: ReadonlyArray<string> = [];

    constructor(
        private readonly schemasState: SchemasState,
    ) {
    }

    public ngOnChanges() {
        this.isEditable = this.schema.canUpdate;

        this.fieldsInLists = this.schema.fieldsInLists;
        this.fieldsInReferences = this.schema.fieldsInReferences;
    }

    public setFieldsInLists(names: ReadonlyArray<string>) {
        this.fieldsInLists = names;
    }

    public setFieldsInReferences(names: ReadonlyArray<string>) {
        this.fieldsInReferences = names;
    }

    public selectTab(tab: number) {
        this.selectedTab = tab;
    }

    public saveSchema() {
        if (!this.isEditable) {
            return;
        }

        this.schemasState.configureUIFields(this.schema, {
            fieldsInLists: this.fieldsInLists,
            fieldsInReferences: this.fieldsInReferences,
        });
    }
}
