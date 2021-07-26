/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { AppDto, fadeAnimation, ModalModel } from '@app/shared';

@Component({
    selector: 'sqx-app[app]',
    styleUrls: ['./app.component.scss'],
    templateUrl: './app.component.html',
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {
    @Input()
    public app: AppDto;

    @Output()
    public leave = new EventEmitter<AppDto>();

    public dropdown = new ModalModel();
}
