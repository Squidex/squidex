/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { map } from 'rxjs';
import { ApiUrlConfig, CopyDirective, defined, FormErrorComponent, FormHintComponent, FormRowComponent, LayoutComponent, ListViewComponent, SidebarMenuDirective, TeamsState, ToggleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
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
    public readonly urlToRedirect;
    public readonly urlToTest;

    public readonly isEditable = signal(false);
    public readonly isEditing = signal(false);

    public readonly updateForm = new UpdateTeamAuthForm();

    constructor(apiUrl: ApiUrlConfig, teamsState: TeamsState,
        private readonly authState: TeamAuthState,
    ) {
        this.urlToTest = apiUrl.buildUrl('/identity-server/test');

        this.urlToRedirect = toSignal(
            teamsState.selectedTeam.pipe(
                defined(),
                map(team => apiUrl.buildUrl(`/identity-server/signin-${team.id}`))),
            { initialValue: '' });

        effect(onCleanup => {
            const subscription =
                this.authState.changes.pipe(defined())
                    .subscribe(state => {
                        const scheme = state.snapshot.scheme;
                        const isEditable = !!state.snapshot.canUpdate;

                        this.isEditable.set(isEditable);
                        this.isEditing.set(!!scheme);

                        this.updateForm.load(scheme || {});
                        this.updateForm.setEnabled(isEditable);
                    });

            onCleanup(() => subscription.unsubscribe());
        });

        this.authState.load();
    }

    public toggle(isEditing: boolean) {
        if (!this.isEditable()) {
            return;
        }

        this.isEditing.set(isEditing);
        if (!isEditing) {
            this.authState.update(undefined);
        }
    }

    public save() {
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
