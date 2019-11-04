/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges } from '@angular/core';

import { SchemaDetailsDto } from '@app/shared';

type State = { fieldsInLists: ReadonlyArray<string>, fieldsInReferences: ReadonlyArray<string> };

@Component({
    selector: 'sqx-schema-ui-form',
    styleUrls: ['./schema-ui-form.component.scss'],
    templateUrl: './schema-ui-form.component.html'
})
export class SchemaUIFormComponent implements OnChanges {
    @Input()
    public schema: SchemaDetailsDto;

    public selectableTabs: ReadonlyArray<string> = ['List Fields', 'Reference Fields'];
    public selectedTab = this.selectableTabs[0];

    public isEditable = false;

    public state: State = {
        fieldsInLists: [],
        fieldsInReferences: []
    };

    public ngOnChanges() {
        this.isEditable = this.schema.canUpdate;

        this.state = {
            fieldsInLists: [],
            fieldsInReferences: []
        };
    }

    public setFieldsInLists(names: ReadonlyArray<string>) {
        this.state.fieldsInLists = names;
    }

    public setFieldsInReferences(names: ReadonlyArray<string>) {
        this.state.fieldsInReferences = names;
    }

    public selectTab(tab: string) {
        this.selectedTab = tab;
    }

    public saveSchema() {
        if (!this.isEditable) {
            return;
        }

        return 0;
    }
}