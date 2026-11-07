/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, signal, inject } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ApiUrlConfig, CopyDirective, FormErrorComponent, FormHintComponent, FormRowComponent, LayoutComponent, ListViewComponent, SidebarMenuDirective, TeamsState, ToggleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { TeamAuthState, UpdateTeamAuthForm } from '../../internal';

@Component({
    selector: 'sqx-auth-page',
    styleUrls: ['./auth-page.component.scss'],
    templateUrl: './auth-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        CopyDirective,
        FormErrorComponent,
        FormHintComponent,
        FormRowComponent,
        FormsModule,
        LayoutComponent,
        ListViewComponent,
        ReactiveFormsModule,
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
    private readonly authState = inject(TeamAuthState);

    protected readonly urlToRedirect;
    protected readonly urlToTest;

    protected readonly isEditable = signal(false);
    protected readonly isEditing = signal(false);

    protected readonly updateForm = new UpdateTeamAuthForm();

    constructor() {
        const apiUrl = inject(ApiUrlConfig);
        const teamsState = inject(TeamsState);

        this.urlToTest = apiUrl.buildUrl('/identity-server/test');

        this.urlToRedirect = computed(() => {
            const team = teamsState.selectedTeam();
            if (!team) {
                return '';
            }

            return apiUrl.buildUrl(`/identity-server/signin-${team.id}`);
        });

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

        const value = this.updateForm.submit();
        if (!value) {
            return;
        }

        this.authState.update(value)
            .subscribe({
                next: scheme => {
                    this.updateForm.submitCompleted({ newValue: scheme || {} });
                },
                error: error => {
                    this.updateForm.submitFailed(error);
                },
            });
    }
}
