/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Observable, of } from 'rxjs';
import { AppsState, AutocompleteSource, FormHintComponent, LayoutComponent, ListViewComponent, RolesService, RolesState, SchemasState, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { RoleAddFormComponent } from './role-add-form.component';
import { RoleComponent } from './role.component';

class PermissionsAutocomplete implements AutocompleteSource {
    private permissions: ReadonlyArray<string> = [];

    constructor(appsState: AppsState, rolesService: RolesService) {
        rolesService.getPermissions(appsState.appName).subscribe(x => this.permissions = x);
    }

    public find(query: string): Observable<ReadonlyArray<any>> {
        if (!query) {
            return of(this.permissions);
        }

        return of(this.permissions.filter(y => y.indexOf(query) === 0));
    }
}

@Component({
    selector: 'sqx-roles-page',
    styleUrls: ['./roles-page.component.scss'],
    templateUrl: './roles-page.component.html',
    imports: [
        FormHintComponent,
        LayoutComponent,
        ListViewComponent,
        RoleAddFormComponent,
        RoleComponent,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        ShortcutDirective,
        SidebarMenuDirective,
        TitleComponent,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class RolesPageComponent {
    private readonly appsState = inject(AppsState);
    private readonly rolesService = inject(RolesService);

    protected readonly rolesState = inject(RolesState);

    protected readonly schemasState = inject(SchemasState);

    protected readonly allPermissions: AutocompleteSource = new PermissionsAutocomplete(this.appsState, this.rolesService);

    constructor() {
        this.schemasState.loadIfNotLoaded();

        this.rolesState.load();
    }

    protected reload() {
        this.rolesState.load(true);
    }
}
