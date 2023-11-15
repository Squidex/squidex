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
import { ControlErrorsComponent, defined, FormErrorComponent, LayoutComponent, ListViewComponent, SidebarMenuDirective, Subscriptions, TeamDto, TeamsState, TooltipDirective, TourStepDirective, TranslatePipe, UpdateTeamForm } from '@app/shared';

@Component({
    selector: 'sqx-more-page',
    styleUrls: ['./more-page.component.scss'],
    templateUrl: './more-page.component.html',
    standalone: true,
    imports: [
        LayoutComponent,
        ListViewComponent,
        FormsModule,
        ReactiveFormsModule,
        FormErrorComponent,
        ControlErrorsComponent,
        SidebarMenuDirective,
        RouterLink,
        RouterLinkActive,
        TooltipDirective,
        TourStepDirective,
        RouterOutlet,
        AsyncPipe,
        TranslatePipe,
    ],
})
export class MorePageComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public team!: TeamDto;

    public isEditable = false;

    public updateForm = new UpdateTeamForm();

    constructor(
        private readonly teamsState: TeamsState,
    ) {
    }

    public ngOnInit() {
        this.subscriptions.add(
            this.teamsState.selectedTeam.pipe(defined())
                .subscribe(team => {
                    this.team = team;

                    this.isEditable = team.canUpdateGeneral;

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
}
