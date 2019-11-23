/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Observable } from 'rxjs';

import {
    DialogModel,
    hasFilter,
    LanguageDto,
    Queries,
    Query,
    QueryModel,
    SaveQueryForm
} from '@app/shared/internal';

@Component({
    selector: 'sqx-search-form',
    styleUrls: ['./search-form.component.scss'],
    templateUrl: './search-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SearchFormComponent implements OnChanges {
    public readonly standalone = { standalone: true };

    @Output()
    public queryChange = new EventEmitter<Query>();

    @Input()
    public placeholder = '';

    @Input()
    public language: LanguageDto;

    @Input()
    public queryModel: QueryModel;

    @Input()
    public query: Query;

    @Input()
    public queries: Queries;

    @Input()
    public enableShortcut = false;

    @Input()
    public formClass = 'form-inline search-form';

    public saveKey: Observable<string | undefined>;
    public saveQueryDialog = new DialogModel();
    public saveQueryForm = new SaveQueryForm(this.formBuilder);

    public searchDialog = new DialogModel();

    public hasFilter: boolean;

    constructor(
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['queryModel'] && !changes['query']) {
            this.query = {};
        }

        if (changes['query'] || changes['queries']) {
            this.updateSaveKey();
        }

        if (changes['query']) {
            this.hasFilter = hasFilter(this.query);
        }
    }

    public search(close = false) {
        this.hasFilter = hasFilter(this.query);

        this.queryChange.emit(this.query);

        if (close) {
            this.searchDialog.hide();
        }
    }

    public saveQuery() {
        this.saveQueryForm.submitCompleted({});
        this.saveQueryDialog.show();
    }

    public saveQueryComplete() {
        const value = this.saveQueryForm.submit();

        if (value) {
            if (this.queries && this.query) {
                this.queries.add(value.name, this.query, value.user);
            }

            this.saveQueryForm.submitCompleted();
        }

        this.saveQueryDialog.hide();
    }

    public changeQueryFullText(fullText: string) {
        this.query = { ...this.query, fullText };

        this.updateSaveKey();
    }

    public changeQuery(query: Query) {
        this.query = query;

        this.updateSaveKey();
    }

    private updateSaveKey() {
        if (this.queries && this.query) {
            this.saveKey = this.queries.getSaveKey(this.query);
        }
    }
}
