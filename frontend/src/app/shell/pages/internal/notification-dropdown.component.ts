/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { timer } from 'rxjs';
import { onErrorResumeNext, switchMap, tap } from 'rxjs/operators';
import { AuthService, CommentDto, CommentsService, CommentsState, DialogService, LocalStoreService, ModalModel, ResourceOwner, Settings } from '@app/shared';

@Component({
    selector: 'sqx-notification-dropdown',
    styleUrls: ['./notification-dropdown.component.scss'],
    templateUrl: './notification-dropdown.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationDropdownComponent extends ResourceOwner implements OnInit {
    public modalMenu = new ModalModel();

    public commentsState: CommentsState;

    public versionRead = -1;
    public versionReceived = -1;
    public unread = 0;

    public userToken = '';

    constructor(authService: AuthService, commentsService: CommentsService, dialogs: DialogService,
        private readonly changeDetector: ChangeDetectorRef,
        private readonly localStore: LocalStoreService,
    ) {
        super();

        this.userToken = authService.user!.token;

        this.versionRead = localStore.getInt(Settings.Local.NOTIFICATION_VERSION, -1);
        this.versionReceived = this.versionRead;

        this.updateVersion();

        const commentsUrl = `users/${authService.user!.id}/notifications`;

        this.commentsState =
            new CommentsState(
                commentsUrl,
                commentsService,
                dialogs,
                true,
                this.versionRead);
    }

    public ngOnInit() {
        this.own(
            this.modalMenu.isOpenChanges.pipe(
                tap(_ => {
                    this.updateVersion();
                }),
            ));

        this.own(
            this.commentsState.versionNumber.pipe(
                tap(version => {
                    this.versionReceived = version;

                    this.updateVersion();

                    this.changeDetector.detectChanges();
                })));

        this.own(timer(0, 4000).pipe(switchMap(() => this.commentsState.load(true).pipe(onErrorResumeNext()))));
    }

    public trackByComment(_index: number, comment: CommentDto) {
        return comment.id;
    }

    private updateVersion() {
        this.unread = Math.max(0, this.versionReceived - this.versionRead);

        if (this.modalMenu.isOpen) {
            this.versionRead = this.versionReceived;

            this.localStore.setInt(Settings.Local.NOTIFICATION_VERSION, this.versionRead);
        }
    }
}
