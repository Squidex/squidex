/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, ViewChild, ViewContainerRef } from '@angular/core';

@Component({
    selector: 'sqx-root-view',
    template: `
        <div #element></div>

        <ng-content></ng-content>
    `
})
export class RootViewComponent {
    @ViewChild('element')
    public viewContainer: ViewContainerRef;
}