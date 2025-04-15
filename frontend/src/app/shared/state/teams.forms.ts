/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable no-useless-escape */

import { UntypedFormControl, Validators } from '@angular/forms';
import { ExtendedFormGroup, Form } from '@app/framework';
import { ICreateTeamDto, IUpdateTeamDto, TeamDto } from '../model';

export class CreateTeamForm extends Form<ExtendedFormGroup, ICreateTeamDto> {
    constructor() {
        super(new ExtendedFormGroup({
            name: new UntypedFormControl('', [
                Validators.required,
                Validators.maxLength(40),
            ]),
        }));
    }
}

export class UpdateTeamForm extends Form<ExtendedFormGroup, IUpdateTeamDto, TeamDto> {
    constructor() {
        super(new ExtendedFormGroup({
            name: new UntypedFormControl('', [
                Validators.required,
                Validators.maxLength(40),
            ]),
        }));
    }
}
