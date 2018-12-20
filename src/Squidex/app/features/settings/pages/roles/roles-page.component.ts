/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Observable, of } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';

import {
    AddRoleForm,
    AppRoleDto,
    AppRolesService,
    AppsState,
    AutocompleteSource,
    RolesState
} from '@app/shared';

class PermissionsAutocomplete implements AutocompleteSource {
    private permissions: string[] = [];

    constructor(appsState: AppsState, rolesService: AppRolesService) {
        rolesService.getPermissions(appsState.appName).subscribe(x => this.permissions = x);
    }

    public find(query: string): Observable<any[]> {
        return of(this.permissions.filter(y => y.indexOf(query) === 0));
    }
}

@Component({
    selector: 'sqx-roles-page',
    styleUrls: ['./roles-page.component.scss'],
    templateUrl: './roles-page.component.html'
})
export class RolesPageComponent implements OnInit {
    public addRoleForm = new AddRoleForm(this.formBuilder);

    public allPermissions: AutocompleteSource = new PermissionsAutocomplete(this.appsState, this.rolesService);

    constructor(
        public readonly appsState: AppsState,
        public readonly rolesService: AppRolesService,
        public readonly rolesState: RolesState,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.rolesState.load().pipe(onErrorResumeNext()).subscribe();
    }

    public reload() {
        this.rolesState.load(true).pipe(onErrorResumeNext()).subscribe();
    }

    public cancelAddRole() {
        this.addRoleForm.submitCompleted();
    }

    public addRole() {
        const value = this.addRoleForm.submit();

        if (value) {
            this.rolesState.add(value)
                .subscribe(() => {
                    this.addRoleForm.submitCompleted({});
                }, error => {
                    this.addRoleForm.submitFailed(error);
                });
        }
    }

    public trackByRole(index: number, role: AppRoleDto) {
        return role.name;
    }
}

