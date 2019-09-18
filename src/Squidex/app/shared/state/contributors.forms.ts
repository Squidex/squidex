/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { map } from 'rxjs/operators';

import {
    Form,
    hasNoValue$,
    Types,
    value$
} from '@app/framework';

import { AssignContributorDto } from '../services/contributors.service';

import { UserDto } from './../services/users.service';

export class AssignContributorForm extends Form<FormGroup, AssignContributorDto> {
    public hasNoUser = hasNoValue$(this.form.controls['user']);

    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            user: [null,
                [
                    Validators.required
                ]
            ]
        }));
    }

    protected transformSubmit(value: { user: string | UserDto }) {
        let contributorId = value.user;

        if (Types.is(contributorId, UserDto)) {
            contributorId = contributorId.id;
        }

        return { contributorId, role: 'Editor', invite: true };
    }
}

export class ImportContributorsForm extends Form<FormGroup, AssignContributorDto[]> {
    public numberOfEmails = value$(this.form.controls['import']).pipe(map(v => extractEmails(v).length));

    public hasNoUser = this.numberOfEmails.pipe(map(v => v === 0));

    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            import: ['',
                [
                    Validators.required
                ]
            ]
        }));
    }

    protected transformSubmit(value: { import: string }) {
        return extractEmails(value.import);
    }
}

function extractEmails(value: string) {
    let result: AssignContributorDto[] = [];

    if (value) {
        let emails = value.match(EMAIL_REGEX);

        if (emails) {
            for (let match of emails) {
                result.push({ contributorId: match, role: 'Editor', invite: true });
            }
        }
    }

    return result;
}

const EMAIL_REGEX = /(?=.{1,254}$)(?=.{1,64}@)[-!#$%&'*+\/0-9=?A-Z^_`a-z{|}~]+(\.[-!#$%&'*+\/0-9=?A-Z^_`a-z{|}~]+)*@[A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?(\.[A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?)*/gim;