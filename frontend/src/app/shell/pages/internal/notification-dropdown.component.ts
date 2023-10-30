/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { map, switchMap, tap } from 'rxjs/operators';
import { AuthService, CollaborationService, Comment, ModalModel, SharedArray, Subscriptions } from '@app/shared';

@Component({
    selector: 'sqx-notification-dropdown',
    styleUrls: ['./notification-dropdown.component.scss'],
    templateUrl: './notification-dropdown.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        CollaborationService,
    ],
})
export class NotificationDropdownComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public modalMenu = new ModalModel();

    public commentsArray?: SharedArray<Comment>;
    public commentsUnread!: Observable<number>;

    public userToken: string;

    constructor(authService: AuthService,
        private readonly collaborations: CollaborationService,
    ) {
        this.userToken = authService.user!.token;
    }

    public ngOnInit() {
        this.collaborations.connect('users/collaboration');

        const comments$ = this.collaborations.getArray<Comment>('stream');

        this.commentsUnread =
            comments$.pipe(switchMap(x => x.itemsChanges),
                map(array => {
                    return array.filter(x => !x.isRead).length;
                }));

        this.subscriptions.add(
            comments$
                .subscribe(array => {
                    this.commentsArray = array;
                }));

        this.subscriptions.add(
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

        const toDelete = this.commentsArray.items.length - 100;

        if (toDelete > 0) {
            this.commentsArray.remove(0, toDelete);
        }

        this.commentsArray.items.forEach((item, i) => {
            if (!item.isRead) {
                this.commentsArray?.set(i, { ...item, isRead: true });
            }
        });
    }

    public trackByComment(index: number) {
        return index;
    }
}
