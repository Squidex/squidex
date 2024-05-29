/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ApiUrlConfig, ControlErrorsComponent, CopyDirective, defined, FormErrorComponent, FormHintComponent, LayoutComponent, ListViewComponent, SidebarMenuDirective, Subscriptions, TeamsState, ToggleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { TeamAuthState, UpdateTeamAuthForm } from '../../internal';

@Component({
    standalone: true,
    selector: 'sqx-auth-page',
    styleUrls: ['./auth-page.component.scss'],
    templateUrl: './auth-page.component.html',
    imports: [
        AsyncPipe,
        ControlErrorsComponent,
        CopyDirective,
        FormErrorComponent,
        FormHintComponent,
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
export class AuthPageComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public urlToRedirect!: string;
    public urlToTest!: string;

    public isEditable = false;
    public isEditing = false;

    public updateForm = new UpdateTeamAuthForm();

    constructor(
        private readonly apiUrl: ApiUrlConfig,
        private readonly authState: TeamAuthState,
        private readonly teamsState: TeamsState,
    ) {
    }

    public ngOnInit() {
        this.subscriptions.add(
            this.teamsState.selectedTeam.pipe(defined())
                .subscribe(team => {
                    this.urlToTest = this.apiUrl.buildUrl('/identity-server/test');
                    this.urlToRedirect = this.apiUrl.buildUrl(`/identity-server/signin-${team.id}`);
                }));

        this.subscriptions.add(
            this.authState.changes.pipe(defined())
                .subscribe(state => {
                    const scheme = state.snapshot.scheme;

                    this.isEditable = !!state.snapshot.canUpdate;
                    this.isEditing = !!scheme;

                    this.updateForm.load(scheme || {});
                    this.updateForm.setEnabled(this.isEditable);
                }));

        this.authState.load();
    }

    public toggle(isEditing: boolean) {
        if (!this.isEditable) {
            return;
        }

        this.isEditing = isEditing;

        if (!isEditing) {
            this.authState.update(null);
        }
    }

    public save() {
        if (!this.isEditable) {
            return;
        }

        const value = this.updateForm.submit();

        if (value) {
            this.authState.update(value)
                .subscribe({
                    next: scheme => {
                        this.updateForm.submitCompleted({ newValue: scheme || {} as any });
                    },
                    error: error => {
                        this.updateForm.submitFailed(error);
                    },
                });
        }
    }
}
