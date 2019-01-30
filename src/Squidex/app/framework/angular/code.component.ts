/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component } from '@angular/core';

import { PureComponent } from '@app/framework/internal';

@Component({
    selector: 'sqx-code',
    styleUrls: ['./code.component.scss'],
    templateUrl: './code.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CodeComponent extends PureComponent {
    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector);
    }
}