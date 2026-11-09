/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, Component, computed, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FieldTree, FormField } from '@angular/forms/signals';
import { AppLanguageDto, AssetsFieldFormModel, AssetsFieldPropertiesDto, FieldDto, FieldFormModel, FormRowComponent, LocalizedInputComponent, SignalSelectDirective, TagEditorComponent, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-assets-validation',
    styleUrls: ['assets-validation.component.scss'],
    templateUrl: 'assets-validation.component.html',
    imports: [
        FormField,
        FormRowComponent,
        FormsModule,
        LocalizedInputComponent,
        SignalSelectDirective,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class AssetsValidationComponent {
    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<AssetsFieldPropertiesDto>();

    /** The languages of the app, used to edit localized default values. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** Shows the localized default values, if the field is localizable. */
    public readonly isLocalizable = input(false, { transform: booleanAttribute });

    protected readonly form = computed(() => {
        return this.fieldForm() as FieldTree<AssetsFieldFormModel>;
    });
}
