/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output, Type } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BooleanValue, BootstrapClasses, EMPTY_FILTER_MODEL, FieldComponent, FilterField, Input as FilterInput, FilterModel, FilterOptions, NumberValue, SelectValue, StringValue } from 'ngx-inline-filter';
import { Observable } from 'rxjs';
import { ControlErrorsComponent, DateTimeEditorComponent, DropdownComponent, FocusOnInitDirective, HighlightPipe, LocalizerService, ModalDialogComponent, ModalDirective, ShortcutComponent, TooltipDirective, TourStepDirective, TranslatePipe, Types } from '@app/framework';
import { AppLanguageDto, ContributorsState, DialogModel, Queries, Query, QueryModel, sanitize, SaveQueryForm, TypedSimpleChanges } from '@app/shared/internal';
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
        DateTimeEditorComponent,
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
    public queriesTypes = '';

    @Input({ transform: booleanAttribute })
    public enableShortcut?: boolean | null;

    public filterModel = EMPTY_FILTER_MODEL;

    public saveKey!: Observable<string | undefined>;
    public saveQueryDialog = new DialogModel();
    public saveQueryForm = new SaveQueryForm();

    public cleanedQuery: Query = sanitize();

    public readonly options: FilterOptions;

    constructor(
        public readonly contributorsState: ContributorsState,
        private readonly localizer: LocalizerService,
    ) {
        this.options = {
            cssClasses: {
                ...BootstrapClasses,
                buttonAdd: 'btn btn-success',
                buttonAddOutline: 'btn btn-outline-success btn-sm',
                buttonDefault: (active: boolean) => `btn ${active ? 'fw-bold' : ''}`,
                buttonLogical: active => `btn btn-sm btn-secondary btn-toggle ${active ? 'btn-primary' : ''}`,
                dropdown: 'bg-white',
                dropdownItem: active => `control-dropdown-item control-dropdown-item-selectable separated ${active ? 'active' : ''}`,
                dropdownSearch: 'p-2',
            },
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
                sortAsc: 'ascending',
                sortDesc: 'descending',
                sorting: localizer.getOrKey('search.sorting'),
            },
        };
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.query) {
            this.cleanedQuery = sanitize(this.query);
        }

        if (changes.query || changes.queries) {
            this.updateSaveKey();
        }

        if (changes.queryModel) {
            this.updateFilterModel();
        }
    }

    private updateFilterModel() {
        const model = this.queryModel;
        if (!model) {
            this.filterModel = EMPTY_FILTER_MODEL;
            return;
        }

        const filterModel: FilterModel = { fields: [], operators: [] };

        const uniqueOperators = new Set<string>();
        for (var operators of Object.values(model.operators)) {
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

        for (const field of model.schema.fields) {
            let args;
            let component: Type<FieldComponent<any>> | undefined = undefined;

            const { type, extra } = field.schema;
            if (field.schema.type === 'Boolean') {
                component = BooleanValue;
            } else if (type === 'DateTime' && extra?.editor === 'Date') {
                args = { editor: 'Date' };
            } else if (type === 'DateTime') {
                args = { editor: 'DateTime' };
            } else if (type === 'Number') {
                component = NumberValue;
            } else if (type === 'String' && extra?.editor === 'Status') {
                args = { editor: 'Status', statuses: model.statuses };
            } else if (type === 'String' && extra?.editor === 'User') {
                args = { editor: 'User' };
            } else if (type === 'String' && Types.isArrayOfString(extra?.options)) {
                args = (extra.options as string[]).map(value => ({ value, label: value }));
                component = SelectValue;
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
                operators: model.operators[field.schema.type] as any,
                path: field.path,
            };

            filterModel.fields.push(filterField);
        }

        this.filterModel = filterModel;
    }

    public bookmark(value: boolean) {
        if (value) {
            this.saveQueryDialog.show();
        }
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
