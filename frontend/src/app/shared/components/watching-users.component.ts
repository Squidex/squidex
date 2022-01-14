/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { timer } from 'rxjs';
import { AppsState, CommentsService, switchSafe } from '@app/shared/internal';

@Component({
    selector: 'sqx-watching-users[resource]',
    styleUrls: ['./watching-users.component.scss'],
    templateUrl: './watching-users.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WatchingUsersComponent {
    private appName: string;

    @Input()
    public resource!: string;

    public users =
        timer(0, 5000).pipe(
            switchSafe((() => this.commentsService.getWatchingUsers(this.appName, this.resource))));

    constructor(appsState: AppsState,
        private readonly commentsService: CommentsService,
    ) {
        this.appName = appsState.appName;
    }
}
