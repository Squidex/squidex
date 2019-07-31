/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Observable } from 'rxjs';

import {
    DialogModel,
    fadeAnimation,
    FilterState,
    Queries,
    SaveQueryForm
} from '@app/shared/internal';

import { QueryModel } from './queries/model';

@Component({
    selector: 'sqx-search-form',
    styleUrls: ['./search-form.component.scss'],
    templateUrl: './search-form.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SearchFormComponent implements OnInit {
    @Input()
    public placeholder = '';

    @Input()
    public filter: FilterState;

    @Input()
    public queryModel: QueryModel;

    @Input()
    public queries: Queries;

    @Output()
    public querySubmit = new EventEmitter();

    @Input()
    public enableShortcut = false;

    @Input()
    public formClass = 'form-inline search-form';

    public saveKey: Observable<string | undefined>;
    public saveQueryDialog = new DialogModel();
    public saveQueryForm = new SaveQueryForm(this.formBuilder);

    public searchDialog = new DialogModel();

    constructor(
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        if (this.queries) {
            this.saveKey = this.queries.getSaveKey(this.filter.query);
        }
    }

    public saveQuery() {
        this.saveQueryForm.submitCompleted({});
        this.saveQueryDialog.show();
    }

    public saveQueryComplete() {
        const value = this.saveQueryForm.submit();

        if (value) {
            if (this.queries) {
                this.queries.add(value.name, this.filter.apiFilter!);
            }

            this.saveQueryForm.submitCompleted();
        }

        this.saveQueryDialog.hide();
    }

    public search() {
        this.querySubmit.emit();
    }
}
