/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/no-output-native */

import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { StopClickDirective, TranslatePipe } from '@app/framework';
import { equalsQuery, Query, SavedQuery } from '@app/shared/internal';

@Component({
    selector: 'sqx-query-list',
    styleUrls: ['./query-list.component.scss'],
    templateUrl: './query-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        StopClickDirective,
        TranslatePipe,
    ],
})
export class QueryListComponent {
    @Output()
    public search = new EventEmitter<Query>();

    @Output()
    public remove = new EventEmitter<SavedQuery>();

    @Input()
    public queryUsed?: Query | null;

    @Input()
    public queries?: ReadonlyArray<SavedQuery> | null;

    @Input({ transform: booleanAttribute })
    public canRemove?: boolean | null;

    @Input()
    public types = '';

    public isSelectedQuery(saved: SavedQuery) {
        return equalsQuery(saved.query, this.queryUsed);
    }
}
