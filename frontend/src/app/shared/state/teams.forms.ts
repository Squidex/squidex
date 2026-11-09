/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { maxLength, required } from '@angular/forms/signals';
import { SignalForm } from '@app/framework';
import { CreateTeamDto, TeamDto, UpdateTeamDto } from '../model';

export type TeamFormModel = { name: string };

export class CreateTeamForm extends SignalForm<TeamFormModel, CreateTeamDto> {
    constructor() {
        super({ name: '' }, path => {
            maxLength(path.name, 40);
            required(path.name);
        });
    }

    protected transformSubmit(value: TeamFormModel) {
        return new CreateTeamDto(value);
    }
}

export class UpdateTeamForm extends SignalForm<TeamFormModel, UpdateTeamDto, TeamDto> {
    constructor() {
        super({ name: '' }, path => {
            maxLength(path.name, 40);
            required(path.name);
        });
    }

    protected transformSubmit(value: TeamFormModel) {
        return new UpdateTeamDto(value);
    }
}
