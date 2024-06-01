/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { AuthService, CollaborationService, CommentComponent, CommentsState, DropdownMenuComponent, ModalDirective, ModalModel, ModalPlacementDirective, Subscriptions, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-notification-dropdown',
    styleUrls: ['./notification-dropdown.component.scss'],
    templateUrl: './notification-dropdown.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        CollaborationService,
        CommentsState,
    ],
    imports: [
        AsyncPipe,
        CommentComponent,
        DropdownMenuComponent,
        ModalDirective,
        ModalPlacementDirective,
        TranslatePipe,
    ],
})
export class NotificationDropdownComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public modalMenu = new ModalModel();

    public commentUser: string;
    public commentItems = this.commentsState.getGroupedComments(of([]));

    constructor(authService: AuthService,
        public readonly commentsState: CommentsState,
        public readonly collaboration: CollaborationService,
    ) {
        this.commentUser = authService.user!.token;
    }

    public ngOnInit() {
        this.collaboration.connect('users/collaboration');

        this.subscriptions.add(
            this.modalMenu.isOpenChanges.pipe(
                tap(_ => {
                    this.commentsState.prune(100);
                    this.commentsState.markRead();
                }),
            ));
    }
}
