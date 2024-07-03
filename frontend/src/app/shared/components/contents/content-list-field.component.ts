/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { FromNowPipe, ShortDatePipe, TooltipDirective, TranslatePipe } from '@app/framework';
import { ContentDto, FieldValue, getContentValue, LanguageDto, META_FIELDS, SchemaDto, StatefulComponent, TableField, TableSettings } from '@app/shared/internal';
import { UserNameRefPipe, UserPictureRefPipe } from '../pipes';
import { ContentStatusComponent } from './content-status.component';
import { ContentValueEditorComponent } from './content-value-editor.component';
import { ContentValueComponent } from './content-value.component';
import { TranslationStatusComponent } from './translation-status.component';

interface State {
    // The formatted value.
    formatted: FieldValue;
}

@Component({
    standalone: true,
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
export class ContentListFieldComponent extends StatefulComponent<State> {
    public readonly metaFields = META_FIELDS;

    @Input({ required: true })
    public field!: TableField;

    @Input()
    public fields?: TableSettings;

    @Input({ required: true })
    public content!: ContentDto;

    @Input({ transform: booleanAttribute })
    public patchAllowed?: boolean | null;

    @Input()
    public patchForm?: UntypedFormGroup | null;

    @Input({ required: true })
    public schema?: SchemaDto;

    @Input({ required: true })
    public language!: LanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<LanguageDto>;

    public get isInlineEditable() {
        return this.field.rootField?.isInlineEditable === true;
    }

    constructor() {
        super({ formatted: '' });
    }

    public ngOnChanges() {
        this.reset();
    }

    public reset() {
        if (this.field.rootField) {
            const { value, formatted } = getContentValue(this.content, this.language, this.field.rootField);

            if (this.patchForm) {
                const formControl = this.patchForm.controls[this.field.rootField.name];

                if (formControl) {
                    formControl.setValue(value);
                }
            }

            this.next({ formatted });
        }
    }
}
