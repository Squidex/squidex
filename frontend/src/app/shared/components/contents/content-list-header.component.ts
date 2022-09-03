/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { LanguageDto, META_FIELDS, Query, SortMode, TableField } from '@app/shared/internal';

@Component({
    selector: 'sqx-content-list-header[field][language]',
    styleUrls: ['./content-list-header.component.scss'],
    templateUrl: './content-list-header.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContentListHeaderComponent implements OnChanges {
    public readonly metaFields = META_FIELDS;

    @Input()
    public field!: TableField;

    @Output()
    public queryChange = new EventEmitter<Query>();

    @Input()
    public query: Query | undefined;

    @Input()
    public language!: LanguageDto;

    public sortPath?: string;
    public sortDefault?: SortMode;

    public ngOnChanges() {
        const { field, language } = this;

        if (field === META_FIELDS.created) {
            this.sortPath = 'created';
        } else if (field === META_FIELDS.lastModified) {
            this.sortPath = 'lastModified';
        } else if (field.rootField?.properties.isSortable !== true) {
            this.sortPath = undefined;
        } else if (field.rootField.isLocalizable && language) {
            this.sortPath = `data.${field.name}.${language.iso2Code}`;
        } else {
            this.sortPath = `data.${field.name}.iv`;
        }

        if (field === META_FIELDS.lastModified) {
            this.sortDefault = 'descending';
        }
    }
}
