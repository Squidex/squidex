/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { timer } from 'rxjs';
import { onErrorResumeNext, switchMap, tap } from 'rxjs/operators';

import {
    AuthService,
    CommentDto,
    CommentsService,
    CommentsState,
    DialogService,
    fadeAnimation,
    LocalStoreService,
    ModalModel,
    ResourceOwner
} from '@app/shared';

@Component({
    selector: 'sqx-notifications-menu',
    styleUrls: ['./notifications-menu.component.scss'],
    templateUrl: './notifications-menu.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class NotificationsMenuComponent extends ResourceOwner implements OnInit {
    private isOpen: boolean;
    private configKey: string;

    public modalMenu = new ModalModel();

    public commentsUrl: string;
    public commentsState: CommentsState;

    public userId: string;
    public userToken: string;

    public versionRead = -1;
    public versionReceived = -1;

    public get unread() {
        return Math.max(0, this.versionReceived - this.versionRead);
    }

    constructor(authService: AuthService,
        private readonly changeDetector: ChangeDetectorRef,
        private readonly commentsService: CommentsService,
        private readonly dialogs: DialogService,
        private readonly localStore: LocalStoreService
    ) {
        super();

        this.userToken = authService.user!.token;
        this.userId = authService.user!.id;

        this.configKey = `users.${this.userId}.notifications`;

        this.versionRead = localStore.getInt(this.configKey, -1);
        this.versionReceived = this.versionRead;
    }

    public ngOnInit() {
        this.commentsUrl = `users/${this.userId}/notifications`;
        this.commentsState = new CommentsState(this.commentsUrl, this.commentsService, this.dialogs);

        this.own(
            this.modalMenu.isOpen.pipe(
                tap(isOpen => {
                    this.isOpen = isOpen;

                    this.updateVersion();
                })
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

    public delete(comment: CommentDto) {
        this.commentsState.delete(comment);
    }

    public trackByComment(comment: CommentDto) {
        return comment.id;
    }

    private updateVersion() {
        if (this.isOpen) {
            this.versionRead = this.versionReceived;

            this.localStore.setInt(this.configKey, this.versionRead);
        }
    }
}