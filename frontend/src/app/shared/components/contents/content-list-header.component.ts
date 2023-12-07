/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { LanguageDto, META_FIELDS, Query, SortMode, TableField } from '@app/shared/internal';
import { TableHeaderComponent } from '../table-header.component';

@Component({
    standalone: true,
    selector: 'sqx-content-list-header',
    styleUrls: ['./content-list-header.component.scss'],
    templateUrl: './content-list-header.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        TableHeaderComponent,
    ],
})
export class ContentListHeaderComponent {
    public readonly metaFields = META_FIELDS;

    @Input({ required: true })
    public field!: TableField;

    @Output()
    public queryChange = new EventEmitter<Query>();

    @Input()
    public query: Query | undefined;

    @Input({ required: true })
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
            this.sortPath = `${field.name}.${language.iso2Code}`;
        } else {
            this.sortPath = `${field.name}.iv`;
        }

        if (field === META_FIELDS.lastModified) {
            this.sortDefault = 'descending';
        }
    }
}
