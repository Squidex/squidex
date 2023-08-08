/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, numberAttribute, Output } from '@angular/core';
import { FilterComparison, FilterLogical, FilterNode, LanguageDto, QueryModel, StatusInfo } from '@app/shared/internal';

@Component({
    selector: 'sqx-filter-node',
    styleUrls: ['./filter-node.component.scss'],
    templateUrl: './filter-node.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FilterNodeComponent {
    public comparison?: FilterComparison;

    @Output()
    public change = new EventEmitter();

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
