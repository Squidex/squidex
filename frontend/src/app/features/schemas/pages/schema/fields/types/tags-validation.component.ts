/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { AppLanguageDto, FieldDto, FormRowComponent, LocalizedInputComponent, TagEditorComponent, TagsFieldPropertiesDto, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-tags-validation',
    styleUrls: ['tags-validation.component.scss'],
    templateUrl: 'tags-validation.component.html',
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
export class TagsValidationComponent {
    /** The form group to edit the field properties. */
    public readonly fieldForm = input.required<UntypedFormGroup>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<TagsFieldPropertiesDto>();

    /** The languages of the app, used to edit localized default values. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** Shows the localized default values, if the field is localizable. */
    public readonly isLocalizable = input(false, { transform: booleanAttribute });
}
