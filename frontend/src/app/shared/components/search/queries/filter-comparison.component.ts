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
import { ContributorsState, FilterableField, FilterComparison, FilterFieldUI, getFilterUI, LanguageDto, QueryModel, StatusInfo } from '@app/shared/internal';
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
    public filterChange = new EventEmitter();

    @Output()
    public remove = new EventEmitter();

    @Input({ required: true })
    public language!: LanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<LanguageDto>;

    @Input({ required: true })
    public statuses?: ReadonlyArray<StatusInfo> | null;

    @Input({ required: true })
    public model!: QueryModel;

    @Input({ required: true })
    public filter!: FilterComparison;

    public field?: FilterableField;
    public fieldUI?: FilterFieldUI;
    public operators: ReadonlyArray<string> = [];

    constructor(
        public readonly contributorsState: ContributorsState,
    ) {
    }

    public ngOnChanges() {
        this.updatePath(false);
    }

    public changeValue(value: any) {
        this.filter.value = value;

        this.emitChange();
    }

    public changeOp(op: string) {
        this.filter.op = op;

        this.updatePath(false);

        this.emitChange();
    }

    public changePath(path: string) {
        this.filter.path = path;

        this.updatePath(true);

        this.emitChange();
    }

    private updatePath(updateValue: boolean) {
        this.field = this.model.schema.fields.find(x => x.path === this.filter.path);

        this.operators = this.model.operators[this.field?.schema.type!] || [];

        if (!this.operators.includes(this.filter.op)) {
            this.filter.op = this.operators[0];
        }

        if (updateValue) {
            this.filter.value = null;
        }

        this.fieldUI = getFilterUI(this.filter, this.field!);
    }

    public emitChange() {
        this.filterChange.emit();
    }
}
