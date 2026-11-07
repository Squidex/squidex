/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { SlicePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { StringColorPipe, TooltipDirective } from '@app/framework';
import { CollaborationService } from '@app/shared/internal';
import { UserPicturePipe } from './pipes';

@Component({
    selector: 'sqx-watching-users',
    styleUrls: ['./watching-users.component.scss'],
    templateUrl: './watching-users.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        SlicePipe,
        StringColorPipe,
        TooltipDirective,
        UserPicturePipe,
    ],
})
export class WatchingUsersComponent {
    constructor(
        protected readonly collaboration: CollaborationService,
    ) {
    }
}
