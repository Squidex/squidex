/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, forwardRef, input } from '@angular/core';
import { AppLanguageDto, AppsState, DialogModel, FieldDto, ModalDirective, SchemaDto, SchemasState, TourStepDirective, TranslatePipe } from '@app/shared';
import { FieldWizardComponent } from './field-wizard.component';
import { SortableFieldListComponent } from './sortable-field-list.component';

@Component({
    selector: 'sqx-schema-fields',
    styleUrls: ['./schema-fields.component.scss'],
    templateUrl: './schema-fields.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
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
    /** The schema to render and edit the fields for. */
    public readonly schema = input.required<SchemaDto>();

    /** The languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    public readonly fieldWizard = new DialogModel();

    constructor(
        public readonly appsState: AppsState,
        public readonly schemasState: SchemasState,
    ) {
    }

    public sortFields(fields: ReadonlyArray<FieldDto>) {
        this.schemasState.orderFields(this.schema(), fields).subscribe();
    }
}
