/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { CollaborationService, Profile } from '@app/shared/internal';

@Component({
    selector: 'sqx-watching-users',
    styleUrls: ['./watching-users.component.scss'],
    templateUrl: './watching-users.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
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
