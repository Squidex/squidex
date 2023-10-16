/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { switchMap, tap } from 'rxjs/operators';
import { AuthService, CollaborationService, Comment, ModalModel, ResourceOwner, SharedArray } from '@app/shared';

@Component({
    selector: 'sqx-notification-dropdown',
    styleUrls: ['./notification-dropdown.component.scss'],
    templateUrl: './notification-dropdown.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        CollaborationService,
    ],
})
export class NotificationDropdownComponent extends ResourceOwner implements OnInit {
    private readonly userId: string;

    public modalMenu = new ModalModel();

    public commentsArray?: SharedArray<Comment>;
    public commentsUnread = 0;

    public userToken: string;

    constructor(authService: AuthService,
        private readonly collaborations: CollaborationService,
    ) {
        super();

        this.userId = `users/${authService.user!.id}/notifications`;
        this.userToken = authService.user!.token;
    }

    public ngOnInit() {
        this.collaborations.connect(`users/${this.userId}/notifications`);

        const comments$ = this.collaborations.getArray<Comment>('stream');

        this.own(
            comments$
                .subscribe(array => {
                    this.commentsArray = array;
                }));

        this.own(
            comments$.pipe(switchMap(x => x.itemsChanges))
                .subscribe(array => {
                    this.commentsUnread = array.filter(x => !x.isRead).length;
                }));

        this.own(
            this.modalMenu.isOpenChanges.pipe(
                tap(_ => {
                    this.markRead();
                }),
            ));
    }

    public markRead() {
        if (!this.commentsArray) {
            return;
        }

        this.commentsArray?.items.forEach((item, i) => {
            if (!item.isRead) {
                this.commentsArray?.set(i, { ...item, isRead: true });
            }
        });
    }

    public delete(index: number) {
        this.commentsArray?.remove(index);
    }

    public trackByComment(index: number) {
        return index;
    }
}
