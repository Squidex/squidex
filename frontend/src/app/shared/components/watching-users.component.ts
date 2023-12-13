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
    standalone: true,
    selector: 'sqx-watching-users',
    styleUrls: ['./watching-users.component.scss'],
    templateUrl: './watching-users.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        NgFor,
        NgIf,
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

    public trackByUser(_: number, item: { user: Profile }) {
        return item.user.id;
    }
}
