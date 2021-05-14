/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Form, hasNoValue$, Types, value$ } from '@app/framework';
import { debounceTime, map, shareReplay } from 'rxjs/operators';
import { AssignContributorDto } from './../services/contributors.service';
import { UserDto } from './../services/users.service';

export class AssignContributorForm extends Form<FormGroup, AssignContributorDto> {
    public hasNoUser = hasNoValue$(this.form.controls['user']);

    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            user: [null,
                [
                    Validators.required,
                ],
            ],
            role: [null,
                [
                    Validators.required,
                ],
            ],
        }));
    }

    protected transformSubmit(value: any) {
        let contributorId = value.user;

        if (Types.is(contributorId, UserDto)) {
            contributorId = contributorId.id;
        }

        return { contributorId, role: value.role, invite: true };
    }
}

type ImportContributorsFormType = ReadonlyArray<AssignContributorDto>;

export class ImportContributorsForm extends Form<FormGroup, ImportContributorsFormType> {
    public numberOfEmails = value$(this.form.controls['import']).pipe(debounceTime(100), map(v => extractEmails(v).length), shareReplay(1));

    public hasNoUser = this.numberOfEmails.pipe(map(v => v === 0));

    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            import: ['',
                [
                    Validators.required,
                ],
            ],
        }));
    }

    protected transformSubmit(value: any) {
        return extractEmails(value.import);
    }
}

function extractEmails(value: string) {
    const result: AssignContributorDto[] = [];

    if (value) {
        const added: { [email: string]: boolean } = {};

        const emails = value.match(EMAIL_REGEX);

        if (emails) {
            for (const match of emails) {
                if (!added[match]) {
                    result.push({ contributorId: match, role: 'Editor', invite: true });

                    added[match] = true;
                }
            }
        }
    }

    return result;
}

// eslint-disable-next-line no-useless-escape
const EMAIL_REGEX = /(?=.{1,254}$)(?=.{1,64}@)[-!#$%&'*+\/0-9=?A-Z^_`a-z{|}~]+(\.[-!#$%&'*+\/0-9=?A-Z^_`a-z{|}~]+)*@[A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?(\.[A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?)*/gim;
