/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { FormBuilder, FormControl } from '@angular/forms';

import { ModalView } from '@app/shared';

@Component({
    selector: 'sqx-search-form',
    styleUrls: ['./search-form.component.scss'],
    templateUrl: './search-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SearchFormComponent implements OnChanges {
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

    @Input()
    public enableShortcut = false;

    @Input()
    public formClass = 'form-inline search-form';

    public contentsFilter = new FormControl();

    public searchModal = new ModalView();
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

    public search() {
        this.invalidate(this.contentsFilter.value);

        this.queryChanged.emit(this.contentsFilter.value);
    }

    public ngOnChanges() {
        this.invalidate(this.query);
    }

    private invalidate(query: string) {
        if (query === this.contentsFilter.value) {
            return;
        }

        let odataOrderBy = '';
        let odataFilter = '';
        let odataSearch = '';

        if (this.query) {
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
        }

        this.searchForm.setValue({
            odataFilter,
            odataSearch,
            odataOrderBy
        }, { emitEvent: false });

        this.contentsFilter.setValue(this.query);
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
            this.queryChanged.emit(query);
        }

        this.contentsFilter.setValue(query);
    }
}