/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DateTimeEditorComponent, DropdownComponent, HighlightPipe, TranslatePipe } from '@app/framework';
import { ContributorsState, FilterableField, FilterComparison, FilterFieldUI, FilterNegation, getFilterUI, isNegation, LanguageDto, QueryModel } from '@app/shared/internal';
import { UserDtoPicture } from '../../pipes';
import { ReferenceInputComponent } from '../../references/reference-input.component';
import { QueryPathComponent } from './query-path.component';
import { FilterOperatorPipe } from './query.pipes';

@Component({
    standalone: true,
    selector: 'sqx-filter-comparison',
    styleUrls: ['./filter-comparison.component.scss'],
    templateUrl: './filter-comparison.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        DateTimeEditorComponent,
        DropdownComponent,
        FilterOperatorPipe,
        FormsModule,
        HighlightPipe,
        QueryPathComponent,
        ReferenceInputComponent,
        TranslatePipe,
        UserDtoPicture,
    ],
})
export class FilterComparisonComponent {
    @Output()
    public filterChange = new EventEmitter<FilterComparison | FilterNegation>();

    @Output()
    public remove = new EventEmitter();

    @Input({ required: true })
    public language!: LanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<LanguageDto>;

    @Input({ required: true })
    public model!: QueryModel;

    @Input({ required: true })
    public filter!: FilterComparison | FilterNegation;

    public actualComparison!: FilterComparison;
    public actualNegated = false;

    public field?: FilterableField;
    public fieldUI?: FilterFieldUI;
    public operators: ReadonlyArray<string> = [];

    constructor(
        public readonly contributorsState: ContributorsState,
    ) {
    }

    public ngOnChanges() {
        if (isNegation(this.filter)) {
            this.actualComparison = this.filter.not;
            this.actualNegated = true;
        } else {
            this.actualComparison = this.filter;
            this.actualNegated = false;
        }

        this.field = this.model.schema.fields.find(x => x.path === this.actualComparison.path);
        this.fieldUI = getFilterUI(this.actualComparison, this.field!);

        this.operators = this.model.operators[this.field?.schema.type!] || [];

        if (!this.operators.includes(this.actualComparison.op)) {
            this.actualComparison = { ...this.actualComparison, op: this.operators[0] };
        }
    }

    public changeValue(value: any) {
        this.change({ value });
    }

    public changeOp(op: string) {
        this.change({ op });
    }

    public changePath(path: string) {
        this.change({ path, value: null });
    }

    private change(update: Partial<FilterComparison>) {
        this.emitChange({ ...this.actualComparison, ...update }, this.actualNegated);
    }

    public toggleNot() {
        this.emitChange(this.actualComparison, !this.actualNegated);
    }

    private emitChange(filter: FilterComparison, not: boolean) {
        this.filterChange.emit(not ? { not: filter } : filter);
    }
}
