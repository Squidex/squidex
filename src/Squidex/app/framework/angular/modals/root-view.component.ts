/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ViewChild, ViewContainerRef } from '@angular/core';

import { PureComponent } from '@app/framework/internal';

@Component({
    selector: 'sqx-root-view',
    styleUrls: ['./root-view.component.scss'],
    templateUrl: './root-view.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class RootViewComponent extends PureComponent {
    @ViewChild('element', { read: ViewContainerRef })
    public viewContainer: ViewContainerRef;

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector);
    }
}