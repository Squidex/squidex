/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect } from '@angular/core';
import { AuthService, CollaborationService, CommentComponent, CommentsState, DropdownMenuComponent, ModalDirective, ModalModel, ModalPlacementDirective, TranslatePipe } from '@app/shared';

@Component({
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
export class NotificationDropdownComponent {
    public readonly modalMenu = new ModalModel();

    public readonly commentUser: string;
    public readonly commentItems = this.commentsState.groupedUnresolvedComments;

    constructor(authService: AuthService,
        public readonly commentsState: CommentsState,
        public readonly collaboration: CollaborationService,
    ) {
        this.commentUser = authService.user!.token;

        this.collaboration.connect('users/collaboration');

        effect(onCleanup => {
            const subscription =
                this.modalMenu.isOpenChanges
                    .subscribe(() => {
                        this.commentsState.prune(100);
                        this.commentsState.markRead();
                    });

            onCleanup(() => subscription.unsubscribe());
        });
    }
}
