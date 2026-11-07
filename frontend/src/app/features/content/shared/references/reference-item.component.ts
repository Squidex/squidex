/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */


import { booleanAttribute, ChangeDetectionStrategy, Component, computed, input, numberAttribute, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AppLanguageDto, ConfirmClickDirective, ContentDto, ContentListCellDirective, ContentListFieldComponent, ContentValueComponent, getContentValue, META_FIELDS, TooltipDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: '[sqxReferenceItem][language][languages]',
    styleUrls: ['./reference-item.component.scss'],
    templateUrl: './reference-item.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ConfirmClickDirective,
        ContentListCellDirective,
        ContentListFieldComponent,
        ContentValueComponent,
        RouterLink,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class ReferenceItemComponent {
    /** Emits when the user removes the reference. */
    public readonly delete = output();

    /** Emits when the user clones the referenced content. */
    public readonly clone = output();

    /** The language to render the content values in. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** Shows the button to remove the reference. */
    public readonly canRemove = input(true, { transform: booleanAttribute });

    /** Hides the schema name. */
    public readonly isCompact = input(false, { transform: booleanAttribute });

    /** Disables the reference. */
    public readonly isDisabled = input(false, { transform: booleanAttribute });

    /** The validation results of the referenced contents, by content ID. */
    public readonly validations = input<{ [id: string]: boolean }>();

    /** Shows the column with the validation result. */
    public readonly validityVisible = input(false, { transform: booleanAttribute });

    /** The number of reference fields to render as columns. */
    public readonly columns = input(0, { transform: numberAttribute });

    /** The referenced content. */
    public readonly content = input.required<ContentDto>({ alias: 'sqxReferenceItem' });

    protected readonly values = computed(() => {
        const content = this.content();
        const language = this.language();
        const values: any[] = [];

        for (let i = 0; i < this.columns(); i++) {
            const field = content.referenceFields[i];

            if (field) {
                const { formatted } = getContentValue(content, language, field);

                values.push(formatted);
            } else {
                values.push('');
            }
        }

        return values;
    });

    protected readonly isValid = computed(() => {
        const validations = this.validations();

        return !validations ? undefined : validations[this.content().id];
    });

    protected readonly metaFields = META_FIELDS;
}
