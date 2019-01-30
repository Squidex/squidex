/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, Output } from '@angular/core';

import { Pager, PureComponent } from '@app/framework/internal';

@Component({
    selector: 'sqx-pager',
    styleUrls: ['./pager.component.scss'],
    templateUrl: './pager.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class PagerComponent extends PureComponent {
    @Output()
    public nextPage = new EventEmitter();

    @Output()
    public prevPage = new EventEmitter();

    @Input()
    public pager: Pager;

    @Input()
    public hideWhenButtonsDisabled = false;

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector);
    }
}