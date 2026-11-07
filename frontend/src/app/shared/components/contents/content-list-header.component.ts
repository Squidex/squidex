/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { SortOrder } from 'ngx-inline-filter';
import { AppLanguageDto, META_FIELDS, Query, TableField } from '@app/shared/internal';
import { TableHeaderComponent } from '../table-header.component';

@Component({
    selector: 'sqx-content-list-header',
    styleUrls: ['./content-list-header.component.scss'],
    templateUrl: './content-list-header.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        TableHeaderComponent,
    ],
})
export class ContentListHeaderComponent {
    /** The field to render the header for. */
    public readonly field = input.required<TableField>();

    /** Emits the new query when the user sorts by this field. */
    public readonly queryChange = output<Query>();

    /** The current query, to show the sort order. */
    public readonly query = input<Query>();

    /** The language for localized fields, to sort by the value in this language. */
    public readonly language = input.required<AppLanguageDto>();

    protected readonly sortPath = computed(() => {
        const field = this.field();
        if (field === META_FIELDS.created) {
            return 'created';
        }

        if (field === META_FIELDS.lastModified) {
            return 'lastModified';
        }

        if (field.rootField?.properties.isSortable !== true) {
            return undefined;
        }

        const language = this.language();
        if (field.rootField.isLocalizable && language) {
            return `${field.name}.${language.iso2Code}`;
        }

        return `${field.name}.iv`;
    });

    protected readonly sortDefault = computed<SortOrder | undefined>(() => {
        return this.field() === META_FIELDS.lastModified ? 'descending' : undefined;
    });
}
