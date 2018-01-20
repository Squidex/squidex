/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Observable } from 'rxjs';

import {
    AppContext,
    AppContributorDto,
    AppContributorsDto,
    AppContributorsService,
    AutocompleteSource,
    HistoryChannelUpdated,
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
    templateUrl: './contributors-page.component.html',
    providers: [
        AppContext
    ]
})
export class ContributorsPageComponent implements OnInit {
    public appContributors: AppContributorsDto;

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

    constructor(public readonly ctx: AppContext, usersService: UsersService,
        private readonly appContributorsService: AppContributorsService,
        private readonly formBuilder: FormBuilder
    ) {
        this.usersDataSource = new UsersDataSource(usersService, this);
    }

    public ngOnInit() {
        this.load();
    }

    public load() {
        this.appContributorsService.getContributors(this.ctx.appName)
            .subscribe(dto => {
                this.updateContributorsFromDto(dto);
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public removeContributor(contributor: AppContributorDto) {
        this.appContributorsService.deleteContributor(this.ctx.appName, contributor.contributorId, this.appContributors.version)
            .subscribe(dto => {
                this.updateContributors(this.appContributors.removeContributor(contributor, dto.version));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public changePermission(contributor: AppContributorDto, permission: string) {
        const requestDto = contributor.changePermission(permission);

        this.appContributorsService.postContributor(this.ctx.appName, requestDto, this.appContributors.version)
            .subscribe(dto => {
                this.updateContributors(this.appContributors.updateContributor(contributor, dto.version));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public assignContributor() {
        const requestDto = new AppContributorDto(this.addContributorForm.controls['user'].value.id, 'Editor');

        this.appContributorsService.postContributor(this.ctx.appName, requestDto, this.appContributors.version)
            .subscribe(dto => {
                this.updateContributors(this.appContributors.addContributor(requestDto, dto.version));
                this.resetContributorForm();
            }, error => {
                this.ctx.notifyError(error);

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

        this.ctx.bus.emit(new HistoryChannelUpdated());
    }
}
