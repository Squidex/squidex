/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Observable } from 'rxjs';

import {
    AppComponentBase,
    AppContributorDto,
    AppContributorsDto,
    AppContributorsService,
    AppsStoreService,
    AuthService,
    AutocompleteSource,
    DialogService,
    HistoryChannelUpdated,
    MessageBus,
    UsersService
} from 'shared';

export class UsersDataSource implements AutocompleteSource {
    constructor(
        private readonly usersService: UsersService,
        private readonly component: ContributorsPageComponent
    ) {
    }

    public find(query: string): Observable<any[]> {
        return this.usersService.getUsers(query)
            .map(users => {
                const results: any[] = [];

                for (let user of users) {
                    if (!this.component.appContributors || !this.component.appContributors.contributors.find(t => t.contributorId === user.id)) {
                        results.push(user);
                    }
                }
                return results;
            });
    }
}

@Component({
    selector: 'sqx-contributors-page',
    styleUrls: ['./contributors-page.component.scss'],
    templateUrl: './contributors-page.component.html'
})
export class ContributorsPageComponent extends AppComponentBase implements OnInit {
    public appContributors: AppContributorsDto;

    public currentUserId: string;

    public maxContributors = -1;

    public usersDataSource: UsersDataSource;
    public usersPermissions = [ 'Owner', 'Developer', 'Editor' ];

    public get canAddContributor() {
        return this.addContributorForm.valid && (this.maxContributors <= -1 || this.appContributors.contributors.length < this.maxContributors);
    }

    public addContributorForm =
        this.formBuilder.group({
            user: [null,
                [
                    Validators.required
                ]]
        });

    constructor(apps: AppsStoreService, dialogs: DialogService, usersService: UsersService, authService: AuthService,
        private readonly appContributorsService: AppContributorsService,
        private readonly messageBus: MessageBus,
        private readonly formBuilder: FormBuilder
    ) {
        super(dialogs, apps, authService);

        this.usersDataSource = new UsersDataSource(usersService, this);
    }

    public ngOnInit() {
        this.currentUserId = this.authService.user!.id;

        this.load();
    }

    public load() {
        this.appNameOnce()
            .switchMap(app => this.appContributorsService.getContributors(app).retry(2))
            .subscribe(dto => {
                this.updateContributorsFromDto(dto);
            }, error => {
                this.notifyError(error);
            });
    }

    public removeContributor(contributor: AppContributorDto) {
        this.appNameOnce()
            .switchMap(app => this.appContributorsService.deleteContributor(app, contributor.contributorId, this.appContributors.version))
            .subscribe(dto => {
                this.updateContributors(this.appContributors.removeContributor(contributor, dto.version));
            }, error => {
                this.notifyError(error);
            });
    }

    public changePermission(contributor: AppContributorDto, permission: string) {
        const requestDto = contributor.changePermission(permission);

        this.appNameOnce()
            .switchMap(app => this.appContributorsService.postContributor(app, requestDto, this.appContributors.version))
            .subscribe(dto => {
                this.updateContributors(this.appContributors.updateContributor(contributor, dto.version));
            }, error => {
                this.notifyError(error);
            });
    }

    public assignContributor() {
        const requestDto = new AppContributorDto(this.addContributorForm.controls['user'].value.id, 'Editor');

        this.appNameOnce()
            .switchMap(app => this.appContributorsService.postContributor(app, requestDto, this.appContributors.version))
            .subscribe(dto => {
                this.updateContributors(this.appContributors.addContributor(requestDto, dto.version));
                this.resetContributorForm();
            }, error => {
                this.notifyError(error);
                this.resetContributorForm();
            });
    }

    private resetContributorForm() {
        this.addContributorForm.reset();
    }

    private updateContributorsFromDto(appContributors: AppContributorsDto) {
        this.updateContributors(appContributors);

        this.maxContributors = appContributors.maxContributors;
    }

    private updateContributors(appContributors: AppContributorsDto) {
        this.appContributors = appContributors;

        this.messageBus.emit(new HistoryChannelUpdated());
    }
}
