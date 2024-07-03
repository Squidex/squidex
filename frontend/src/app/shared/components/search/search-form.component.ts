/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { ControlErrorsComponent, FocusOnInitDirective, MarkdownPipe, ModalDialogComponent, ModalDirective, SafeHtmlPipe, ShortcutComponent, ShortcutDirective, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/framework';
import { DialogModel, equalsQuery, hasFilter, LanguageDto, Queries, Query, QueryModel, SaveQueryForm, TypedSimpleChanges } from '@app/shared/internal';
import { TourHintDirective } from '../tour-hint.directive';
import { QueryComponent } from './queries/query.component';
import { SavedQueriesComponent } from './shared-queries.component';

@Component({
    standalone: true,
    selector: 'sqx-search-form',
    styleUrls: ['./search-form.component.scss'],
    templateUrl: './search-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        ControlErrorsComponent,
        FocusOnInitDirective,
        FormsModule,
        MarkdownPipe,
        ModalDialogComponent,
        ModalDirective,
        QueryComponent,
        ReactiveFormsModule,
        SafeHtmlPipe,
        SavedQueriesComponent,
        ShortcutComponent,
        ShortcutDirective,
        TooltipDirective,
        TourHintDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class SearchFormComponent {
    private previousQuery?: Query | null;

    @Output()
    public queryChange = new EventEmitter<Query>();

    @Input()
    public placeholder = '';

    @Input()
    public language!: LanguageDto;

    @Input()
    public languages: ReadonlyArray<LanguageDto> = [];

    @Input()
    public queryModel?: QueryModel | null;

    @Input()
    public query?: Query | null;

    @Input()
    public queries?: Queries | null;

    @Input()
    public queriesTypes = '';

    @Input({ transform: booleanAttribute })
    public enableShortcut?: boolean | null;

    @Input()
    public formClass = 'form-inline search-form';

    public showQueries = true;

    public saveKey!: Observable<string | undefined>;
    public saveQueryDialog = new DialogModel();
    public saveQueryForm = new SaveQueryForm();

    public searchDialog = new DialogModel(false);

    public hasFilter = false;

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.query || changes.queries) {
            this.updateSaveKey();
        }

        if (changes.query) {
            this.previousQuery = this.query;
        }

        this.hasFilter = hasFilter(this.query);
    }

    public search(close = false) {
        this.hasFilter = hasFilter(this.query);

        if (this.query && !equalsQuery(this.query, this.previousQuery)) {
            this.queryChange.emit(this.query);

            this.previousQuery = this.query;
        }

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
                if (value.user) {
                    this.queries.addUser(value.name, this.query);
                } else {
                    this.queries.addShared(value.name, this.query);
                }
            }

            this.saveQueryForm.submitCompleted();
            this.saveQueryDialog.hide();
        }
    }

    public changeQueryFullText(fullText: string) {
        this.query = { ...this.query, fullText };

        this.updateSaveKey();
    }

    public changeQuery(query: Query) {
        this.query = query;

        this.updateSaveKey();
    }

    public changeView(value: boolean) {
        this.showQueries = value;
    }

    private updateSaveKey() {
        if (this.queries && this.query) {
            this.saveKey = this.queries.getSaveKey(this.query);
        }
    }
}
