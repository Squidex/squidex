/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { FilterableField, QueryModel } from '@app/shared/internal';

@Component({
    selector: 'sqx-query-path[model]',
    styleUrls: ['./query-path.component.scss'],
    templateUrl: './query-path.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QueryPathComponent implements OnChanges {
    @Output()
    public pathChange = new EventEmitter<string>();

    @Input()
    public path = '';

    @Input()
    public model!: QueryModel;

    public field?: FilterableField;

    public ngOnChanges() {
        this.field = this.model.schema.fields.find(x => x.path === this.path);
    }
}
