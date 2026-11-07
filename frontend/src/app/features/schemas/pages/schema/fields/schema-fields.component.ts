/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, forwardRef, inject, input } from '@angular/core';
import { AppLanguageDto, AppsState, DialogModel, FieldDto, ModalDirective, SchemaDto, SchemasState, TourStepDirective, TranslatePipe } from '@app/shared';
import { FieldWizardComponent } from './field-wizard.component';
import { SortableFieldListComponent } from './sortable-field-list.component';

@Component({
    selector: 'sqx-schema-fields',
    styleUrls: ['./schema-fields.component.scss'],
    templateUrl: './schema-fields.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FieldWizardComponent,
        ModalDirective,
        TourStepDirective,
        TranslatePipe,
        forwardRef(() => SortableFieldListComponent),
    ],
})
export class SchemaFieldsComponent {
    private readonly schemasState = inject(SchemasState);

    protected readonly appsState = inject(AppsState);

    /** The schema to render and edit the fields for. */
    public readonly schema = input.required<SchemaDto>();

    /** The languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    protected readonly fieldWizard = new DialogModel();

    protected sortFields(fields: ReadonlyArray<FieldDto>) {
        this.schemasState.orderFields(this.schema(), fields).subscribe();
    }
}
