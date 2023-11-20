/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { map, tap } from 'rxjs/operators';
import { AuthService, CollaborationService, CommentComponent, CommentsState, DropdownMenuComponent, getGroupedComments, ModalDirective, ModalModel, ModalPlacementDirective, Subscriptions, TranslatePipe } from '@app/shared';

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
        NgFor,
        NgIf,
        TranslatePipe,
    ],
})
export class NotificationDropdownComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public modalMenu = new ModalModel();

    public userToken: string;

    public commentItems =
        this.comments.itemsChanges.pipe(
            map(items => getGroupedComments(items, [])));

    constructor(authService: AuthService,
        public readonly comments: CommentsState,
        public readonly collaboration: CollaborationService,
    ) {
        this.userToken = authService.user!.token;
    }

    public ngOnInit() {
        this.collaboration.connect('users/collaboration');

        this.subscriptions.add(
            this.modalMenu.isOpenChanges.pipe(
                tap(_ => {
                    this.comments.prune(100);
                    this.comments.markRead();
                }),
            ));
    }

    public trackByComment(index: number) {
        return index;
    }
}
