/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { SlicePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { StringColorPipe, TooltipDirective } from '@app/framework';
import { CollaborationService } from '@app/shared/internal';
import { UserPicturePipe } from './pipes';

@Component({
    selector: 'sqx-watching-users',
    styleUrls: ['./watching-users.component.scss'],
    templateUrl: './watching-users.component.html',
    imports: [
        SlicePipe,
        StringColorPipe,
        TooltipDirective,
        UserPicturePipe,
    ],
})
export class WatchingUsersComponent {
    protected readonly collaboration = inject(CollaborationService);
}
