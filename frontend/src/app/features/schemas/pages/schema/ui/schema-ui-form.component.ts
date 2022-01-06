/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges } from '@angular/core';
import { SchemaDto, SchemasState } from '@app/shared';

@Component({
    selector: 'sqx-schema-ui-form',
    styleUrls: ['./schema-ui-form.component.scss'],
    templateUrl: './schema-ui-form.component.html',
})
export class SchemaUIFormComponent implements OnChanges {
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
