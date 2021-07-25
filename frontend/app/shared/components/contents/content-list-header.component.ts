/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { LanguageDto, MetaFields, Query, RootFieldDto, TableField, Types } from '@app/shared/internal';

@Component({
    selector: 'sqx-content-list-header[field][language]',
    styleUrls: ['./content-list-header.component.scss'],
    templateUrl: './content-list-header.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContentListHeaderComponent {
    @Input()
    public field: TableField;

    @Output()
    public queryChange = new EventEmitter<Query>();

    @Input()
    public query: Query | undefined;

    @Input()
    public language: LanguageDto;

    public get metaFields() {
        return MetaFields;
    }

    public get isSortable() {
        return Types.is(this.field, RootFieldDto) ? this.field.properties.isSortable : false;
    }

    public get fieldName() {
        return Types.is(this.field, RootFieldDto) ? this.field.name : this.field;
    }

    public get fieldDisplayName() {
        return Types.is(this.field, RootFieldDto) ? this.field.displayName : '';
    }

    public get fieldPath() {
        if (Types.isString(this.field)) {
            return this.field;
        } else if (this.field.isLocalizable && this.language) {
            return `data.${this.field.name}.${this.language.iso2Code}`;
        } else {
            return `data.${this.field.name}.iv`;
        }
    }
}
