/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Subscription } from 'rxjs';

import {
    AuthService,
    ComponentBase,
    DialogService,
    ImmutableArray,
    MessageBus,
    Pager,
    UserDto,
    UserManagementService
} from 'shared';

import { UserCreated, UserUpdated } from './../messages';

@Component({
    selector: 'sqx-users-page',
    styleUrls: ['./users-page.component.scss'],
    templateUrl: './users-page.component.html'
})
export class UsersPageComponent extends ComponentBase implements OnDestroy, OnInit {
    private userCreatedSubscription: Subscription;
    private userUpdatedSubscription: Subscription;

    public currentUserId: string;

    public usersItems = ImmutableArray.empty<UserDto>();
    public usersPager = new Pager(0);
    public usersFilter = new FormControl();
    public usersQuery = '';

    constructor(dialogs: DialogService,
        private readonly userManagementService: UserManagementService,
        private readonly authService: AuthService,
        private readonly messageBus: MessageBus
    ) {
        super(dialogs);
    }

    public ngOnDestroy() {
        this.userCreatedSubscription.unsubscribe();
        this.userUpdatedSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.userCreatedSubscription =
            this.messageBus.of(UserCreated)
                .subscribe(message => {
                    this.usersItems = this.usersItems.pushFront(message.user);
                    this.usersPager = this.usersPager.incrementCount();
                });

        this.userUpdatedSubscription =
            this.messageBus.of(UserUpdated)
                .subscribe(message => {
                    this.usersItems = this.usersItems.replaceBy('id', message.user);
                });

        this.currentUserId = this.authService.user!.id;

        this.load();
    }

    public search() {
        this.usersPager = new Pager(0);
        this.usersQuery = this.usersFilter.value;

        this.load();
    }

    public load(showInfo = false) {
        this.userManagementService.getUsers(this.usersPager.pageSize, this.usersPager.skip, this.usersQuery)
            .subscribe(dtos => {
                this.usersItems = ImmutableArray.of(dtos.items);
                this.usersPager = this.usersPager.setCount(dtos.total);

                if (showInfo) {
                    this.notifyInfo('Users reloaded.');
                }
            }, error => {
                this.notifyError(error);
            });
    }

    public lock(user: UserDto) {
        this.userManagementService.lockUser(user.id)
            .subscribe(() => {
                this.usersItems = this.usersItems.replaceBy('id', user.lock());
            }, error => {
                this.notifyError(error);
            });
    }

    public unlock(user: UserDto) {
        this.userManagementService.unlockUser(user.id)
            .subscribe(() => {
                this.usersItems = this.usersItems.replaceBy('id', user.unlock());
            }, error => {
                this.notifyError(error);
            });
    }

    public goNext() {
        this.usersPager = this.usersPager.goNext();

        this.load();
    }

    public goPrev() {
        this.usersPager = this.usersPager.goPrev();

        this.load();
    }
}

