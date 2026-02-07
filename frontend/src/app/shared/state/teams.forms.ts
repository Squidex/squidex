/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { UntypedFormControl, Validators } from '@angular/forms';
import { ExtendedFormGroup, Form } from '@app/framework';
import { CreateTeamDto, TeamDto, UpdateTeamDto } from '../model';

export class CreateTeamForm extends Form<ExtendedFormGroup, CreateTeamDto> {
    constructor() {
        super(new ExtendedFormGroup({
            name: new UntypedFormControl('', [
                Validators.required,
                Validators.maxLength(40),
            ]),
        }));
    }

    public transformSubmit(value: any) {
        return new CreateTeamDto(value);
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

    public transformSubmit(value: any) {
        return new UpdateTeamDto(value);
    }
}
