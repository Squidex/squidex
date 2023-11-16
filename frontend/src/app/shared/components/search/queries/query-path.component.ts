/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DropdownComponent, FormHintComponent } from '@app/framework';
import { FilterableField, QueryModel } from '@app/shared/internal';

@Component({
    standalone: true,
    selector: 'sqx-query-path',
    styleUrls: ['./query-path.component.scss'],
    templateUrl: './query-path.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        DropdownComponent,
        FormHintComponent,
        FormsModule,
    ],
})
export class QueryPathComponent {
    @Output()
    public pathChange = new EventEmitter<string>();

    @Input()
    public path = '';

    @Input({ required: true })
    public model!: QueryModel;

    public field?: FilterableField;

    public ngOnChanges() {
        this.field = this.model.schema.fields.find(x => x.path === this.path);
    }
}
