/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable no-useless-escape */

import { UntypedFormControl, Validators } from '@angular/forms';
import { ExtendedFormGroup, Form } from '@app/framework';
import { CreateTeamDto, TeamDto, UpdateTeamDto } from './../services/teams.service';

export class CreateTeamForm extends Form<ExtendedFormGroup, CreateTeamDto> {
    constructor() {
        super(new ExtendedFormGroup({
            name: new UntypedFormControl('', [
                Validators.required,
                Validators.maxLength(40),
            ]),
        }));
    }
}

export class UpdateTeamForm extends Form<ExtendedFormGroup, UpdateTeamDto, TeamDto> {
    constructor() {
        super(new ExtendedFormGroup({
            name: new UntypedFormControl('', [
                Validators.required,
                Validators.maxLength(40),
            ]),
        }));
    }
}
