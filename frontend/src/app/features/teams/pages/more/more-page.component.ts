/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, effect, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ConfirmClickDirective, FormErrorComponent, FormHintComponent, FormRowComponent, LayoutComponent, ListViewComponent, SidebarMenuDirective, TeamsState, TooltipDirective, TourStepDirective, TranslatePipe, UpdateTeamForm } from '@app/shared';

@Component({
    selector: 'sqx-more-page',
    styleUrls: ['./more-page.component.scss'],
    templateUrl: './more-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ConfirmClickDirective,
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
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class MorePageComponent {
    private readonly router = inject(Router);
    private readonly teamsState = inject(TeamsState);

    private readonly team;

    protected readonly isEditable = computed(() => {
        return !!this.team()?.canUpdateGeneral;
    });

    protected readonly isDeletable = computed(() => {
        return !!this.team()?.canDelete;
    });

    protected readonly updateForm = new UpdateTeamForm();

    constructor() {
        this.team = this.teamsState.selectedTeam;

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

    protected save() {
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

    protected deleteTeam() {
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
