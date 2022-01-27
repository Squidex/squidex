/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { QueryModel } from '@app/shared/internal';

@Component({
    selector: 'sqx-query-path',
    styleUrls: ['./query-path.component.scss'],
    templateUrl: './query-path.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QueryPathComponent {
    @Output()
    public pathChange = new EventEmitter<string>();

    @Input()
    public path = '';

    @Input()
    public model!: QueryModel;

    public get value() {
        return this.model.fields.find(x => x.fieldPath === this.path);
    }
}
