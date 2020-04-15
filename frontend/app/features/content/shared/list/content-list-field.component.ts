/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { ContentDto, getContentValue, LanguageDto, MetaFields, RootFieldDto, TableField, Types } from '@app/shared';

@Component({
    selector: 'sqx-content-list-field',
    styleUrls: ['./content-list-field.component.scss'],
    templateUrl: './content-list-field.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentListFieldComponent implements OnChanges {
    @Input()
    public field: TableField;

    @Input()
    public content: ContentDto;

    @Input()
    public patchAllowed: boolean;

    @Input()
    public patchForm: FormGroup;

    @Input()
    public language: LanguageDto;

    public value: any;

    public ngOnChanges() {
        this.reset();
    }

    public reset() {
        if (Types.is(this.field, RootFieldDto)) {
            const { value, formatted } = getContentValue(this.content, this.language, this.field);

            if (this.patchForm) {
                const formControl = this.patchForm.controls[this.field.name];

                if (formControl) {
                    formControl.setValue(value);
                }
            }

            this.value = formatted;
        }
    }

    public get metaFields() {
        return MetaFields;
    }

    public get isInlineEditable() {
        return Types.is(this.field, RootFieldDto) ? this.field.isInlineEditable : false;
    }

    public get fieldName() {
        return Types.is(this.field, RootFieldDto) ? this.field.name : this.field;
    }
}