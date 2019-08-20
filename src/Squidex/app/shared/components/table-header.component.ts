/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';

import {
    LanguageDto,
    Query,
    RootFieldDto,
    SortMode,
    Types
} from '@app/shared/internal';

type Field = string | RootFieldDto;

@Component({
    selector: 'sqx-table-header',
    template: `
        <a *ngIf="sortable; else notSortable" (click)="sort()" class="pointer truncate">
            <i *ngIf="order === 'ascending'" class="icon-caret-down"></i>
            <i *ngIf="order === 'descending'" class="icon-caret-up"></i>

            {{text}}
        </a>

        <ng-template #notSortable>
            {{text}}
        </ng-template>`,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class TableHeaderComponent implements OnChanges {
    private fieldPath: string;

    @Input()
    public text: string;

    @Input()
    public field: Field;

    @Input()
    public language: LanguageDto;

    @Input()
    public sortable = false;

    @Input()
    public query: Query;

    @Output()
    public queryChange = new EventEmitter<Query>();

    public order: SortMode | null;

    public ngOnChanges(changes: SimpleChanges) {
        if (this.sortable) {
            if (changes['language'] || changes['field']) {
                this.fieldPath = getFieldPath(this.language, this.field);
            }

            if (changes['query'] && this.fieldPath) {
                this.order = getSortMode(this.query, this.fieldPath);
            }
        }
    }

    public sort() {
        if (this.sortable && this.fieldPath) {
            if (!this.order || this.order !== 'ascending') {
                this.order = 'ascending';
            } else {
                this.order = 'descending';
            }

            this.queryChange.emit(this.newQuery());
        }
    }

    private newQuery() {
        return {...this.query, sort: [{ path: this.fieldPath, order: this.order! }] };
    }
}

function getSortMode(query: Query, path: string) {
    if (path && query && query.sort && query.sort.length === 1 && query.sort[0].path === path) {
        return query.sort[0].order;
    }

    return null;
}

function getFieldPath(language: LanguageDto | undefined, field: Field) {
    if (Types.isString(field)) {
        return field;
    } else if (field.isLocalizable && language) {
        return `data.${field.name}.${language.iso2Code}`;
    } else {
        return `data.${field.name}.iv`;
    }
}