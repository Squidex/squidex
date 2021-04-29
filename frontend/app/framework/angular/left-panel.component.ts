/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { slideRightAnimation } from '@app/framework/internal';

@Component({
    selector: 'sqx-left-panel',
    styleUrls: ['./left-panel.component.scss'],
    templateUrl: './left-panel.component.html',
    animations: [
        slideRightAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class LeftPanelComponent {
    @Input()
    public title: string;

    @Input()
    public titleIcon: string;

    @Input()
    public customHeader: boolean;

    @Input()
    public sidebarWidth: any = '15rem';

    public isCollapsed = false;

    public toggle() {
        this.isCollapsed = !this.isCollapsed;
    }
}