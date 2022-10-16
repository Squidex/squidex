/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, OnChanges } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { ContentDto, FieldValue, getContentValue, LanguageDto, META_FIELDS, SchemaDto, StatefulComponent, TableField, TableSettings } from '@app/shared/internal';

interface State {
    // The formatted value.
    formatted: FieldValue;
}

@Component({
    selector: 'sqx-content-list-field[content][field][language][languages][schema]',
    styleUrls: ['./content-list-field.component.scss'],
    templateUrl: './content-list-field.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContentListFieldComponent extends StatefulComponent<State> implements OnChanges {
    public readonly metaFields = META_FIELDS;

    @Input()
    public field!: TableField;

    @Input()
    public fields?: TableSettings;

    @Input()
    public content!: ContentDto;

    @Input()
    public patchAllowed?: boolean | null;

    @Input()
    public patchForm?: UntypedFormGroup | null;

    @Input()
    public schema?: SchemaDto;

    @Input()
    public language!: LanguageDto;

    @Input()
    public languages!: ReadonlyArray<LanguageDto>;

    public get isInlineEditable() {
        return this.field.rootField?.isInlineEditable === true;
    }

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, {
            formatted: '',
        });
    }

    public ngOnChanges() {
        this.reset();
    }

    public reset() {
        if (this.field.rootField) {
            const { value, formatted } = getContentValue(this.content, this.language, this.field.rootField);

            if (this.patchForm) {
                const formControl = this.patchForm.controls[this.field.name];

                if (formControl) {
                    formControl.setValue(value);
                }
            }

            this.next({ formatted });
        }
    }
}
