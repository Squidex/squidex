/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { slideRightAnimation } from '@app/framework/internal';

@Component({
    selector: 'sqx-right-panel',
    styleUrls: ['./right-panel.component.scss'],
    templateUrl: './right-panel.component.html',
    animations: [
        slideRightAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class RightPanelComponent {
    public isCollapsed = false;

    public toggle() {
        this.isCollapsed = !this.isCollapsed;
    }
}