/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output, Type } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BooleanValue, BootstrapClasses, FieldComponent, FilterField, Input as FilterInput, FilterModel, FilterOptions, NumberValue, StringValue } from 'ngx-inline-filter';
import { Observable } from 'rxjs';
import { ControlErrorsComponent, DropdownComponent, FocusOnInitDirective, HighlightPipe, LocalizerService, ModalDialogComponent, ModalDirective, ShortcutComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/framework';
import { AppLanguageDto, ContributorsState, DialogModel, Queries, Query, QueryModel, SaveQueryForm, StatusInfo, TypedSimpleChanges } from '@app/shared/internal';
import { UserDtoPicture } from '../pipes';
import { ReferenceInputComponent } from '../references/reference-input.component';
import { TourHintDirective } from '../tour-hint.directive';

@Component({
    selector: 'sqx-search-form',
    styleUrls: ['./search-form.component.scss'],
    templateUrl: './search-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        ControlErrorsComponent,
        DropdownComponent,
        FilterInput,
        FocusOnInitDirective,
        FormsModule,
        HighlightPipe,
        ModalDialogComponent,
        ModalDirective,
        ReferenceInputComponent,
        ReactiveFormsModule,
        ShortcutComponent,
        TooltipDirective,
        TourHintDirective,
        TourStepDirective,
        TranslatePipe,
        UserDtoPicture,
    ],
})
export class SearchFormComponent {
    @Output()
    public queryChange = new EventEmitter<Query>();

    @Input()
    public placeholder = '';

    @Input()
    public language!: AppLanguageDto;

    @Input()
    public languages: ReadonlyArray<AppLanguageDto> = [];

    @Input()
    public queryModel?: QueryModel | null;

    @Input()
    public query?: Query | null;

    @Input()
    public queries?: Queries | null;

    @Input()
    public statuses: ReadonlyArray<StatusInfo> = [];

    @Input()
    public queriesTypes = '';

    @Input({ transform: booleanAttribute })
    public enableShortcut?: boolean | null;

    @Input()
    public model?: QueryModel;

    public filterModel!: FilterModel;

    public saveKey!: Observable<string | undefined>;
    public saveQueryDialog = new DialogModel();
    public saveQueryForm = new SaveQueryForm();

    public readonly options: FilterOptions;

    constructor(
        public readonly contributorsState: ContributorsState,
        private readonly localizer: LocalizerService,
    ) {
        this.options = {
            cssClasses: BootstrapClasses,
            texts: {
                addComparison: localizer.getOrKey('search.addFilter'),
                addGroup: localizer.getOrKey('search.addGroup'),
                addSorting: localizer.getOrKey('search.addSorting'),
                and: 'AND',
                not: 'NOT',
                noResults: ' - ',
                or: 'OR',
                save: localizer.getOrKey('common.save'),
                searchPlaceholder: localizer.getOrKey('common.search'),
                searchShortcut: 'CTRL + I',
                sortAsc: 'asc',
                sortDesc: 'desc',
                sorting: localizer.getOrKey('search.sorting'),
            },
        };
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.query || changes.queries) {
            this.updateSaveKey();
        }

        if (changes.model) {
            this.updateFilterModel();
        }
    }

    private updateFilterModel() {
        const filterModel: FilterModel = { fields: [], operators: [] };
        if (!this.model) {
            return filterModel;
        }

        const uniqueOperators = new Set<string>();
        for (var operators of Object.values(this.model.operators)) {
            for (const operator of operators) {
                uniqueOperators.add(operator);
            }
        }

        for (const operator of uniqueOperators) {
            const isEmpty =
                operator === 'empty' ||
                operator === 'exists';

            const label = this.localizer.get(`common.queryOperators.${operator}`) || operator;

            filterModel.operators.push({ value: operator, label, isEmpty });
        }

        for (const field of this.model.schema.fields) {
            let args;
            let component: Type<FieldComponent<any>> | undefined = undefined;

            const { type, extra } = field.schema;
            if (field.schema.type === 'Boolean') {
                component = BooleanValue;
            } else if (type === 'DateTime' && extra?.editor === 'Date') {
                args = { editor: 'Date' };
            } else if (type === 'DateTime') {
                args = { editor: 'Date' };
            } else if (type === 'Number') {
                component = NumberValue;
            } else if (type === 'String' && extra?.editor === 'Status') {
                args = { editor: 'Status' };
            } else if (type === 'String' && extra?.editor === 'User') {
                args = { editor: 'User' };
            } else if (type === 'String' && !extra) {
                component = StringValue;
            } else if (type === 'StringArray' && extra?.schemaIds) {
                args = { editor: 'Reference', schemaIds: extra.schemaIds };
            } else if (type === 'StringArray') {
                component = StringValue;
            } else {
                continue;
            }

            const filterField: FilterField = {
                args,
                component,
                defaultValue: null,
                description: field.description,
                label: field.path,
                operators: this.model.operators[field.schema.type] as any,
                path: field.path,
            };

            filterModel.fields.push(filterField);
        }

        this.filterModel = filterModel;
    }

    public saveQuery() {
        this.saveQueryForm.submitCompleted({});
        this.saveQueryDialog.show();
    }

    public saveQueryComplete() {
        const value = this.saveQueryForm.submit();
        if (!value) {
            return;
        }

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

    private updateSaveKey() {
        if (this.queries && this.query) {
            this.saveKey = this.queries.getSaveKey(this.query);
        }
    }
}
