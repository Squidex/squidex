/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgFor, NgIf, SlicePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { StringColorPipe, TooltipDirective } from '@app/framework';
import { CollaborationService, Profile } from '@app/shared/internal';
import { UserPicturePipe } from './pipes';

@Component({
    selector: 'sqx-watching-users',
    styleUrls: ['./watching-users.component.scss'],
    templateUrl: './watching-users.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [
        NgIf,
        NgFor,
        TooltipDirective,
        AsyncPipe,
        SlicePipe,
        StringColorPipe,
        UserPicturePipe,
    ],
})
export class WatchingUsersComponent {
    constructor(
        public readonly collaboration: CollaborationService,
    ) {
    }

    public trackByUser(_: number, item: { user: Profile }) {
        return item.user.id;
    }
}
