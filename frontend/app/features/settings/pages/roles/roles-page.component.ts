/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { AppsState, AutocompleteSource, RoleDto, RolesService, RolesState, SchemasState } from '@app/shared';
import { Observable, of } from 'rxjs';

class PermissionsAutocomplete implements AutocompleteSource {
    private permissions: ReadonlyArray<string> = [];

    constructor(appsState: AppsState, rolesService: RolesService) {
        // eslint-disable-next-line no-return-assign
        rolesService.getPermissions(appsState.appName).subscribe(x => this.permissions = x);
    }

    public find(query: string): Observable<ReadonlyArray<any>> {
        return of(this.permissions.filter(y => y.indexOf(query) === 0));
    }
}

@Component({
    selector: 'sqx-roles-page',
    styleUrls: ['./roles-page.component.scss'],
    templateUrl: './roles-page.component.html',
})
export class RolesPageComponent implements OnInit {
    public allPermissions: AutocompleteSource = new PermissionsAutocomplete(this.appsState, this.rolesService);

    constructor(
        private readonly appsState: AppsState,
        public readonly rolesService: RolesService,
        public readonly rolesState: RolesState,
        public readonly schemasState: SchemasState,
    ) {
    }

    public ngOnInit() {
        this.schemasState.loadIfNotLoaded();

        this.rolesState.load();
    }

    public reload() {
        this.rolesState.load(true);
    }

    public trackByRole(_index: number, role: RoleDto) {
        return role.name;
    }
}
