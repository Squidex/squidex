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
    AppContributorDto,
    AppContributorsDto,
    AppContributorsService,
    AppsState,
    AutocompleteSource,
    DialogService,
    PublicUserDto,
    UsersService
} from '@app/shared';

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
                ]
            ]
        });

    constructor(
        public readonly appsState: AppsState,
        private readonly appContributorsService: AppContributorsService,
        private readonly dialogs: DialogService,
        private readonly formBuilder: FormBuilder,
        usersService: UsersService
    ) {
        this.usersDataSource = new UsersDataSource(usersService, this);
    }

    public ngOnInit() {
        this.load();
    }

    public load() {
        this.appContributorsService.getContributors(this.appsState.appName)
            .subscribe(dto => {
                this.updateContributorsFromDto(dto);
            }, error => {
                this.dialogs.notifyError(error);
            });
    }

    public removeContributor(contributor: AppContributorDto) {
        this.appContributorsService.deleteContributor(this.appsState.appName, contributor.contributorId, this.appContributors.version)
            .subscribe(dto => {
                this.updateContributors(this.appContributors.removeContributor(contributor, dto.version));
            }, error => {
                this.dialogs.notifyError(error);
            });
    }

    public changePermission(contributor: AppContributorDto, permission: string) {
        const requestDto = contributor.changePermission(permission);

        this.appContributorsService.postContributor(this.appsState.appName, requestDto, this.appContributors.version)
            .subscribe(dto => {
                this.updateContributors(this.appContributors.updateContributor(contributor, dto.version));
            }, error => {
                this.dialogs.notifyError(error);
            });
    }

    public assignContributor() {
        let value: any = this.addContributorForm.controls['user'].value;

        if (value instanceof PublicUserDto) {
            value = value.id;
        }

        const requestDto = new AppContributorDto(value, 'Editor');

        this.appContributorsService.postContributor(this.appsState.appName, requestDto, this.appContributors.version)
            .subscribe(dto => {
                this.updateContributors(this.appContributors.addContributor(new AppContributorDto(dto.payload.contributorId, requestDto.permission), dto.version));
                this.resetContributorForm();
            }, error => {
                this.dialogs.notifyError(error);

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
    }
}
