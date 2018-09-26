/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import { Pager } from './../internal';

@Component({
    selector: 'sqx-pager',
    styleUrls: ['./pager.component.scss'],
    templateUrl: './pager.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class PagerComponent {
    @Input()
    public pager: Pager;

    @Input()
    public hideWhenButtonsDisabled = false;

    @Output()
    public next = new EventEmitter();

    @Output()
    public prev = new EventEmitter();
}