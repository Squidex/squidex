/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Observable } from 'rxjs';

import {
    DialogModel,
    fadeAnimation,
    Queries,
    Query,
    QueryModel,
    QueryState,
    ResourceOwner,
    SaveQueryForm
} from '@app/shared/internal';
import { hasFilter } from '../state/query';

@Component({
    selector: 'sqx-search-form',
    styleUrls: ['./search-form.component.scss'],
    templateUrl: './search-form.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SearchFormComponent extends ResourceOwner implements OnChanges, OnInit {
    public readonly standalone = { standalone: true };

    @Input()
    public placeholder = '';

    @Input()
    public queryModel: QueryModel;

    @Input()
    public query: QueryState;

    @Input()
    public queries: Queries;

    @Output()
    public querySubmit = new EventEmitter();

    @Input()
    public enableShortcut = false;

    @Input()
    public formClass = 'form-inline search-form';

    public currentQuery: Query | undefined;

    public saveKey: Observable<string | undefined>;
    public saveQueryDialog = new DialogModel();
    public saveQueryForm = new SaveQueryForm(this.formBuilder);

    public searchDialog = new DialogModel();

    public hasFilter: boolean;

    constructor(
        private readonly formBuilder: FormBuilder
    ) {
        super();
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['query']) {
            super.unsubscribeAll();

            this.own(this.query.query.subscribe(query => {
                this.hasFilter = hasFilter(query);

                this.currentQuery = query;
            }));
        }
    }

    public ngOnInit() {
        if (this.queries) {
            this.saveKey = this.queries.getSaveKey(this.query.queryJson);
        }
    }

    public search() {
        this.query.setQuery(this.currentQuery);
        this.querySubmit.emit();
    }

    public saveQuery() {
        this.saveQueryForm.submitCompleted({});
        this.saveQueryDialog.show();
    }

    public saveQueryComplete() {
        const value = this.saveQueryForm.submit();

        if (value) {
            if (this.queries && this.currentQuery) {
                this.queries.add(value.name, this.currentQuery);
            }

            this.saveQueryForm.submitCompleted();
        }

        this.saveQueryDialog.hide();
    }

    public changeQuery(query: Query) {
        this.currentQuery = query;
    }
}
