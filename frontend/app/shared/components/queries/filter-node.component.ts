/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import {
    FilterComparison,
    FilterLogical,
    FilterNode,
    LanguageDto,
    QueryModel
} from '@app/shared/internal';

@Component({
    selector: 'sqx-filter-node',
    styleUrls: ['./filter-node.component.scss'],
    templateUrl: './filter-node.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FilterNodeComponent {
    public comparison: FilterComparison;

    @Output()
    public change = new EventEmitter();

    @Output()
    public remove = new EventEmitter();

    @Input()
    public language: LanguageDto;

    @Input()
    public level: number;

    @Input()
    public model: QueryModel;

    @Input()
    public set filter(value: FilterNode) {
        if (value['and'] || value['or']) {
            this.logical = <FilterLogical>value;
        } else {
            this.comparison = <FilterComparison>value;
        }
    }

    public logical: FilterLogical;
}