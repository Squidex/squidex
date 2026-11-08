/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Component, computed, effect, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FieldTree, FormField } from '@angular/forms/signals';
import { AppLanguageDto, AppSettingsDto, DropdownMenuComponent, FieldDto, FieldFormModel, FormRowComponent, LocalizedInputComponent, ModalDirective, ModalModel, ModalPlacementDirective, PatternDto, SchemaDto, SignalSelectDirective, StringContentTypeValues, StringFieldFormModel, StringFieldPropertiesDto, TranslatePipe, Types } from '@app/shared';

@Component({
    selector: 'sqx-string-validation',
    styleUrls: ['string-validation.component.scss'],
    templateUrl: 'string-validation.component.html',
    imports: [
        DropdownMenuComponent,
        FormField,
        FormRowComponent,
        FormsModule,
        LocalizedInputComponent,
        ModalDirective,
        ModalPlacementDirective,
        SignalSelectDirective,
        TranslatePipe,
    ],
})
export class StringValidationComponent {
    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The schema the field belongs to. */
    public readonly schema = input.required<SchemaDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<StringFieldPropertiesDto>();

    /** The settings of the app, which provide the predefined patterns. */
    public readonly settings = input.required<AppSettingsDto>();

    /** The languages of the app, used to edit localized default values. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** Shows the localized default values, if the field is localizable. */
    public readonly isLocalizable = input(false, { transform: booleanAttribute });

    protected readonly form = computed(() => {
        return this.fieldForm() as FieldTree<StringFieldFormModel>;
    });

    protected readonly showPatternMessage = computed(() => {
        return !!this.form().pattern().value();
    });

    protected readonly showPatternSuggestions = computed(() => {
        return !this.form().pattern().value();
    });

    protected readonly patternName = computed(() => {
        const regex = this.form().pattern().value();
        if (!regex) {
            return '';
        }

        const matchingPattern = this.settings().patterns.find(x => x.regex === regex);
        if (matchingPattern) {
            return matchingPattern.name;
        }

        return 'Advanced';
    });

    protected readonly showUnique = computed(() => {
        const field = this.field();

        return Types.is(field, FieldDto) && !field.isLocalizable && this.schema().type !== 'Component';
    });

    protected readonly contentTypes = StringContentTypeValues;

    protected readonly patternsModal = new ModalModel();

    constructor() {
        effect(() => {
            const form = this.form();
            if (!form.pattern().value()) {
                form.patternMessage().value.set('');
            }
        });
    }

    protected setPattern(pattern: PatternDto) {
        const form = this.form();

        form.pattern().value.set(pattern.regex);
        form.patternMessage().value.set(pattern.message ?? '');
    }
}
