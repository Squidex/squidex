/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { LanguageDto, MetaFields, Query, TableField } from '@app/shared/internal';

@Component({
    selector: 'sqx-content-list-header[field][language]',
    styleUrls: ['./content-list-header.component.scss'],
    templateUrl: './content-list-header.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContentListHeaderComponent {
    public readonly metaFields = MetaFields;

    @Input()
    public field!: TableField;

    @Output()
    public queryChange = new EventEmitter<Query>();

    @Input()
    public query: Query | undefined;

    @Input()
    public language!: LanguageDto;

    public get isSortable() {
        return this.field.rootField?.properties.isSortable === true;
    }

    public get fieldPath() {
        if (!this.field.rootField) {
            return this.field.name;
        } else if (this.field.rootField.isLocalizable && this.language) {
            return `data.${this.field.name}.${this.language.iso2Code}`;
        } else {
            return `data.${this.field.name}.iv`;
        }
    }
}
