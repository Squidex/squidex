/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { AppLanguageDto, FieldDto, FormRowComponent, LocalizedInputComponent, ReferencesFieldPropertiesDto, SchemaTagSource, TagEditorComponent, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-references-validation',
    styleUrls: ['references-validation.component.scss'],
    templateUrl: 'references-validation.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormRowComponent,
        FormsModule,
        LocalizedInputComponent,
        ReactiveFormsModule,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class ReferencesValidationComponent {
    /** The form group to edit the field properties. */
    public readonly fieldForm = input.required<UntypedFormGroup>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<ReferencesFieldPropertiesDto>();

    /** The languages of the app, used to edit localized default values. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** Shows the localized default values, if the field is localizable. */
    public readonly isLocalizable = input(false, { transform: booleanAttribute });

    constructor(
        protected readonly schemasSource: SchemaTagSource,
    ) {
    }
}
