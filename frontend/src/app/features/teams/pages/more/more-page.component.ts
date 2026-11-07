/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ConfirmClickDirective, defined, FormErrorComponent, FormHintComponent, FormRowComponent, LayoutComponent, ListViewComponent, SidebarMenuDirective, TeamsState, TooltipDirective, TourStepDirective, TranslatePipe, UpdateTeamForm } from '@app/shared';

@Component({
    selector: 'sqx-more-page',
    styleUrls: ['./more-page.component.scss'],
    templateUrl: './more-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        ConfirmClickDirective,
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
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class MorePageComponent {
    public readonly team;

    public readonly isEditable = computed(() => {
        return !!this.team()?.canUpdateGeneral;
    });

    public readonly isDeletable = computed(() => {
        return !!this.team()?.canDelete;
    });

    public readonly updateForm = new UpdateTeamForm();

    constructor(
        private readonly router: Router,
        private readonly teamsState: TeamsState,
    ) {
        this.team = toSignal(this.teamsState.selectedTeam.pipe(defined()));

        effect(() => {
            const team = this.team();
            if (!team) {
                return;
            }

            this.updateForm.load(team);
            this.updateForm.setEnabled(team.canUpdateGeneral);
        });

        this.teamsState.reloadTeams();
    }

    public save() {
        const team = this.team();
        if (!team || !this.isEditable()) {
            return;
        }

        const value = this.updateForm.submit();
        if (!value) {
            return;
        }

        this.teamsState.update(team, value)
            .subscribe({
                next: team => {
                    this.updateForm.submitCompleted({ newValue: team });
                },
                error: error => {
                    this.updateForm.submitFailed(error);
                },
            });
    }

    public deleteTeam() {
        const team = this.team();
        if (!team || !this.isDeletable()) {
            return;
        }

        this.teamsState.delete(team)
            .subscribe(() => {
                this.router.navigate(['/app']);
            });
    }
}
