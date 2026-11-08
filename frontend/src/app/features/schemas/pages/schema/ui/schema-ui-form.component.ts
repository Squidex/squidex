/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, inject, input, linkedSignal, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ConfigureUIFieldsDto, SchemaDto, SchemasState, TranslatePipe } from '@app/shared';
import { FieldListComponent } from './field-list.component';

@Component({
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
    private readonly schemasState = inject(SchemasState);

    /** The schema to configure the list and reference fields for. */
    public readonly schema = input.required<SchemaDto>();

    protected readonly selectedTab = signal(0);

    protected readonly fieldsInLists = linkedSignal<string[]>(() => {
        return this.schema().fieldsInLists;
    });

    protected readonly fieldsInReferences = linkedSignal<string[]>(() => {
        return this.schema().fieldsInReferences;
    });

    protected readonly isEditable = computed(() => {
        return this.schema().canUpdate;
    });

    protected setFieldsInLists(names: string[]) {
        this.fieldsInLists.set(names);
    }

    protected setFieldsInReferences(names: string[]) {
        this.fieldsInReferences.set(names);
    }

    protected selectTab(tab: number) {
        this.selectedTab.set(tab);
    }

    protected saveSchema() {
        if (!this.isEditable()) {
            return;
        }

        const request = new ConfigureUIFieldsDto({
            fieldsInLists: this.fieldsInLists(),
            fieldsInReferences: this.fieldsInReferences(),
        });

        this.schemasState.configureUIFields(this.schema(), request);
    }
}
