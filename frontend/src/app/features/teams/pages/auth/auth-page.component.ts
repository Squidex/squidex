/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { map } from 'rxjs';
import { ApiUrlConfig, CopyDirective, FormErrorComponent, FormHintComponent, FormRowComponent, LayoutComponent, ListViewComponent, SidebarMenuDirective, TeamsState, ToggleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { TeamAuthState, UpdateTeamAuthForm } from '../../internal';

@Component({
    selector: 'sqx-auth-page',
    styleUrls: ['./auth-page.component.scss'],
    templateUrl: './auth-page.component.html',
    imports: [
        CopyDirective,
        FormErrorComponent,
        FormField,
        FormHintComponent,
        FormRowComponent,
        FormsModule,
        LayoutComponent,
        ListViewComponent,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        SidebarMenuDirective,
        ToggleComponent,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class AuthPageComponent {
    private readonly apiUrl = inject(ApiUrlConfig);
    private readonly authState = inject(TeamAuthState);
    private readonly teamsState = inject(TeamsState);

    protected readonly isEditable = signal(false);

    protected readonly isEditing = signal(false);

    protected readonly urlToRedirect = computed(() => {
        const team = this.teamsState.selectedTeam();
        if (!team) {
            return '';
        }

        return this.apiUrl.buildUrl(`/identity-server/signin-${team.id}`);
    });

    protected readonly urlToTest = this.apiUrl.buildUrl('/identity-server/test');

    protected readonly updateForm = new UpdateTeamAuthForm();

    constructor() {
        effect(() => {
            const scheme = this.authState.scheme();
            const isEditable = this.authState.canUpdate();

            this.isEditable.set(isEditable);
            this.isEditing.set(!!scheme);

            this.updateForm.load(scheme || {});
            this.updateForm.setEnabled(isEditable);
        });

        this.authState.load();
    }

    protected toggle(isEditing: boolean) {
        if (!this.isEditable()) {
            return;
        }

        this.isEditing.set(isEditing);
        if (!isEditing) {
            this.authState.update(undefined);
        }
    }

    protected save() {
        if (!this.isEditable()) {
            return;
        }

        this.updateForm.submitAndUpdate(value => {
            return this.authState.update(value).pipe(map(scheme => scheme || {}));
        }, { resetAfterSubmit: true });
    }
}
