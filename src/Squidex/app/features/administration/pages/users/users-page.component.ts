/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';

import {
    AuthService,
    ComponentBase,
    ImmutableArray,
    NotificationService,
    UserDto,
    UserManagementService,
    UsersProviderService
} from 'shared';

@Component({
    selector: 'sqx-users-page',
    styleUrls: ['./users-page.component.scss'],
    templateUrl: './users-page.component.html'
})
export class UsersPageComponent extends ComponentBase implements OnInit {
    public currentUserId: string;

    public usersItems = ImmutableArray.empty<UserDto>();
    public usersTotal = 0;

    public pageSize = 10;

    public canGoNext = false;
    public canGoPrev = false;

    public itemFirst = 0;
    public itemLast = 0;

    public currentPage = 0;
    public currentQuery = '';

    public usersFilter = new FormControl();

    constructor(notifications: NotificationService, users: UsersProviderService,
        private readonly userManagementService: UserManagementService,
        private readonly authService: AuthService
    ) {
        super(notifications, users);
    }

    public ngOnInit() {
        this.currentUserId = this.authService.user!.id;

        this.load();
    }

    public search() {
        this.currentPage = 0;
        this.currentQuery = this.usersFilter.value;

        this.load();
    }

    private load() {
        this.userManagementService.getUsers(this.pageSize, this.currentPage * this.pageSize, this.currentQuery)
            .subscribe(dtos => {
                this.usersItems = ImmutableArray.of(dtos.items);
                this.usersTotal = dtos.total;

                this.updatePaging();
            }, error => {
                this.notifyError(error);
            });
    }

    public lock(id: string) {
        this.userManagementService.lockUser(id)
            .subscribe(() => {
                this.usersItems = this.usersItems.map(u => {
                    if (u.id === id) {
                        return new UserDto(u.id, u.email, u.displayName, u.pictureUrl, true);
                    } else {
                        return u;
                    }
                });
            }, error => {
                this.notifyError(error);
            });
    }

    public unlock(id: string) {
        this.userManagementService.unlockUser(id)
            .subscribe(() => {
                this.usersItems = this.usersItems.map(u => {
                    if (u.id === id) {
                        return new UserDto(u.id, u.email, u.displayName, u.pictureUrl, false);
                    } else {
                        return u;
                    }
                });
            }, error => {
                this.notifyError(error);
            });
    }

    public goNext() {
        if (this.canGoNext) {
            this.currentPage++;

            this.updatePaging();
            this.load();
        }
    }

    public goPrev() {
        if (this.canGoPrev) {
            this.currentPage--;

            this.updatePaging();
            this.load();
        }
    }

    private updatePaging() {
        const totalPages = Math.ceil(this.usersTotal / this.pageSize);

        this.itemFirst = this.currentPage * this.pageSize + 1;
        this.itemLast = Math.min(this.usersTotal, (this.currentPage + 1) * this.pageSize);

        this.canGoNext = this.currentPage < totalPages - 1;
        this.canGoPrev = this.currentPage > 0;
    }
}

