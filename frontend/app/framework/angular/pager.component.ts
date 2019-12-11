/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import { Pager } from '@app/framework/internal';

@Component({
    selector: 'sqx-pager',
    styleUrls: ['./pager.component.scss'],
    templateUrl: './pager.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class PagerComponent {
    @Output()
    public pagerChange = new EventEmitter<Pager>();

    @Input()
    public pager: Pager;

    @Input()
    public autoHide = false;

    public goPrev() {
        this.pagerChange.emit(this.pager.goPrev());
    }

    public goNext() {
        this.pagerChange.emit(this.pager.goNext());
    }

    public setPageSize(pageSize: number) {
        this.pagerChange.emit(this.pager.setPageSize(pageSize));
    }
}