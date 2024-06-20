/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, EventEmitter, forwardRef, Input, numberAttribute, Output } from '@angular/core';
import { FilterComparison, FilterLogical, FilterNegation, FilterNode, isLogical, LanguageDto, QueryModel } from '@app/shared/internal';
import { FilterComparisonComponent } from './filter-comparison.component';
import { FilterLogicalComponent } from './filter-logical.component';

@Component({
    standalone: true,
    selector: 'sqx-filter-node',
    styleUrls: ['./filter-node.component.scss'],
    templateUrl: './filter-node.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        forwardRef(() => FilterComparisonComponent),
        forwardRef(() => FilterLogicalComponent),
    ],
})
export class FilterNodeComponent {
    @Output()
    public filterChange = new EventEmitter();

    @Output()
    public remove = new EventEmitter();

    @Input({ required: true })
    public language!: LanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<LanguageDto>;

    @Input({ transform: numberAttribute })
    public level = 0;

    @Input({ required: true })
    public model!: QueryModel;

    @Input()
    public set filter(value: FilterNode) {
        if (isLogical(value)) {
            this.actualLogical = value;
        } else {
            this.actualComparison = value;
        }
    }

    public actualComparison?: FilterComparison | FilterNegation;
    public actualLogical?: FilterLogical;
}
