/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, inject, input, linkedSignal, output, Type } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { BooleanValue, BootstrapClasses, EMPTY_FILTER_MODEL, FieldComponent, FilterField, Input as FilterInput, FilterModel, FilterOptions, NumberValue, SelectValue, StringValue } from 'ngx-inline-filter';
import { DateTimeEditorComponent, DropdownComponent, FocusOnInitDirective, FormErrorComponent, FormRowComponent, HighlightPipe, LocalizerService, ModalDialogComponent, ModalDirective, ShortcutComponent, TooltipDirective, TourStepDirective, TranslatePipe, Types } from '@app/framework';
import { AppLanguageDto, ContributorsState, DialogModel, Queries, Query, QueryModel, sanitize, SaveQueryForm } from '@app/shared/internal';
import { UserDtoPicture } from '../pipes';
import { ReferenceInputComponent } from '../references/reference-input.component';
import { TourHintDirective } from '../tour-hint.directive';

@Component({
    selector: 'sqx-search-form',
    styleUrls: ['./search-form.component.scss'],
    templateUrl: './search-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        DateTimeEditorComponent,
        DropdownComponent,
        FilterInput,
        FocusOnInitDirective,
        FormErrorComponent,
        FormField,
        FormRowComponent,
        FormsModule,
        HighlightPipe,
        ModalDialogComponent,
        ModalDirective,
        ReferenceInputComponent,
        ShortcutComponent,
        TooltipDirective,
        TourHintDirective,
        TourStepDirective,
        TranslatePipe,
        UserDtoPicture,
    ],
})
export class SearchFormComponent {
    private readonly localizer = inject(LocalizerService);

    protected readonly contributorsState = inject(ContributorsState);

    /** Emits the query when the user starts a search. */
    public readonly queryChange = output<Query>();

    /** The placeholder of the search input. */
    public readonly placeholder = input('');

    /** The language for reference fields. */
    public readonly language = input<AppLanguageDto>();

    /** All languages of the app, for reference fields. */
    public readonly languages = input<ReadonlyArray<AppLanguageDto>>([]);

    /** The model with the fields and operators that can be used in filters. */
    public readonly queryModel = input<QueryModel | null>();

    /** The current query. */
    public readonly query = input<Query | null>();

    /** The saved queries, used to bookmark the current query. */
    public readonly queries = input<Queries | null>();

    /** The name of the type of the queries. */
    public readonly queriesTypes = input('');

    /** Focuses the search input when CTRL + I is pressed. */
    public readonly enableShortcut = input(false, { transform: booleanAttribute });

    protected readonly cleanedQuery = linkedSignal(() => sanitize(this.query()));

    protected readonly filterModel = computed(() => {
        return this.buildFilterModel(this.queryModel());
    });

    protected readonly saveKey = computed(() => {
        const queries = this.queries();
        const query = this.query();
        if (!queries || !query) {
            return undefined;
        }

        return queries.getSaveKey(query);
    });

    protected readonly saveQueryDialog = new DialogModel();

    protected readonly saveQueryForm = new SaveQueryForm();

    protected readonly options: FilterOptions = {
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
            addComparison: this.localizer.getOrKey('search.addFilter'),
            addGroup: this.localizer.getOrKey('search.addGroup'),
            addSorting: this.localizer.getOrKey('search.addSorting'),
            apply: this.localizer.getOrKey('common.search'),
            and: 'AND',
            not: 'NOT',
            noResults: ' - ',
            or: 'OR',
            reset: this.localizer.getOrKey('common.reset'),
            save: this.localizer.getOrKey('common.save'),
            searchPlaceholder: this.localizer.getOrKey('common.search'),
            searchShortcut: 'CTRL + I',
            sortAsc: 'ascending',
            sortDesc: 'descending',
            sorting: this.localizer.getOrKey('search.sorting'),
        },
    };

    private buildFilterModel(model: QueryModel | null | undefined) {
        if (!model) {
            return EMPTY_FILTER_MODEL;
        }

        const filterModel: FilterModel = { fields: [], operators: [] };

        const uniqueOperators = new Set<string>();
        for (const operators of Object.values(model.operators)) {
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

            let operators = [...model.operators[field.schema.type]];

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
                operators = ['eq'];
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
                operators,
                path: field.path,
            };

            filterModel.fields.push(filterField);
        }

        return filterModel;
    }

    protected bookmark(value: boolean) {
        if (value) {
            this.saveQueryDialog.show();
        }
    }

    public saveQuery() {
        this.saveQueryForm.reset();
        this.saveQueryDialog.show();
    }

    protected saveQueryComplete() {
        const queries = this.queries();
        const query = this.query();

        if (!queries) {
            return;
        }

        this.saveQueryForm.submit(async value => {
            if (!query) {
                throw 'i18n:search.queryNotUsed';
            }

            if (value.user) {
                queries.addUser(value.name, query);
            } else {
                queries.addShared(value.name, query);
            }

            this.saveQueryDialog.hide();
        }, { resetAfterSubmit: true });
    }
}
