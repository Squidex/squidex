/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { Observable, of } from 'rxjs';

import {
    AppsState,
    AutocompleteSource,
    RoleDto,
    RolesService,
    RolesState
} from '@app/shared';

class PermissionsAutocomplete implements AutocompleteSource {
    private permissions: string[] = [];

    constructor(appsState: AppsState, rolesService: RolesService) {
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
    public allPermissions: AutocompleteSource = new PermissionsAutocomplete(this.appsState, this.rolesService);

    constructor(
        public readonly appsState: AppsState,
        public readonly rolesService: RolesService,
        public readonly rolesState: RolesState
    ) {
    }

    public ngOnInit() {
        this.rolesState.load();
    }

    public reload() {
        this.rolesState.load(true);
    }

    public trackByRole(role: RoleDto) {
        return role.name;
    }
}

