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
    FilterState,
    ModalModel,
    Queries,
    SaveQueryForm
} from '@app/shared/internal';

@Component({
    selector: 'sqx-search-form',
    styleUrls: ['./search-form.component.scss'],
    templateUrl: './search-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SearchFormComponent implements OnInit {
    @Input()
    public queries: Queries;

    @Input()
    public placeholder = '';

    @Input()
    public fieldExample = '[MY_FIELD]';

    @Input()
    public expandable = false;

    @Input()
    public filter: FilterState;

    @Input()
    public schemaName = '';

    @Input()
    public enableShortcut = false;

    @Input()
    public formClass = 'form-inline search-form';

    @Output()
    public querySubmit = new EventEmitter();

    public saveKey: Observable<string | undefined>;
    public saveQueryDialog = new DialogModel();
    public saveQueryForm = new SaveQueryForm(this.formBuilder);

    public searchModal = new ModalModel();

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
