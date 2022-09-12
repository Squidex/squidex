/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Output } from '@angular/core';
import { ApiUrlConfig, CreateTeamForm, TeamsState } from '@app/shared/internal';

@Component({
    selector: 'sqx-team-form',
    styleUrls: ['./team-form.component.scss'],
    templateUrl: './team-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TeamFormComponent {
    @Output()
    public complete = new EventEmitter();

    public createForm = new CreateTeamForm();

    constructor(
        public readonly apiUrl: ApiUrlConfig,
        private readonly teamsStore: TeamsState,
    ) {
    }

    public emitComplete() {
        this.complete.emit();
    }

    public createTeam() {
        const value = this.createForm.submit();

        if (value) {
            this.teamsStore.create(value)
                .subscribe({
                    next: () => {
                        this.emitComplete();
                    },
                    error: error => {
                        this.createForm.submitFailed(error);
                    },
                });
        }
    }
}
