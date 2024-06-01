/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, SlicePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { StringColorPipe, TooltipDirective } from '@app/framework';
import { CollaborationService } from '@app/shared/internal';
import { UserPicturePipe } from './pipes';

@Component({
    standalone: true,
    selector: 'sqx-watching-users',
    styleUrls: ['./watching-users.component.scss'],
    templateUrl: './watching-users.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        SlicePipe,
        StringColorPipe,
        TooltipDirective,
        UserPicturePipe,
    ],
})
export class WatchingUsersComponent {
    constructor(
        public readonly collaboration: CollaborationService,
    ) {
    }
}
