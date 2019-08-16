/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, ViewChild, ViewContainerRef } from '@angular/core';

@Component({
    selector: 'sqx-root-view',
    template: `
        <div #element></div>

        <ng-content></ng-content>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class RootViewComponent {
    @ViewChild('element', { read: ViewContainerRef, static: false })
    public viewContainer: ViewContainerRef;
}