/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, effect, inject, untracked } from '@angular/core';
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
        CommentComponent,
        DropdownMenuComponent,
        ModalDirective,
        ModalPlacementDirective,
        TranslatePipe,
    ],
})
export class NotificationDropdownComponent {
    private readonly collaboration = inject(CollaborationService);

    protected readonly commentsState = inject(CommentsState);

    protected readonly modalMenu = new ModalModel();

    protected readonly commentUser: string;

    protected readonly commentItems = this.commentsState.groupedUnresolvedComments;

    constructor() {
        const authService = inject(AuthService);

        this.commentUser = authService.user!.token;

        this.collaboration.connect('users/collaboration');

        effect(() => {
            this.modalMenu.isOpen();

            untracked(() => {
                this.commentsState.prune(100);
                this.commentsState.markRead();
            });
        });
    }
}
