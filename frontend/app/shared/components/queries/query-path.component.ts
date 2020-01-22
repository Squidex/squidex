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
    template: `
        <sqx-dropdown [items]="model.fields | sqxKeys" [ngModel]="path" (ngModelChange)="pathChange.emit($event)" [canSearch]="false" separated="true">
            <ng-template let-field="$implicit">
                <div class="row">
                    <div class="col-auto">
                        <div class="badge badge-pill badge-primary">{{model.fields[field].displayName}}</div>
                    </div>
                    <div class="col text-right">
                        <small class="text-muted">{{model.fields[field].description}}</small>
                    </div>
                </div>
            </ng-template>

            <ng-template let-field="$implicit">
                {{model.fields[field].displayName}}
            </ng-template>
        </sqx-dropdown>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class QueryPathComponent {
    @Output()
    public pathChange = new EventEmitter<string>();

    @Input()
    public path: string;

    @Input()
    public model: QueryModel;
}