/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, computed, effect, input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { FromNowPipe, ShortDatePipe, TooltipDirective, TranslatePipe } from '@app/framework';
import { AppLanguageDto, ContentDto, FieldValue, getContentValue, META_FIELDS, SchemaDto, TableField, TableSettings } from '@app/shared/internal';
import { UserNameRefPipe, UserPictureRefPipe } from '../pipes';
import { ContentStatusComponent } from './content-status.component';
import { ContentValueEditorComponent } from './content-value-editor.component';
import { ContentValueComponent } from './content-value.component';
import { TranslationStatusComponent } from './translation-status.component';

@Component({
    selector: 'sqx-content-list-field',
    styleUrls: ['./content-list-field.component.scss'],
    templateUrl: './content-list-field.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ContentStatusComponent,
        ContentValueComponent,
        ContentValueEditorComponent,
        FromNowPipe,
        ShortDatePipe,
        TooltipDirective,
        TranslatePipe,
        TranslationStatusComponent,
        UserNameRefPipe,
        UserPictureRefPipe,
    ],
})
export class ContentListFieldComponent {
    protected readonly metaFields = META_FIELDS;

    /** The field or meta field to render. */
    public readonly field = input.required<TableField>();

    /** The table settings, used to render nested values. */
    public readonly fields = input<TableSettings>();

    /** The content to render the value for. */
    public readonly content = input.required<ContentDto>();

    /** Renders an inline editor if the field supports it. */
    public readonly patchAllowed = input(false, { transform: booleanAttribute });

    /** The form for inline editing. The value of the field is written to this form. */
    public readonly patchForm = input<UntypedFormGroup | null>();

    /** The schema of the content. */
    public readonly schema = input.required<SchemaDto | undefined>();

    /** The language to render the value for. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    protected readonly isInlineEditable = computed(() => {
        return this.field().rootField?.isInlineEditable === true;
    });

    protected readonly formatted = computed<FieldValue>(() => {
        const rootField = this.field().rootField;
        if (!rootField) {
            return '';
        }

        return getContentValue(this.content(), this.language(), rootField).formatted;
    });

    constructor() {
        effect(() => {
            this.reset();
        });
    }

    public reset() {
        const rootField = this.field().rootField;
        if (!rootField) {
            return;
        }

        const formControl = this.patchForm()?.controls[rootField.name];
        if (!formControl) {
            return;
        }

        const { value } = getContentValue(this.content(), this.language(), rootField);

        formControl.setValue(value);
    }
}
