/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, effect, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ConfirmClickDirective, FormErrorComponent, FormHintComponent, FormRowComponent, LayoutComponent, ListViewComponent, SidebarMenuDirective, TeamsState, TooltipDirective, TourStepDirective, TranslatePipe, UpdateTeamForm } from '@app/shared';

@Component({
    selector: 'sqx-more-page',
    styleUrls: ['./more-page.component.scss'],
    templateUrl: './more-page.component.html',
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

    protected readonly teamsState = inject(TeamsState);

    protected readonly isEditable = computed(() => {
        return !!this.teamsState.selectedTeam()?.canUpdateGeneral;
    });

    protected readonly isDeletable = computed(() => {
        return !!this.teamsState.selectedTeam()?.canDelete;
    });

    protected readonly updateForm = new UpdateTeamForm();

    constructor() {
        effect(() => {
            const team = this.teamsState.selectedTeam();
            if (!team) {
                return;
            }

            this.updateForm.load(team);
            this.updateForm.setEnabled(team.canUpdateGeneral);
        });

        this.teamsState.reloadTeams();
    }

    protected save() {
        const team = this.teamsState.selectedTeam();
        if (!team || !this.isEditable()) {
            return;
        }

        this.updateForm.submitAndUpdate(value => {
            return this.teamsState.update(team, value);
        }, { resetAfterSubmit: true });
    }

    protected deleteTeam() {
        const team = this.teamsState.selectedTeam();
        if (!team || !this.isDeletable()) {
            return;
        }

        this.teamsState.delete(team)
            .subscribe(() => {
                this.router.navigate(['/app']);
            });
    }
}
