/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, forwardRef, Input, numberAttribute, Output } from '@angular/core';
import { FilterComparison, FilterLogical, FilterNode, LanguageDto, QueryModel, StatusInfo } from '@app/shared/internal';
import { FilterComparisonComponent } from './filter-comparison.component';
import { FilterLogicalComponent } from './filter-logical.component';

@Component({
    standalone: true,
    selector: 'sqx-filter-node',
    styleUrls: ['./filter-node.component.scss'],
    templateUrl: './filter-node.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        NgIf,
        forwardRef(() => FilterComparisonComponent),
        forwardRef(() => FilterLogicalComponent),
    ],
})
export class FilterNodeComponent {
    public comparison?: FilterComparison;

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

    @Input({ transform: numberAttribute })
    public level = 0;

    @Input({ required: true })
    public model!: QueryModel;

    @Input()
    public set filter(value: FilterNode) {
        if ((value as any)['and'] || (value as any)['or']) {
            this.logical = <FilterLogical>value;
        } else {
            this.comparison = <FilterComparison>value;
        }
    }

    public logical?: FilterLogical;
}
