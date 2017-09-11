/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';

@Component({
    selector: 'sqx-search-form',
    styleUrls: ['./search-form.component.scss'],
    templateUrl: './search-form.component.html'
})
export class SearchFormComponent implements OnChanges {
    private queryValue = '';

    @Input()
    public query = '';

    @Output()
    public queryChanged = new EventEmitter<string>();

    @Input()
    public archived = false;

    @Output()
    public archivedChanged = new EventEmitter<boolean>();

    @Input()
    public canArchive = true;

    public searchForm =
        this.formBuilder.group({
            odataOrderBy: '',
            odataFilter: '',
            odataSearch: ''
        });

    constructor(
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnChanges() {
        if (this.query === this.queryValue) {
            return;
        }

        let odataOrderBy = '';
        let odataFilter = '';
        let odataSearch = '';

        const parts = this.query.split('&');

        if (parts.length === 1 && parts[0][0] !== '$') {
            odataSearch = parts[0];
        } else {
            for (let part of parts) {
                const kvp = part.split('=');

                if (kvp.length === 2) {
                    const key = kvp[0].toLowerCase();

                    if (key === '$filter') {
                        odataFilter = kvp[1];
                    } else if (key === '$orderby') {
                        odataOrderBy = kvp[1];
                    } else if (key === '$search') {
                        odataSearch = kvp[1];
                    }
                }
            }
        }

        this.searchForm.setValue({
            odataFilter,
            odataSearch,
            odataOrderBy
        }, { emitEvent: false });

        this.queryValue = this.query;
    }

    public updateQuery() {
        const odataOrderBy = this.searchForm.controls['odataOrderBy'].value;
        const odataFilter = this.searchForm.controls['odataFilter'].value;
        const odataSearch = this.searchForm.controls['odataSearch'].value;

        let query = '';

        if (odataSearch && !odataOrderBy && !odataFilter) {
            query = odataSearch;
        } else {
            const parts: string[] = [];

            if (odataSearch) {
                parts.push(`$search=${odataSearch}`);
            }

            if (odataFilter) {
                parts.push(`$filter=${odataFilter}`);
            }

            if (odataOrderBy) {
                parts.push(`$orderby=${odataOrderBy}`);
            }

            query = parts.join('&');
        }

        if (query !== this.query) {
            this.queryValue = query;
            this.queryChanged.emit(query);
        }
    }
}