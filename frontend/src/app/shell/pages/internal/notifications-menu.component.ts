/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { timer } from 'rxjs';
import { onErrorResumeNext, switchMap, tap } from 'rxjs/operators';
import { AuthService, CommentDto, CommentsService, CommentsState, DialogService, LocalStoreService, ModalModel, ResourceOwner, UIOptions } from '@app/shared';

const CONFIG_KEY = 'notifications.version';

@Component({
    selector: 'sqx-notifications-menu',
    styleUrls: ['./notifications-menu.component.scss'],
    templateUrl: './notifications-menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationsMenuComponent extends ResourceOwner implements OnInit {
    public modalMenu = new ModalModel();

    public commentsState: CommentsState;

    public versionRead = -1;
    public versionReceived = -1;

    public userToken = '';

    public get unread() {
        return Math.max(0, this.versionReceived - this.versionRead);
    }

    public isNotifoConfigured = false;

    constructor(authService: AuthService, commentsService: CommentsService, dialogs: DialogService, uiOptions: UIOptions,
        private readonly changeDetector: ChangeDetectorRef,
        private readonly localStore: LocalStoreService,
    ) {
        super();

        const notifoApiKey = authService.user?.notifoToken;
        const notifoApiUrl = uiOptions.get('more.notifoApi');

        this.isNotifoConfigured = !!notifoApiKey && !!notifoApiUrl;

        this.userToken = authService.user!.token;

        this.versionRead = localStore.getInt(CONFIG_KEY, -1);
        this.versionReceived = this.versionRead;

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
        if (this.modalMenu.isOpen) {
            this.versionRead = this.versionReceived;

            this.localStore.setInt(CONFIG_KEY, this.versionRead);
        }
    }
}
