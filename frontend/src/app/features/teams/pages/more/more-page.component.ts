/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ConfirmClickDirective, ControlErrorsComponent, defined, FormErrorComponent, FormHintComponent, LayoutComponent, ListViewComponent, SidebarMenuDirective, Subscriptions, TeamDto, TeamsState, TooltipDirective, TourStepDirective, TranslatePipe, UpdateTeamForm } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-more-page',
    styleUrls: ['./more-page.component.scss'],
    templateUrl: './more-page.component.html',
    imports: [
        AsyncPipe,
        ConfirmClickDirective,
        ControlErrorsComponent,
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
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class MorePageComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public team!: TeamDto;

    public isEditable = false;
    public isDeletable = false;

    public updateForm = new UpdateTeamForm();

    constructor(
        private readonly router: Router,
        private readonly teamsState: TeamsState,
    ) {
    }

    public ngOnInit() {
        this.subscriptions.add(
            this.teamsState.selectedTeam.pipe(defined())
                .subscribe(team => {
                    this.team = team;

                    this.isEditable = team.canUpdateGeneral;
                    this.isDeletable = team.canDelete;

                    this.updateForm.load(team);
                    this.updateForm.setEnabled(this.isEditable);
                }));

        this.teamsState.reloadTeams();
    }

    public save() {
        if (!this.isEditable) {
            return;
        }

        const value = this.updateForm.submit();

        if (value) {
            this.teamsState.update(this.team, value)
                .subscribe({
                    next: team => {
                        this.updateForm.submitCompleted({ newValue: team });
                    },
                    error: error => {
                        this.updateForm.submitFailed(error);
                    },
                });
        }
    }

    public deleteTeam() {
        if (!this.isDeletable) {
            return;
        }

        this.teamsState.delete(this.team)
            .subscribe(() => {
                this.router.navigate(['/app']);
            });
    }
}
